using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace EVA_Settings
{
    public sealed class SettingsProfileService
    {
        public const string AutomaticMode = "automatic";
        public const string ManualMode = "manual";
        private const int CurrentSchemaVersion = 1;
        private static readonly Lazy<SettingsProfileService> LazyInstance =
            new Lazy<SettingsProfileService>(() => new SettingsProfileService());
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        private readonly string settingsRoot;

        public static SettingsProfileService Instance => LazyInstance.Value;

        private SettingsProfileService()
        {
            settingsRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EVA", "Settings");
            Directory.CreateDirectory(settingsRoot);
        }

        public SettingsProfile Load(string mode, string workbookFullName, string workbookName, string legacyPath = null)
        {
            ValidateMode(mode);
            MigrateLegacyProfiles(workbookFullName, workbookName, legacyPath);
            string path = GetProfilePath(mode, workbookFullName, workbookName);
            if (!File.Exists(path))
                return CreateEmpty(mode, workbookName);
            try
            {
                SettingsProfile profile = serializer.Deserialize<SettingsProfile>(File.ReadAllText(path, Encoding.UTF8));
                ValidateProfile(profile, mode);
                return profile;
            }
            catch
            {
                return CreateEmpty(mode, workbookName);
            }
        }

        public void Save(SettingsProfile profile, string workbookFullName, string workbookName)
        {
            ValidateProfile(profile, profile.Mode);
            profile.WorkbookName = workbookName;
            string path = GetProfilePath(profile.Mode, workbookFullName, workbookName);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, serializer.Serialize(profile), new UTF8Encoding(false));
        }

        public DeviceSelectionSettings GetSelection(string mode, string deviceType,
            string workbookFullName, string workbookName, string legacyPath = null)
        {
            SettingsProfile profile = Load(mode, workbookFullName, workbookName, legacyPath);
            if (!profile.DeviceTypes.TryGetValue(NormalizeDeviceType(deviceType), out DeviceSelectionSettings selection))
                return new DeviceSelectionSettings();
            return selection;
        }

        public void SaveSelection(string mode, string deviceType, IEnumerable<string> producers,
            IEnumerable<string> series, string workbookFullName, string workbookName, string legacyPath = null)
        {
            SettingsProfile profile = Load(mode, workbookFullName, workbookName, legacyPath);
            profile.DeviceTypes[NormalizeDeviceType(deviceType)] = new DeviceSelectionSettings
            {
                Producers = Clean(producers),
                Series = Clean(series)
            };
            Save(profile, workbookFullName, workbookName);
        }

        public void ResetSelection(string mode, string deviceType, string workbookFullName,
            string workbookName, string legacyPath = null)
        {
            SettingsProfile profile = Load(mode, workbookFullName, workbookName, legacyPath);
            profile.DeviceTypes.Remove(NormalizeDeviceType(deviceType));
            Save(profile, workbookFullName, workbookName);
        }

        public void Export(string mode, string destinationPath, string workbookFullName,
            string workbookName, string legacyPath = null)
        {
            SettingsProfile profile = Load(mode, workbookFullName, workbookName, legacyPath);
            File.WriteAllText(destinationPath, serializer.Serialize(profile), new UTF8Encoding(false));
        }

        public void Import(string expectedMode, string sourcePath, string workbookFullName, string workbookName)
        {
            SettingsProfile imported = serializer.Deserialize<SettingsProfile>(File.ReadAllText(sourcePath, Encoding.UTF8));
            ValidateProfile(imported, expectedMode);
            imported.WorkbookName = workbookName;
            string currentPath = GetProfilePath(expectedMode, workbookFullName, workbookName);
            if (File.Exists(currentPath))
            {
                string backupPath = currentPath + ".backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
                File.Copy(currentPath, backupPath, false);
            }
            Save(imported, workbookFullName, workbookName);
        }

        public AutomaticSelectionState LoadAutomaticSelectionState(string workbookFullName, string workbookName)
        {
            string path = GetAutomaticSelectionStatePath(workbookFullName, workbookName);
            if (!File.Exists(path))
                return new AutomaticSelectionState();
            try
            {
                return serializer.Deserialize<AutomaticSelectionState>(
                    File.ReadAllText(path, Encoding.UTF8)) ?? new AutomaticSelectionState();
            }
            catch
            {
                return new AutomaticSelectionState();
            }
        }

        public void SaveAutomaticSelectionState(AutomaticSelectionState state,
            string workbookFullName, string workbookName)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            string path = GetAutomaticSelectionStatePath(workbookFullName, workbookName);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, serializer.Serialize(state), new UTF8Encoding(false));
        }

        private void MigrateLegacyProfiles(string workbookFullName, string workbookName, string legacyPath)
        {
            if (string.IsNullOrWhiteSpace(legacyPath) || !File.Exists(legacyPath))
                return;
            string automaticPath = GetProfilePath(AutomaticMode, workbookFullName, workbookName);
            string manualPath = GetProfilePath(ManualMode, workbookFullName, workbookName);
            if (File.Exists(automaticPath) && File.Exists(manualPath))
                return;

            SettingsProfile automatic = CreateEmpty(AutomaticMode, workbookName);
            SettingsProfile manual = CreateEmpty(ManualMode, workbookName);
            foreach (string line in File.ReadAllLines(legacyPath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split('%');
                if (parts.Length < 3) continue;
                bool isManual = parts[0].StartsWith("*", StringComparison.Ordinal);
                SettingsProfile target = isManual ? manual : automatic;
                target.DeviceTypes[NormalizeDeviceType(parts[0])] = new DeviceSelectionSettings
                {
                    Producers = SplitValues(parts[1]),
                    Series = SplitValues(parts[2])
                };
            }
            if (!File.Exists(automaticPath) && automatic.DeviceTypes.Count > 0)
                Save(automatic, workbookFullName, workbookName);
            if (!File.Exists(manualPath) && manual.DeviceTypes.Count > 0)
                Save(manual, workbookFullName, workbookName);
        }

        private string GetProfilePath(string mode, string workbookFullName, string workbookName)
        {
            string identity = string.IsNullOrWhiteSpace(workbookFullName) ? workbookName : workbookFullName;
            string hash;
            using (SHA256 sha = SHA256.Create())
                hash = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(identity ?? string.Empty)))
                    .Replace("-", string.Empty).Substring(0, 12);
            string safeName = string.Concat((Path.GetFileNameWithoutExtension(workbookName) ?? "Workbook")
                .Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
            return Path.Combine(settingsRoot, safeName + "_" + hash, mode + ".json");
        }

        private string GetAutomaticSelectionStatePath(string workbookFullName, string workbookName)
        {
            string automaticProfilePath = GetProfilePath(AutomaticMode, workbookFullName, workbookName);
            return Path.Combine(Path.GetDirectoryName(automaticProfilePath), "automatic-state.json");
        }

        private static SettingsProfile CreateEmpty(string mode, string workbookName) => new SettingsProfile
        {
            SchemaVersion = CurrentSchemaVersion,
            Mode = mode,
            WorkbookName = workbookName
        };

        private static void ValidateProfile(SettingsProfile profile, string expectedMode)
        {
            if (profile == null) throw new InvalidDataException("Файл настроек пуст или повреждён.");
            if (profile.SchemaVersion != CurrentSchemaVersion) throw new InvalidDataException("Версия файла настроек не поддерживается.");
            ValidateMode(expectedMode);
            if (!string.Equals(profile.Mode, expectedMode, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Файл предназначен для другого режима подбора.");
            if (profile.DeviceTypes == null) profile.DeviceTypes = new Dictionary<string, DeviceSelectionSettings>();
        }

        private static void ValidateMode(string mode)
        {
            if (mode != AutomaticMode && mode != ManualMode)
                throw new ArgumentException("Неизвестный режим настроек.", nameof(mode));
        }

        private static string NormalizeDeviceType(string value) => (value ?? string.Empty).TrimStart('*');
        private static List<string> SplitValues(string value) => Clean((value ?? string.Empty).Split('#'));
        private static List<string> Clean(IEnumerable<string> values) => (values ?? Enumerable.Empty<string>())
            .Where(value => !string.IsNullOrWhiteSpace(value) && value != "%")
            .Select(value => value.Trim()).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
    }

    public sealed class SettingsProfile
    {
        public int SchemaVersion { get; set; } = 1;
        public string Mode { get; set; }
        public string WorkbookName { get; set; }
        public Dictionary<string, DeviceSelectionSettings> DeviceTypes { get; set; } =
            new Dictionary<string, DeviceSelectionSettings>();
    }

    public sealed class DeviceSelectionSettings
    {
        public List<string> Producers { get; set; } = new List<string>();
        public List<string> Series { get; set; } = new List<string>();
    }

    public sealed class AutomaticSelectionState
    {
        public bool ModularCircuitBreakersEnabled { get; set; }
        public bool ModularResidualCurrentCircuitBreakersEnabled { get; set; }
        public bool SelectAllPanels { get; set; } = true;
    }
}
