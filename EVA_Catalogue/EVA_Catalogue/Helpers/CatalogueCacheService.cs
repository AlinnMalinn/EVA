using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace EVA_Catalogue
{
    /// <summary>
    /// Excel is the source of truth. JSON files are an internal, disposable cache.
    /// </summary>
    public sealed class CatalogueCacheService : IDisposable
    {
        private const int CacheSchemaVersion = 2;
        private static readonly Lazy<CatalogueCacheService> LazyInstance =
            new Lazy<CatalogueCacheService>(() => new CatalogueCacheService());

        private readonly object syncRoot = new object();
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        private readonly Dictionary<string, CatalogueCacheFile> memoryCache =
            new Dictionary<string, CatalogueCacheFile>(StringComparer.OrdinalIgnoreCase);
        private readonly string cacheDirectory;
        private readonly string manifestPath;
        private CatalogueCacheManifest manifest;
        private FileSystemWatcher watcher;
        private Timer refreshTimer;
        private string sourceDirectory;

        public static CatalogueCacheService Instance => LazyInstance.Value;

        private CatalogueCacheService()
        {
            cacheDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EVA", "CatalogueCache");
            Directory.CreateDirectory(cacheDirectory);
            manifestPath = Path.Combine(cacheDirectory, "manifest.json");
            manifest = ReadManifest();
        }

        public string CacheDirectory => cacheDirectory;

        public bool HasCatalogueFiles()
        {
            string directory;
            lock (syncRoot)
                directory = sourceDirectory;
            return !string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory) &&
                EnumerateCatalogueFiles(directory).Any();
        }

        public void ConfigureDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                return;

            string fullPath = Path.GetFullPath(directory);
            lock (syncRoot)
            {
                if (string.Equals(sourceDirectory, fullPath, StringComparison.OrdinalIgnoreCase))
                    return;

                sourceDirectory = fullPath;
                memoryCache.Clear();
                watcher?.Dispose();
                watcher = new FileSystemWatcher(sourceDirectory)
                {
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };
                watcher.Changed += CatalogueFolderChanged;
                watcher.Created += CatalogueFolderChanged;
                watcher.Deleted += CatalogueFolderChanged;
                watcher.Renamed += CatalogueFolderChanged;
            }
        }

        public int RefreshAll(bool force = false)
        {
            string directory;
            lock (syncRoot)
                directory = sourceDirectory;

            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                return 0;

            int refreshed = 0;
            foreach (string excelPath in EnumerateCatalogueFiles(directory))
            {
                if (EnsureCache(excelPath, force))
                    refreshed++;
            }

            RemoveDeletedSources(directory);
            return refreshed;
        }

        public IReadOnlyList<string> GetProducerNames(string requiredSheetName = null)
        {
            RefreshAll();
            string directory;
            lock (syncRoot)
                directory = sourceDirectory;

            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                return new List<string>();

            return EnumerateCatalogueFiles(directory)
                .Where(path => HasRequiredSheet(path, requiredSheetName))
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        private bool HasRequiredSheet(string excelPath, string requiredSheetName)
        {
            if (string.IsNullOrWhiteSpace(requiredSheetName))
                return true;

            EnsureCache(excelPath, false);
            CatalogueManifestEntry entry;
            lock (syncRoot)
                entry = FindManifestEntry(Path.GetFullPath(excelPath));
            if (entry == null)
                return false;

            if (string.Equals(requiredSheetName, "QF", StringComparison.OrdinalIgnoreCase))
                return entry.HasQfSheet;
            if (string.Equals(requiredSheetName, "QFD", StringComparison.OrdinalIgnoreCase))
                return entry.HasQfdSheet;
            return false;
        }

        public DataSet GetSeries(string producer, bool residualCurrent)
        {
            CatalogueCacheFile catalogue = GetCatalogue(producer);
            IEnumerable<DeviceCacheItem> devices = residualCurrent
                ? catalogue.ResidualCurrentCircuitBreakers
                : catalogue.ModularCircuitBreakers;

            DataTable table = CreateDeviceTable();
            foreach (string series in devices.Select(x => x.SeriesName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.CurrentCultureIgnoreCase))
            {
                DataRow row = table.NewRow();
                row["SeriesName"] = series;
                table.Rows.Add(row);
            }
            return ToDataSet(table);
        }

        public DataSet FindDevices(string producer, bool residualCurrent, string seriesName,
            object ratedCurrent, object numberOfPoles, object responseCharacteristics,
            object maximumBreakingCapacity, object thermalOverloadRelease,
            object leakageCurrent = null, object residualCurrentType = null)
        {
            CatalogueCacheFile catalogue = GetCatalogue(producer);
            IEnumerable<DeviceCacheItem> query = residualCurrent
                ? catalogue.ResidualCurrentCircuitBreakers
                : catalogue.ModularCircuitBreakers;

            double rated = ToDouble(ratedCurrent);
            double minimumCapacity = ToDouble(maximumBreakingCapacity);
            int poles = ToInt(numberOfPoles);
            int thermal = ToInt(thermalOverloadRelease);

            query = query.Where(item =>
                NearlyEqual(item.RatedCurrent, rated) &&
                item.NumberOfPoles == poles &&
                TextEquals(item.ResponseCharacteristics, responseCharacteristics) &&
                item.ThermalOverloadRelease == thermal &&
                item.MaximumBreakingCapacity >= minimumCapacity);

            if (!string.IsNullOrWhiteSpace(seriesName))
                query = query.Where(item => (item.SeriesName ?? string.Empty)
                    .IndexOf(seriesName, StringComparison.CurrentCultureIgnoreCase) >= 0);

            if (residualCurrent)
            {
                double leakage = ToDouble(leakageCurrent);
                query = query.Where(item => NearlyEqual(item.LeakageCurrent, leakage) &&
                    TextEquals(item.ResidualCurrentType, residualCurrentType));
            }

            DataTable table = CreateDeviceTable();
            foreach (DeviceCacheItem item in query.OrderBy(x => x.MaximumBreakingCapacity))
                AddDeviceRow(table, item);
            return ToDataSet(table);
        }

        private CatalogueCacheFile GetCatalogue(string producer)
        {
            string excelPath = FindCataloguePath(producer);
            if (excelPath == null)
                return new CatalogueCacheFile();

            EnsureCache(excelPath, false);
            lock (syncRoot)
            {
                if (memoryCache.TryGetValue(excelPath, out CatalogueCacheFile cached))
                    return cached;
            }

            CatalogueCacheFile loaded = ReadCache(GetCachePath(excelPath));
            lock (syncRoot)
                memoryCache[excelPath] = loaded;
            return loaded;
        }

        private bool EnsureCache(string excelPath, bool force)
        {
            FileInfo source = new FileInfo(excelPath);
            if (!source.Exists)
                return false;

            string cachePath = GetCachePath(excelPath);
            CatalogueManifestEntry existingEntry;
            lock (syncRoot)
                existingEntry = FindManifestEntry(source.FullName);

            if (!force && File.Exists(cachePath) && existingEntry != null)
            {
                if (existingEntry.SchemaVersion == CacheSchemaVersion &&
                    string.Equals(existingEntry.SourcePath, source.FullName, StringComparison.OrdinalIgnoreCase) &&
                    existingEntry.SourceLength == source.Length &&
                    existingEntry.SourceLastWriteUtcTicks == source.LastWriteTimeUtc.Ticks)
                {
                    return false;
                }
            }

            try
            {
                CatalogueCacheFile imported = ImportExcel(source);
                WriteCacheAtomically(cachePath, imported);
                lock (syncRoot)
                {
                    memoryCache[source.FullName] = imported;
                    UpsertManifestEntry(imported, cachePath);
                    WriteManifest();
                }
                return true;
            }
            catch
            {
                // A file being saved by Excel may be temporarily locked. Keep the last valid cache.
                if (existingEntry != null && File.Exists(cachePath))
                    return false;
                throw;
            }
        }

        private CatalogueCacheFile ImportExcel(FileInfo source)
        {
            CatalogueCacheFile result = new CatalogueCacheFile
            {
                SchemaVersion = CacheSchemaVersion,
                SourcePath = source.FullName,
                SourceLength = source.Length,
                SourceLastWriteUtcTicks = source.LastWriteTimeUtc.Ticks,
                ImportedAtUtcTicks = DateTime.UtcNow.Ticks
            };

            using (XLWorkbook workbook = new XLWorkbook(source.FullName))
            {
                foreach (IXLWorksheet sheet in workbook.Worksheets)
                {
                    bool isQf = string.Equals(sheet.Name, "QF", StringComparison.OrdinalIgnoreCase);
                    bool isQfd = string.Equals(sheet.Name, "QFD", StringComparison.OrdinalIgnoreCase);
                    if (!isQf && !isQfd)
                        continue;

                    if (isQf)
                        result.HasQfSheet = true;
                    if (isQfd)
                        result.HasQfdSheet = true;

                    IXLRow lastRow = sheet.LastRowUsed();
                    if (lastRow == null)
                        continue;

                    for (int row = 2; row <= lastRow.RowNumber(); row++)
                    {
                        if (sheet.Cell(row, 1).IsEmpty() && sheet.Cell(row, 2).IsEmpty())
                            continue;

                        DeviceCacheItem item = isQf
                            ? ReadQfRow(sheet, row)
                            : ReadQfdRow(sheet, row);
                        if (isQf)
                            result.ModularCircuitBreakers.Add(item);
                        else
                            result.ResidualCurrentCircuitBreakers.Add(item);
                    }
                }
            }
            return result;
        }

        private static DeviceCacheItem ReadQfRow(IXLWorksheet sheet, int row)
        {
            return new DeviceCacheItem
            {
                SeriesName = CellText(sheet, row, 1), Name = CellText(sheet, row, 2),
                NumberOfPoles = ToInt(CellText(sheet, row, 3)), RatedCurrent = ToDouble(CellText(sheet, row, 4)),
                ResponseCharacteristics = CellText(sheet, row, 5), MaximumBreakingCapacity = ToDouble(CellText(sheet, row, 6)),
                ThermalOverloadRelease = ToInt(CellText(sheet, row, 7)), Code = CellText(sheet, row, 8),
                Mark = CellText(sheet, row, 9)
            };
        }

        private static DeviceCacheItem ReadQfdRow(IXLWorksheet sheet, int row)
        {
            return new DeviceCacheItem
            {
                SeriesName = CellText(sheet, row, 1), Name = CellText(sheet, row, 2),
                NumberOfPoles = ToInt(CellText(sheet, row, 3)), RatedCurrent = ToDouble(CellText(sheet, row, 4)),
                LeakageCurrent = ToDouble(CellText(sheet, row, 5)), ResponseCharacteristics = CellText(sheet, row, 6),
                MaximumBreakingCapacity = ToDouble(CellText(sheet, row, 7)), ThermalOverloadRelease = ToInt(CellText(sheet, row, 8)),
                Code = CellText(sheet, row, 9), Mark = CellText(sheet, row, 10), ResidualCurrentType = CellText(sheet, row, 11)
            };
        }

        private static string CellText(IXLWorksheet sheet, int row, int column) =>
            sheet.Cell(row, column).GetValue<string>().Trim();

        private string FindCataloguePath(string producer)
        {
            string directory;
            lock (syncRoot)
                directory = sourceDirectory;
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                return null;

            return EnumerateCatalogueFiles(directory).FirstOrDefault(path =>
                string.Equals(Path.GetFileNameWithoutExtension(path), producer, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<string> EnumerateCatalogueFiles(string directory)
        {
            return Directory.EnumerateFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
                .Where(path => !Path.GetFileName(path).StartsWith("~$", StringComparison.Ordinal))
                .Where(path => string.Equals(Path.GetExtension(path), ".xlsx", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(Path.GetExtension(path), ".xlsm", StringComparison.OrdinalIgnoreCase));
        }

        private string GetCachePath(string sourcePath)
        {
            string hash;
            using (SHA256 sha = SHA256.Create())
                hash = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(Path.GetFullPath(sourcePath))))
                    .Replace("-", string.Empty).Substring(0, 12);
            string safeName = string.Concat(Path.GetFileNameWithoutExtension(sourcePath)
                .Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
            return Path.Combine(cacheDirectory, safeName + "_" + hash + ".json");
        }

        private CatalogueCacheFile ReadCache(string path)
        {
            return serializer.Deserialize<CatalogueCacheFile>(File.ReadAllText(path, Encoding.UTF8))
                ?? new CatalogueCacheFile();
        }

        private CatalogueCacheManifest ReadManifest()
        {
            if (!File.Exists(manifestPath))
                return new CatalogueCacheManifest();
            try
            {
                return serializer.Deserialize<CatalogueCacheManifest>(
                    File.ReadAllText(manifestPath, Encoding.UTF8)) ?? new CatalogueCacheManifest();
            }
            catch
            {
                return new CatalogueCacheManifest();
            }
        }

        private CatalogueManifestEntry FindManifestEntry(string sourcePath)
        {
            return manifest.Catalogues.FirstOrDefault(item =>
                string.Equals(item.SourcePath, sourcePath, StringComparison.OrdinalIgnoreCase));
        }

        private void UpsertManifestEntry(CatalogueCacheFile cache, string cachePath)
        {
            CatalogueManifestEntry entry = FindManifestEntry(cache.SourcePath);
            if (entry == null)
            {
                entry = new CatalogueManifestEntry();
                manifest.Catalogues.Add(entry);
            }
            entry.SchemaVersion = cache.SchemaVersion;
            entry.SourcePath = cache.SourcePath;
            entry.SourceLength = cache.SourceLength;
            entry.SourceLastWriteUtcTicks = cache.SourceLastWriteUtcTicks;
            entry.ImportedAtUtcTicks = cache.ImportedAtUtcTicks;
            entry.CacheFile = Path.GetFileName(cachePath);
            entry.HasQfSheet = cache.HasQfSheet;
            entry.HasQfdSheet = cache.HasQfdSheet;
        }

        private void WriteManifest()
        {
            File.WriteAllText(manifestPath, serializer.Serialize(manifest), new UTF8Encoding(false));
        }

        private void WriteCacheAtomically(string path, CatalogueCacheFile data)
        {
            string temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, serializer.Serialize(data), new UTF8Encoding(false));
            if (File.Exists(path))
                File.Replace(temporaryPath, path, null);
            else
                File.Move(temporaryPath, path);
        }

        private void RemoveDeletedSources(string directory)
        {
            HashSet<string> existing = new HashSet<string>(EnumerateCatalogueFiles(directory)
                .Select(Path.GetFullPath), StringComparer.OrdinalIgnoreCase);
            lock (syncRoot)
            {
                foreach (string path in memoryCache.Keys.Where(path => !existing.Contains(path)).ToList())
                    memoryCache.Remove(path);
                int removed = manifest.Catalogues.RemoveAll(item =>
                    string.Equals(Path.GetDirectoryName(item.SourcePath), directory, StringComparison.OrdinalIgnoreCase) &&
                    !existing.Contains(item.SourcePath));
                if (removed > 0)
                    WriteManifest();
            }
        }

        private void CatalogueFolderChanged(object sender, FileSystemEventArgs e)
        {
            string extension = Path.GetExtension(e.FullPath);
            if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(extension, ".xlsm", StringComparison.OrdinalIgnoreCase))
                return;
            if (Path.GetFileName(e.FullPath).StartsWith("~$", StringComparison.Ordinal))
                return;

            lock (syncRoot)
            {
                refreshTimer?.Dispose();
                refreshTimer = new Timer(_ =>
                {
                    try { RefreshAll(); }
                    catch { /* The next access or manual refresh retries the import. */ }
                }, null, 1500, Timeout.Infinite);
            }
        }

        private static DataTable CreateDeviceTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("id", typeof(int)); table.Columns.Add("SeriesName", typeof(string));
            table.Columns.Add("NameD", typeof(string)); table.Columns.Add("NumberOfPoles", typeof(int));
            table.Columns.Add("RatedСurrent", typeof(double)); table.Columns.Add("LeakageСurrent", typeof(double));
            table.Columns.Add("ResponseCharacteristics", typeof(string)); table.Columns.Add("MaximumBreakingCapacity", typeof(double));
            table.Columns.Add("ThermalOverloadRelease", typeof(int)); table.Columns.Add("Code", typeof(string));
            table.Columns.Add("Mark", typeof(string)); table.Columns.Add("ResidualCurrentType", typeof(string));
            return table;
        }

        private static void AddDeviceRow(DataTable table, DeviceCacheItem item)
        {
            DataRow row = table.NewRow();
            row["id"] = table.Rows.Count + 1; row["SeriesName"] = item.SeriesName ?? string.Empty;
            row["NameD"] = item.Name ?? string.Empty; row["NumberOfPoles"] = item.NumberOfPoles;
            row["RatedСurrent"] = item.RatedCurrent; row["LeakageСurrent"] = item.LeakageCurrent;
            row["ResponseCharacteristics"] = item.ResponseCharacteristics ?? string.Empty;
            row["MaximumBreakingCapacity"] = item.MaximumBreakingCapacity;
            row["ThermalOverloadRelease"] = item.ThermalOverloadRelease; row["Code"] = item.Code ?? string.Empty;
            row["Mark"] = item.Mark ?? string.Empty; row["ResidualCurrentType"] = item.ResidualCurrentType ?? string.Empty;
            table.Rows.Add(row);
        }

        private static DataSet ToDataSet(DataTable table)
        {
            DataSet result = new DataSet(); result.Tables.Add(table); return result;
        }

        private static bool TextEquals(string left, object right) =>
            string.Equals(left ?? string.Empty, Convert.ToString(right, CultureInfo.CurrentCulture) ?? string.Empty,
                StringComparison.CurrentCultureIgnoreCase);
        private static bool NearlyEqual(double left, double right) => Math.Abs(left - right) < 0.000001;
        private static int ToInt(object value) => (int)Math.Round(ToDouble(value));
        private static double ToDouble(object value)
        {
            string text = Convert.ToString(value, CultureInfo.CurrentCulture)?.Trim();
            if (string.IsNullOrEmpty(text)) return 0;
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out double current)) return current;
            if (double.TryParse(text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double invariant)) return invariant;
            return 0;
        }

        public void Dispose()
        {
            watcher?.Dispose(); refreshTimer?.Dispose();
        }
    }

    public sealed class CatalogueCacheFile
    {
        public int SchemaVersion { get; set; }
        public string SourcePath { get; set; }
        public long SourceLength { get; set; }
        public long SourceLastWriteUtcTicks { get; set; }
        public long ImportedAtUtcTicks { get; set; }
        public bool HasQfSheet { get; set; }
        public bool HasQfdSheet { get; set; }
        public List<DeviceCacheItem> ModularCircuitBreakers { get; set; } = new List<DeviceCacheItem>();
        public List<DeviceCacheItem> ResidualCurrentCircuitBreakers { get; set; } = new List<DeviceCacheItem>();
    }

    public sealed class CatalogueCacheManifest
    {
        public List<CatalogueManifestEntry> Catalogues { get; set; } = new List<CatalogueManifestEntry>();
    }

    public sealed class CatalogueManifestEntry
    {
        public int SchemaVersion { get; set; }
        public string SourcePath { get; set; }
        public long SourceLength { get; set; }
        public long SourceLastWriteUtcTicks { get; set; }
        public long ImportedAtUtcTicks { get; set; }
        public string CacheFile { get; set; }
        public bool HasQfSheet { get; set; }
        public bool HasQfdSheet { get; set; }
    }

    public sealed class DeviceCacheItem
    {
        public string SeriesName { get; set; }
        public string Name { get; set; }
        public int NumberOfPoles { get; set; }
        public double RatedCurrent { get; set; }
        public double LeakageCurrent { get; set; }
        public string ResponseCharacteristics { get; set; }
        public double MaximumBreakingCapacity { get; set; }
        public int ThermalOverloadRelease { get; set; }
        public string Code { get; set; }
        public string Mark { get; set; }
        public string ResidualCurrentType { get; set; }
    }
}
