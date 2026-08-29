using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EVA_Catalogue_Shared;
using Excel = Microsoft.Office.Interop.Excel;

namespace EVA_Settings
{
    public sealed class SettingsProfileService
    {
        public const string AutomaticMode = "automatic";
        public const string ManualMode = "manual";
        public const string ModularCircuitBreakersDeviceType = "ModularCircuitBreakers";
        public const string ModularResidualCurrentCircuitBreakersDeviceType = "ModularResidualCurrentCircuitBreakers";
        private const int CurrentSchemaVersion = 1;
        private const string SettingsSheetName = "списки1";
        private const int HeaderRow = 201;
        private const int DataStartRow = 202;
        private const int DataColumnCount = 5;
        private const int MaximumRecordCount = 10000;
        private const string StorageMarker = "EVA_SELECTION_SETTINGS";
        private const string ProducerRecord = "producer";
        private const string SeriesRecord = "series";
        private const string StateMode = "automatic-state";
        private const string StateDeviceType = "MainWindow";
        private const string StateRecord = "state";

        private static readonly Lazy<SettingsProfileService> LazyInstance =
            new Lazy<SettingsProfileService>(() => new SettingsProfileService());
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        private readonly string legacySettingsRoot;

        public static SettingsProfileService Instance => LazyInstance.Value;

        private SettingsProfileService()
        {
            legacySettingsRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EVA", "Settings");
        }

        public SettingsProfile Load(string mode, string workbookFullName, string workbookName, string legacyPath = null)
        {
            ValidateMode(mode);
            WorkbookSettingsData data = LoadWorkbookData(workbookFullName, workbookName, legacyPath);
            return data.Profiles.TryGetValue(mode, out SettingsProfile profile)
                ? profile
                : CreateEmpty(mode, workbookName);
        }

        public void Save(SettingsProfile profile, string workbookFullName, string workbookName)
        {
            ValidateProfile(profile, profile.Mode);
            profile.WorkbookName = workbookName;
            WorkbookSettingsData data = LoadWorkbookData(workbookFullName, workbookName, null);
            data.Profiles[profile.Mode] = profile;
            WriteWorkbookData(data);
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
            Import(expectedMode, sourcePath, workbookFullName, workbookName, null);
        }

        public List<string> GetImportDeviceTypes(string expectedMode, string sourcePath)
        {
            SettingsProfile imported = serializer.Deserialize<SettingsProfile>(File.ReadAllText(sourcePath, Encoding.UTF8));
            ValidateProfile(imported, expectedMode);
            return imported.DeviceTypes.Keys.Select(NormalizeDeviceType)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(value => value, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        public void Import(string expectedMode, string sourcePath, string workbookFullName, string workbookName,
            IEnumerable<string> deviceTypes)
        {
            SettingsProfile imported = serializer.Deserialize<SettingsProfile>(File.ReadAllText(sourcePath, Encoding.UTF8));
            ValidateProfile(imported, expectedMode);
            imported.WorkbookName = workbookName;
            WorkbookSettingsData data = LoadWorkbookData(workbookFullName, workbookName, null);
            if (deviceTypes == null)
            {
                data.Profiles[expectedMode] = imported;
            }
            else
            {
                SettingsProfile target = data.Profiles[expectedMode];
                foreach (string deviceType in deviceTypes.Select(NormalizeDeviceType).Distinct())
                {
                    KeyValuePair<string, DeviceSelectionSettings> importedDevice = imported.DeviceTypes
                        .FirstOrDefault(item => string.Equals(NormalizeDeviceType(item.Key), deviceType,
                            StringComparison.CurrentCultureIgnoreCase));
                    if (!string.IsNullOrEmpty(importedDevice.Key))
                    {
                        DeviceSelectionSettings selection = importedDevice.Value;
                        target.DeviceTypes[deviceType] = new DeviceSelectionSettings
                        {
                            Producers = Clean(selection.Producers),
                            Series = Clean(selection.Series)
                        };
                    }
                    else
                    {
                        target.DeviceTypes.Remove(deviceType);
                    }
                }
            }
            WriteWorkbookData(data);
        }

        public AutomaticSelectionState LoadAutomaticSelectionState(string workbookFullName, string workbookName)
        {
            WorkbookSettingsData data = LoadWorkbookData(workbookFullName, workbookName, null);
            return data.HasAutomaticState ? data.AutomaticState : new AutomaticSelectionState();
        }

        public void SaveAutomaticSelectionState(AutomaticSelectionState state,
            string workbookFullName, string workbookName)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            WorkbookSettingsData data = LoadWorkbookData(workbookFullName, workbookName, null);
            data.AutomaticState = state;
            data.HasAutomaticState = true;
            WriteWorkbookData(data);
        }

        private WorkbookSettingsData LoadWorkbookData(string workbookFullName, string workbookName, string legacyPath)
        {
            Excel.Worksheet worksheet = GetSettingsWorksheet();
            string marker = Convert.ToString(((Excel.Range)worksheet.Cells[HeaderRow, 1]).Value2);
            if (string.Equals(marker, StorageMarker, StringComparison.Ordinal))
            {
                ValidateWorkbookSchema(worksheet);
                return ReadWorkbookData(worksheet, workbookName);
            }

            WorkbookSettingsData migrated = ReadLegacyData(workbookFullName, workbookName, legacyPath);
            if (migrated.HasLegacyData)
                WriteWorkbookData(migrated, worksheet);
            return migrated;
        }

        private WorkbookSettingsData ReadWorkbookData(Excel.Worksheet worksheet, string workbookName)
        {
            var data = new WorkbookSettingsData(workbookName);
            int recordCount = ReadRecordCount(worksheet);
            if (recordCount == 0)
                return data;

            Excel.Range range = worksheet.Range[
                worksheet.Cells[DataStartRow, 1],
                worksheet.Cells[DataStartRow + recordCount - 1, DataColumnCount]];
            object[,] values = range.Value2 as object[,];
            if (values == null)
                return data;

            for (int row = 1; row <= recordCount; row++)
            {
                string mode = Convert.ToString(values[row, 1]);
                string deviceType = Convert.ToString(values[row, 2]);
                string recordType = Convert.ToString(values[row, 3]);
                string key = Convert.ToString(values[row, 4]);
                string value = Convert.ToString(values[row, 5]);

                if (mode == AutomaticMode || mode == ManualMode)
                {
                    SettingsProfile profile = data.Profiles[mode];
                    string normalizedType = NormalizeDeviceType(deviceType);
                    if (!profile.DeviceTypes.TryGetValue(normalizedType, out DeviceSelectionSettings selection))
                    {
                        selection = new DeviceSelectionSettings();
                        profile.DeviceTypes[normalizedType] = selection;
                    }
                    if (recordType == ProducerRecord && !string.IsNullOrWhiteSpace(value))
                        selection.Producers.Add(value);
                    else if (recordType == SeriesRecord && !string.IsNullOrWhiteSpace(value))
                        selection.Series.Add(value);
                }
                else if (mode == StateMode && recordType == StateRecord)
                {
                    data.HasAutomaticState = true;
                    bool stateValue = value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                    if (key == nameof(AutomaticSelectionState.ModularCircuitBreakersEnabled))
                        data.AutomaticState.ModularCircuitBreakersEnabled = stateValue;
                    else if (key == nameof(AutomaticSelectionState.ModularResidualCurrentCircuitBreakersEnabled))
                        data.AutomaticState.ModularResidualCurrentCircuitBreakersEnabled = stateValue;
                    else if (key == nameof(AutomaticSelectionState.SelectAllPanels))
                        data.AutomaticState.SelectAllPanels = stateValue;
                }
            }

            foreach (SettingsProfile profile in data.Profiles.Values)
                foreach (DeviceSelectionSettings selection in profile.DeviceTypes.Values)
                {
                    selection.Producers = Clean(selection.Producers);
                    selection.Series = Clean(selection.Series);
                }
            return data;
        }

        private void WriteWorkbookData(WorkbookSettingsData data, Excel.Worksheet worksheet = null)
        {
            worksheet = worksheet ?? GetSettingsWorksheet();
            List<SettingsRow> rows = CreateRows(data);
            int previousCount = ReadRecordCount(worksheet);
            int rowsToClear = Math.Max(previousCount, rows.Count);
            if (rowsToClear > 0)
            {
                Excel.Range oldRange = worksheet.Range[
                    worksheet.Cells[DataStartRow, 1],
                    worksheet.Cells[DataStartRow + rowsToClear - 1, DataColumnCount]];
                oldRange.ClearContents();
            }

            object[,] header = new object[1, 3];
            header[0, 0] = StorageMarker;
            header[0, 1] = CurrentSchemaVersion;
            header[0, 2] = rows.Count;
            Excel.Range headerRange = worksheet.Range[worksheet.Cells[HeaderRow, 1], worksheet.Cells[HeaderRow, 3]];
            headerRange.Value2 = header;

            if (rows.Count == 0)
                return;
            object[,] values = new object[rows.Count, DataColumnCount];
            for (int index = 0; index < rows.Count; index++)
            {
                values[index, 0] = rows[index].Mode;
                values[index, 1] = rows[index].DeviceType;
                values[index, 2] = rows[index].RecordType;
                values[index, 3] = rows[index].Key;
                values[index, 4] = rows[index].Value;
            }
            Excel.Range targetRange = worksheet.Range[
                worksheet.Cells[DataStartRow, 1],
                worksheet.Cells[DataStartRow + rows.Count - 1, DataColumnCount]];
            targetRange.Value2 = values;
        }

        private static List<SettingsRow> CreateRows(WorkbookSettingsData data)
        {
            var rows = new List<SettingsRow>();
            foreach (string mode in new[] { ManualMode, AutomaticMode })
            {
                SettingsProfile profile = data.Profiles[mode];
                foreach (KeyValuePair<string, DeviceSelectionSettings> device in
                    profile.DeviceTypes.OrderBy(item => item.Key, StringComparer.CurrentCultureIgnoreCase))
                {
                    int order = 0;
                    foreach (string producer in Clean(device.Value.Producers))
                        rows.Add(new SettingsRow(mode, device.Key, ProducerRecord, (++order).ToString(), producer));
                    order = 0;
                    foreach (string series in Clean(device.Value.Series))
                        rows.Add(new SettingsRow(mode, device.Key, SeriesRecord, (++order).ToString(), series));
                }
            }
            if (data.HasAutomaticState)
            {
                rows.Add(new SettingsRow(StateMode, StateDeviceType, StateRecord,
                    nameof(AutomaticSelectionState.ModularCircuitBreakersEnabled),
                    data.AutomaticState.ModularCircuitBreakersEnabled ? "1" : "0"));
                rows.Add(new SettingsRow(StateMode, StateDeviceType, StateRecord,
                    nameof(AutomaticSelectionState.ModularResidualCurrentCircuitBreakersEnabled),
                    data.AutomaticState.ModularResidualCurrentCircuitBreakersEnabled ? "1" : "0"));
                rows.Add(new SettingsRow(StateMode, StateDeviceType, StateRecord,
                    nameof(AutomaticSelectionState.SelectAllPanels),
                    data.AutomaticState.SelectAllPanels ? "1" : "0"));
            }
            return rows;
        }

        private WorkbookSettingsData ReadLegacyData(string workbookFullName, string workbookName, string legacyPath)
        {
            var data = new WorkbookSettingsData(workbookName);
            string automaticPath = GetLegacyProfilePath(AutomaticMode, workbookFullName, workbookName);
            string manualPath = GetLegacyProfilePath(ManualMode, workbookFullName, workbookName);
            data.HasLegacyData |= TryReadLegacyJsonProfile(automaticPath, AutomaticMode, out SettingsProfile automatic);
            data.HasLegacyData |= TryReadLegacyJsonProfile(manualPath, ManualMode, out SettingsProfile manual);
            if (automatic != null) data.Profiles[AutomaticMode] = automatic;
            if (manual != null) data.Profiles[ManualMode] = manual;
            data.Profiles[AutomaticMode].WorkbookName = workbookName;
            data.Profiles[ManualMode].WorkbookName = workbookName;

            if (!string.IsNullOrWhiteSpace(legacyPath) && File.Exists(legacyPath))
            {
                data.HasLegacyData = true;
                foreach (string line in File.ReadAllLines(legacyPath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split('%');
                    if (parts.Length < 3) continue;
                    bool isManual = parts[0].StartsWith("*", StringComparison.Ordinal);
                    SettingsProfile target = data.Profiles[isManual ? ManualMode : AutomaticMode];
                    string deviceType = NormalizeDeviceType(parts[0]);
                    if (target.DeviceTypes.ContainsKey(deviceType)) continue;
                    target.DeviceTypes[deviceType] = new DeviceSelectionSettings
                    {
                        Producers = SplitValues(parts[1]),
                        Series = SplitValues(parts[2])
                    };
                }
            }

            string statePath = GetLegacyAutomaticStatePath(workbookFullName, workbookName);
            if (File.Exists(statePath))
            {
                try
                {
                    data.AutomaticState = serializer.Deserialize<AutomaticSelectionState>(
                        File.ReadAllText(statePath, Encoding.UTF8)) ?? new AutomaticSelectionState();
                    data.HasAutomaticState = true;
                    data.HasLegacyData = true;
                }
                catch { }
            }
            return data;
        }

        private bool TryReadLegacyJsonProfile(string path, string mode, out SettingsProfile profile)
        {
            profile = null;
            if (!File.Exists(path)) return false;
            try
            {
                profile = serializer.Deserialize<SettingsProfile>(File.ReadAllText(path, Encoding.UTF8));
                ValidateProfile(profile, mode);
                return true;
            }
            catch
            {
                profile = null;
                return false;
            }
        }

        private static Excel.Worksheet GetSettingsWorksheet()
        {
            Excel.Workbook workbook = AppManager.ExcelApp?.ActiveWorkbook;
            if (workbook == null)
                throw new InvalidOperationException("Текущая книга Excel недоступна.");
            return workbook.Worksheets[SettingsSheetName] as Excel.Worksheet
                ?? throw new InvalidOperationException("В книге отсутствует лист \"списки1\".");
        }

        private static int ReadRecordCount(Excel.Worksheet worksheet)
        {
            object rawValue = ((Excel.Range)worksheet.Cells[HeaderRow, 3]).Value2;
            if (!int.TryParse(Convert.ToString(rawValue), out int count) || count < 0 || count > MaximumRecordCount)
                return 0;
            return count;
        }

        private static void ValidateWorkbookSchema(Excel.Worksheet worksheet)
        {
            object rawVersion = ((Excel.Range)worksheet.Cells[HeaderRow, 2]).Value2;
            if (!int.TryParse(Convert.ToString(rawVersion), out int version) || version != CurrentSchemaVersion)
                throw new InvalidDataException("Версия настроек на листе \"списки1\" не поддерживается.");

            object rawCount = ((Excel.Range)worksheet.Cells[HeaderRow, 3]).Value2;
            if (!int.TryParse(Convert.ToString(rawCount), out int count) || count < 0 || count > MaximumRecordCount)
                throw new InvalidDataException("Таблица настроек на листе \"списки1\" повреждена.");
        }

        private string GetLegacyProfilePath(string mode, string workbookFullName, string workbookName)
        {
            string identity = string.IsNullOrWhiteSpace(workbookFullName) ? workbookName : workbookFullName;
            string hash;
            using (SHA256 sha = SHA256.Create())
                hash = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(identity ?? string.Empty)))
                    .Replace("-", string.Empty).Substring(0, 12);
            string safeName = string.Concat((Path.GetFileNameWithoutExtension(workbookName) ?? "Workbook")
                .Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
            return Path.Combine(legacySettingsRoot, safeName + "_" + hash, mode + ".json");
        }

        private string GetLegacyAutomaticStatePath(string workbookFullName, string workbookName)
        {
            string automaticProfilePath = GetLegacyProfilePath(AutomaticMode, workbookFullName, workbookName);
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

        private sealed class WorkbookSettingsData
        {
            public WorkbookSettingsData(string workbookName)
            {
                Profiles = new Dictionary<string, SettingsProfile>
                {
                    [ManualMode] = CreateEmpty(ManualMode, workbookName),
                    [AutomaticMode] = CreateEmpty(AutomaticMode, workbookName)
                };
            }
            public Dictionary<string, SettingsProfile> Profiles { get; }
            public AutomaticSelectionState AutomaticState { get; set; } = new AutomaticSelectionState();
            public bool HasAutomaticState { get; set; }
            public bool HasLegacyData { get; set; }
        }

        private sealed class SettingsRow
        {
            public SettingsRow(string mode, string deviceType, string recordType, string key, string value)
            {
                Mode = mode; DeviceType = deviceType; RecordType = recordType; Key = key; Value = value;
            }
            public string Mode { get; }
            public string DeviceType { get; }
            public string RecordType { get; }
            public string Key { get; }
            public string Value { get; }
        }
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

    public static class SettingsImportDeviceDialog
    {
        public static List<string> Show(IEnumerable<string> availableDeviceTypes, Window owner = null)
        {
            var checkBoxes = (availableDeviceTypes ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .Select(deviceType => new KeyValuePair<string, CheckBox>(deviceType, new CheckBox
                {
                    Content = new TextBlock
                    {
                        Text = GetDisplayName(deviceType),
                        Margin = new Thickness(10, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = Brushes.Black,
                        TextWrapping = TextWrapping.Wrap
                    },
                    IsChecked = true,
                    Margin = new Thickness(18, 0, 18, 0),
                    Padding = new Thickness(0),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    FontSize = 14
                })).ToList();

            var window = new Window
            {
                Title = "Импорт настроек",
                Width = 720,
                Height = Math.Min(560, Math.Max(390, 300 + checkBoxes.Count * 58)),
                MinWidth = 650,
                MinHeight = 370,
                ResizeMode = ResizeMode.CanResize,
                WindowStartupLocation = owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
                Owner = owner,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                FontFamily = owner?.FontFamily ?? SystemFonts.MessageFontFamily,
                FontSize = 14,
                ShowInTaskbar = false
            };

            var frame = new Border
            {
                Margin = new Thickness(12),
                CornerRadius = new CornerRadius(12),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(144, 144, 144)),
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 14,
                    ShadowDepth = 3,
                    Opacity = 0.22,
                    Color = Colors.Black
                }
            };
            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(62) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(74) });

            var header = new Grid { Background = Brushes.Transparent };
            header.MouseLeftButtonDown += (sender, args) =>
            {
                if (args.ButtonState == System.Windows.Input.MouseButtonState.Pressed) window.DragMove();
            };
            header.Children.Add(new TextBlock
            {
                Text = "Импорт настроек",
                FontSize = 19,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            });
            Button titleClose = CreateCloseButton();
            titleClose.Click += (sender, args) => window.DialogResult = false;
            header.Children.Add(titleClose);
            root.Children.Add(header);

            var instruction = new TextBlock
            {
                Text = "Выберите типы оборудования, настройки которых нужно импортировать:",
                FontSize = 15,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(26, 10, 26, 14)
            };
            Grid.SetRow(instruction, 1);
            root.Children.Add(instruction);

            var choices = new ListBox
            {
                Margin = new Thickness(26, 0, 26, 0),
                Background = Brushes.White,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(2),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                SelectionMode = SelectionMode.Single
            };
            foreach (KeyValuePair<string, CheckBox> item in checkBoxes)
            {
                var itemBorder = new Border
                {
                    Height = 58,
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Child = item.Value
                };
                choices.Items.Add(itemBorder);
            }
            Grid.SetRow(choices, 2);
            root.Children.Add(choices);

            var buttons = new Grid { Margin = new Thickness(26, 16, 26, 18) };
            buttons.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            buttons.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            buttons.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            buttons.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            buttons.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Button selectAll = CreateButton("Выбрать всё", 120, false);
            selectAll.Click += (sender, args) => checkBoxes.ForEach(item => item.Value.IsChecked = true);
            buttons.Children.Add(selectAll);
            Button clear = CreateButton("Снять выбор", 120, false);
            clear.Margin = new Thickness(8, 0, 0, 0);
            clear.Click += (sender, args) => checkBoxes.ForEach(item => item.Value.IsChecked = false);
            Grid.SetColumn(clear, 1);
            buttons.Children.Add(clear);

            Button import = CreateButton("⇩  Импортировать", 155, true);
            import.Click += (sender, args) =>
            {
                if (!checkBoxes.Any(item => item.Value.IsChecked == true))
                {
                    MessageBox.Show(window, "Выберите хотя бы один тип оборудования.", "Импорт настроек",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                window.DialogResult = true;
            };
            Grid.SetColumn(import, 3);
            buttons.Children.Add(import);

            Button close = CreateButton("Закрыть", 105, false);
            close.Margin = new Thickness(8, 0, 0, 0);
            close.IsCancel = true;
            close.Click += (sender, args) => window.DialogResult = false;
            Grid.SetColumn(close, 4);
            buttons.Children.Add(close);
            Grid.SetRow(buttons, 3);
            root.Children.Add(buttons);
            frame.Child = root;
            window.Content = frame;

            if (window.ShowDialog() != true) return null;
            return checkBoxes.Where(item => item.Value.IsChecked == true).Select(item => item.Key).ToList();
        }

        private static string GetDisplayName(string deviceType)
        {
            if (string.Equals(deviceType, SettingsProfileService.ModularCircuitBreakersDeviceType,
                StringComparison.CurrentCultureIgnoreCase))
                return "Модульные автоматические выключатели";
            if (string.Equals(deviceType, SettingsProfileService.ModularResidualCurrentCircuitBreakersDeviceType,
                StringComparison.CurrentCultureIgnoreCase))
                return "Модульные автоматические выключатели дифференциального тока";
            return deviceType;
        }

        private static Button CreateButton(string text, double width, bool isPrimary)
        {
            return new Button
            {
                Content = text,
                Width = width,
                Height = 36,
                Padding = new Thickness(10, 4, 10, 4),
                Background = isPrimary ? Brushes.Gray : Brushes.LightGray,
                Foreground = isPrimary ? Brushes.White : Brushes.Black,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Template = CreateRoundedButtonTemplate()
            };
        }

        private static Button CreateCloseButton()
        {
            var button = new Button
            {
                Content = "×",
                Width = 42,
                Height = 42,
                FontSize = 28,
                FontWeight = FontWeights.Light,
                Foreground = Brushes.Gray,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
                Template = CreateRoundedButtonTemplate()
            };
            return button;
        }

        private static ControlTemplate CreateRoundedButtonTemplate()
        {
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background")
                { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush")
                { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding("BorderThickness")
                { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(7));
            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(presenter);
            return new ControlTemplate(typeof(Button)) { VisualTree = border };
        }
    }
}
