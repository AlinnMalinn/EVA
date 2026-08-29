
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text;
using System.Windows.Controls;
using System;
using System.Windows.Interop;
using System.Linq;
using System.Windows.Forms; // для FolderBrowserDialog
using Microsoft.WindowsAPICodePack.Dialogs; // для CommonOpenFileDialog
using EVA_Settings;
using EVA_Catalogue_Shared;


namespace EVA_Catalogue
{
    public class MainViewModel : INotifyPropertyChanged
    {
        //public const string SourceDirectoryDB = @"C:\Users\79126\source\EVA\EVA_Catalogue\EVA_Catalogue";

        // public const string SourceDirectoryDB2 = "C:\\Users\\79126\\source\\EVA\\EVA_Catalogue\\EVA_Catalogue";
        // public const string SourceDirectorySettings = @"C:\Users\79126\source\EVA\EVA_Catalogue\EVA_Catalogue\Settings.txt";
 


        public const string TableNameModularCircuitBreakers = @"[Модульные автоматические выключатели]";
        public const string TableNameModularResidualCurrentCircuitBreakers = @"[Модульные автоматические выключатели дифференциального тока]";

        public const string ModularCircuitBreakersSettings = "ModularCircuitBreakers";
        public const string ModularResidualCurrentCircuitBreakersSettings = "ModularResidualCurrentCircuitBreakers";

        public static string currentTableFromDB ; 

        public event PropertyChangedEventHandler PropertyChanged;  
       
        private void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public bool isAutomaticSelectionEnabledForModularCircuitBreakers;
        public bool IsAutomaticSelectionEnabledForModularCircuitBreakers
        {
            get { return isAutomaticSelectionEnabledForModularCircuitBreakers; }

            set
            {
                isAutomaticSelectionEnabledForModularCircuitBreakers = value;
                NotifyPropertyChanged("isAutomaticSelectionEnabledForModularCircuitBreakers");
            }
        }
        public bool isAutomaticSelectionEnabledForModularResidualCircuitBreakers;
        public bool IsAutomaticSelectionEnabledForModularResidualCircuitBreakers
        {
            get { return isAutomaticSelectionEnabledForModularResidualCircuitBreakers; }

            set
            {
                isAutomaticSelectionEnabledForModularResidualCircuitBreakers = value;
                NotifyPropertyChanged("isAutomaticSelectionEnabledForModularResidualCircuitBreakers");
            }
        }
        public bool isExcelDataAvailable;
        public bool IsExcelDataAvailable
        {
            get { return isExcelDataAvailable; }
            set
            {
                isExcelDataAvailable = value;
                NotifyPropertyChanged("IsExcelDataAvailable");
                NotifyPropertyChanged(nameof(CatalogueStatusMessage));
            }
        }
        public string CatalogueStatusMessage
        {
            get
            {
                if (IsExcelDataAvailable)
                    return string.Empty;
                if (string.IsNullOrWhiteSpace(LinkForDB))
                    return "Папка с каталогами не выбрана.\nУкажите путь к папке в настройках каталогов.";
                if (!Directory.Exists(LinkForDB))
                    return "Папка с каталогами недоступна или была перемещена.\nУкажите путь к папке заново.";
                return "Excel-каталоги оборудования в выбранной папке не найдены.\nДобавьте файлы каталогов и нажмите «Обновить каталоги».";
            }
        }
        public string linkForDB;
        public string LinkForDB
        {
            get { return linkForDB; }
            set
            {
                linkForDB = value;
                NotifyPropertyChanged("LinkForDB");
                NotifyPropertyChanged(nameof(CatalogueStatusMessage));
            }
        }



        public MainViewModel()
        {
            IsExcelDataAvailable = new PathHelper().CheckLinkForDB();
            LinkForDB = new PathHelper().GetLinkForDB();
            if (IsExcelDataAvailable)
            {
                CatalogueCacheService.Instance.ConfigureDirectory(LinkForDB);
                var previousCursor = Mouse.OverrideCursor;
                try
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    CatalogueCacheService.Instance.RefreshAll();
                    IsExcelDataAvailable = CatalogueCacheService.Instance.HasCatalogueFiles();
                }
                catch { /* Ошибка будет показана при обращении к конкретному каталогу. */ }
                finally { Mouse.OverrideCursor = previousCursor; }
            }
            Accept = new RelayCommand(param => OkCommand()); //проброс команды
            Cancel = new RelayCommand(param => CancelCommand());
           
            EquipmentSelection = new RelayCommand(param => SayResult());

            RefreshCataloguesCommand = new RelayCommand(param => RefreshCatalogues());
            ExportSettingsCommand = new RelayCommand(param => ExportSettings());
            ImportSettingsCommand = new RelayCommand(param => ImportSettings());
            OpenWindowSettingsModularCircuitBreakersCommand = new RelayCommand(param => OpenWindowSettingsModularCircuitBreakers());
            OpenWindowSettingsModularResidualCurrentBreakersCommand = new RelayCommand(param => OpenWindowSettingsModularResidualCurrentBreakers());
            LoadAutomaticSelectionState();
        }
        private bool isSelectionForAllPanels = true;
        public bool IsSelectionForAllPanels
        {
            get { return isSelectionForAllPanels; }
            set
            {
                isSelectionForAllPanels = value;
                NotifyPropertyChanged(nameof(IsSelectionForAllPanels));
            }
        }
           
        private void CancelCommand()
        {
            System.Windows.Application.Current.MainWindow.Close();


        }

        private void RefreshCatalogues()
        {
            var previousCursor = Mouse.OverrideCursor;
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                PathHelper pathHelper = new PathHelper();
                string directory = pathHelper.PathDBHelper();
                LinkForDB = directory;
                if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                {
                    IsExcelDataAvailable = false;
                    string message = string.IsNullOrWhiteSpace(directory)
                        ? "Папка с каталогами не выбрана. Укажите путь к папке в настройках каталогов."
                        : "Папка с каталогами недоступна или была перемещена. Укажите путь к папке заново.";
                    System.Windows.MessageBox.Show(message, "Обновление каталогов",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                CatalogueCacheService.Instance.ConfigureDirectory(directory);
                int refreshed = CatalogueCacheService.Instance.RefreshAll(true);
                IsExcelDataAvailable = CatalogueCacheService.Instance.HasCatalogueFiles();
                System.Windows.MessageBox.Show(
                    refreshed == 0 ? "Excel-каталоги не найдены." : $"Каталоги обновлены: {refreshed}.",
                    "Обновление каталогов", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Не удалось обновить каталоги: " + ex.Message,
                    "Обновление каталогов", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                Mouse.OverrideCursor = previousCursor;
            }
        }

        private void ExportSettings()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Настройки EVA (*.evasettings)|*.evasettings",
                DefaultExt = ".evasettings",
                AddExtension = true,
                FileName = "EVA_Автоматический_" + DateTime.Now.ToString("yyyy-MM-dd")
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                Excel.Workbook workbook = AppManager.ExcelApp.ActiveWorkbook;
                SettingsProfileService.Instance.Export(SettingsProfileService.AutomaticMode, dialog.FileName,
                    workbook.FullName, workbook.Name, new PathHelper().PathSettingsHelper());
                System.Windows.MessageBox.Show("Настройки экспортированы.", "Настройки",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Не удалось экспортировать настройки: " + ex.Message,
                    "Настройки", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ImportSettings()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Настройки EVA (*.evasettings)|*.evasettings"
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                List<string> availableDeviceTypes = SettingsProfileService.Instance.GetImportDeviceTypes(
                    SettingsProfileService.AutomaticMode, dialog.FileName);
                if (availableDeviceTypes.Count == 0)
                    throw new InvalidDataException("В файле нет настроек оборудования.");

                Window owner = System.Windows.Application.Current.Windows.Cast<Window>()
                    .FirstOrDefault(window => window.IsActive);
                List<string> selectedDeviceTypes = SettingsImportDeviceDialog.Show(availableDeviceTypes, owner);
                if (selectedDeviceTypes == null) return;

                Excel.Workbook workbook = AppManager.ExcelApp.ActiveWorkbook;
                SettingsProfileService.Instance.Import(SettingsProfileService.AutomaticMode, dialog.FileName,
                    workbook.FullName, workbook.Name, selectedDeviceTypes);
                System.Windows.MessageBox.Show("Настройки импортированы.", "Настройки",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Не удалось импортировать настройки: " + ex.Message,
                    "Настройки", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OkCommand()
        {
            try
            {
                Excel.Workbook workbook = AppManager.ExcelApp.ActiveWorkbook;
                SettingsProfileService.Instance.SaveAutomaticSelectionState(
                    new AutomaticSelectionState
                    {
                        ModularCircuitBreakersEnabled = IsAutomaticSelectionEnabledForModularCircuitBreakers,
                        ModularResidualCurrentCircuitBreakersEnabled = IsAutomaticSelectionEnabledForModularResidualCircuitBreakers,
                        SelectAllPanels = IsSelectionForAllPanels
                    },
                    workbook.FullName,
                    workbook.Name);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Не удалось сохранить состояние автоматического подбора: " + ex.Message,
                    "Настройки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            System.Windows.Application.Current.MainWindow.Close();
        }

        private void LoadAutomaticSelectionState()
        {
            try
            {
                Excel.Workbook workbook = AppManager.ExcelApp.ActiveWorkbook;
                AutomaticSelectionState state = SettingsProfileService.Instance.LoadAutomaticSelectionState(
                    workbook.FullName, workbook.Name);
                IsAutomaticSelectionEnabledForModularCircuitBreakers = state.ModularCircuitBreakersEnabled;
                IsAutomaticSelectionEnabledForModularResidualCircuitBreakers =
                    state.ModularResidualCurrentCircuitBreakersEnabled;
                IsSelectionForAllPanels = state.SelectAllPanels;
            }
            catch
            {
                IsAutomaticSelectionEnabledForModularCircuitBreakers = false;
                IsAutomaticSelectionEnabledForModularResidualCircuitBreakers = false;
                IsSelectionForAllPanels = true;
            }
        }
        private void SayResult() // подбор оборудования и вывод результатов

        {
            string selectedSheetName = null;
            if (!IsSelectionForAllPanels)
            {
                Excel.Worksheet activeSheet = AppManager.ExcelApp.ActiveSheet as Excel.Worksheet;
                if (activeSheet == null || !activeSheet.Name.StartsWith("EVA", StringComparison.OrdinalIgnoreCase))
                {
                    System.Windows.MessageBox.Show(
                        "Для подбора по текущей панели выберите лист, название которого начинается с \"EVA\".",
                        "Подбор оборудования", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                selectedSheetName = activeSheet.Name;
            }

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            System.Windows.Application.Current.MainWindow.IsEnabled = false;


            PathHelper pathHelper = new PathHelper();
            string sourceDirectorySettings = pathHelper.PathSettingsHelper();
            List<string> producerListForSettingsQF = new List<string>();
            List<string> seriesListForSettingsQF = new List<string>();
            List<string> producerListForSettingsQFD = new List<string>();
            List<string> seriesListForSettingsQFD = new List<string>();
            {
                Excel.Workbook settingsWorkbook = AppManager.ExcelApp.ActiveWorkbook;
                SettingsProfile automaticProfile = SettingsProfileService.Instance.Load(
                    SettingsProfileService.AutomaticMode, settingsWorkbook.FullName,
                    settingsWorkbook.Name, sourceDirectorySettings);
                if (IsAutomaticSelectionEnabledForModularCircuitBreakers == true)
                {
                    if (automaticProfile.DeviceTypes.TryGetValue(ModularCircuitBreakersSettings, out DeviceSelectionSettings qf))
                    {
                        producerListForSettingsQF.AddRange(qf.Producers);
                        seriesListForSettingsQF.AddRange(qf.Series);
                    }
                }
                    if (IsAutomaticSelectionEnabledForModularResidualCircuitBreakers == true)
                    {
                        if (automaticProfile.DeviceTypes.TryGetValue(ModularResidualCurrentCircuitBreakersSettings, out DeviceSelectionSettings qfd))
                        {
                            producerListForSettingsQFD.AddRange(qfd.Producers);
                            seriesListForSettingsQFD.AddRange(qfd.Series);
                        }
                    }
                
                bool selectQf = IsAutomaticSelectionEnabledForModularCircuitBreakers;
                bool selectQfd = IsAutomaticSelectionEnabledForModularResidualCircuitBreakers;
                bool qfProducersMissing = selectQf && !producerListForSettingsQF.Any(s => !string.IsNullOrWhiteSpace(s));
                bool qfdProducersMissing = selectQfd && !producerListForSettingsQFD.Any(s => !string.IsNullOrWhiteSpace(s));

                if (qfProducersMissing || qfdProducersMissing)
                {
                    Mouse.OverrideCursor = null;
                    System.Windows.Application.Current.MainWindow.IsEnabled = true;

                    if (selectQf && selectQfd && qfProducersMissing && qfdProducersMissing)
                    {
                        System.Windows.MessageBox.Show(
                            "Для автоматического подбора не выбраны производители ни для модульных АВ, ни для модульных АВДТ.\nОткройте настройки и выберите производителей.",
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    if (selectQf && selectQfd && qfdProducersMissing)
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show(
                            "Для модульных АВДТ не выбраны производители.\nПродолжить подбор только для модульных АВ?",
                            "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result != MessageBoxResult.Yes)
                        {
                            return;
                        }
                        selectQfd = false;
                    }
                    else if (selectQf && selectQfd && qfProducersMissing)
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show(
                            "Для модульных АВ не выбраны производители.\nПродолжить подбор только для модульных АВДТ?",
                            "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result != MessageBoxResult.Yes)
                        {
                            return;
                        }
                        selectQf = false;
                    }
                    else
                    {
                        string deviceType = qfProducersMissing ? "модульных АВ" : "модульных АВДТ";
                        System.Windows.MessageBox.Show(
                            "Для " + deviceType + " не выбраны производители.\nОткройте настройки и выберите производителей.",
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    System.Windows.Application.Current.MainWindow.IsEnabled = false;
                }

                if ((selectQf && producerListForSettingsQF.Any(s => !string.IsNullOrWhiteSpace(s))) |
                    (selectQfd && producerListForSettingsQFD.Any(s => !string.IsNullOrWhiteSpace(s))))
                    {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    EquipmentSelection es = new EquipmentSelection();
                    List<List<string>> failedIteams =es.SelectDevicecs_ModularCircuitBreaker(producerListForSettingsQF, seriesListForSettingsQF, producerListForSettingsQFD, seriesListForSettingsQFD, selectQf, selectQfd, selectedSheetName);

                   
                    var sb = new System.Text.StringBuilder();
                    int failedQfCount = CountFailedDevices(failedIteams[0]);
                    int failedQfdCount = CountFailedDevices(failedIteams[1]);
                    int failedTotalCount = failedQfCount + failedQfdCount;

                    if (failedTotalCount > 0)
                    {
                        sb.AppendLine("Не удалось подобрать: " + failedTotalCount + " " + GetDeviceWord(failedTotalCount));
                        sb.AppendLine();
                    }
                    if (failedQfCount > 0)
                    {
                        sb.AppendLine("Модульные автоматические выключатели - " + failedQfCount);
                        sb.AppendLine();
                        sb.Append(TextForReport(failedIteams[0]));
                        sb.AppendLine();
                    }
                    if (failedQfdCount > 0)
                    {
                        sb.AppendLine("Модульные дифференциальные автоматические выключатели - " + failedQfdCount);
                        sb.AppendLine();
                        sb.Append(TextForReport(failedIteams[1]));
                        sb.AppendLine();
                    }
                    if (failedTotalCount > 0)
                    {
                        sb.AppendLine("Что необходимо проверить");
                        sb.AppendLine();
                        sb.AppendLine("Проверьте выбранные каталоги производителей и серий, а также параметры аппаратов.");
                        sb.AppendLine();
                    }
                    if (failedQfCount > 0)
                    {
                        AppendQfParameters(sb, "Для АВ:");
                    }
                    if (failedQfdCount > 0)
                    {
                        if (failedQfCount > 0)
                        {
                            sb.AppendLine();
                        }
                        AppendQfParameters(sb, "Для АВДТ:");
                        sb.AppendLine("  • Ток утечки dI, мА (строка 22)");
                        sb.AppendLine("  • Тип дифференциального тока, например A (строка 22)");
                        sb.AppendLine();
                        sb.AppendLine("    Если тип не указан, по умолчанию выбирается тип A.");
                    }
                    Mouse.OverrideCursor = null;
                    string resultText = sb.ToString();
                    if (!string.IsNullOrEmpty(resultText))
                    {

                        var resultWindow = new SelectionResultWindow(resultText)
                        {
                            Owner = System.Windows.Application.Current.MainWindow
                        };
                        resultWindow.ShowDialog();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
        "Оборудование подобрано успешно",
        "Результат",
        MessageBoxButton.OK,
        MessageBoxImage.Information);
                    }
                }
               

            }
            System.Windows.Application.Current.MainWindow.IsEnabled = true;
        }

        private string TextForReport(List<string> failedIteams)
        {
            string currentSheet = null;
            var sb = new System.Text.StringBuilder();

            var columns = new List<string>();

                foreach (var item in failedIteams)
                {
                    if (item.StartsWith("EVA"))
                    {
                        // если есть предыдущий лист, добавляем его с колонками
                        if (currentSheet != null)
                        {
                            sb.AppendLine("  " + currentSheet + ":");
                            sb.AppendLine("  " + string.Join(", ", columns));
                            sb.AppendLine();
                        }

                        // начинаем новый лист
                        currentSheet = item;
                        columns.Clear();
                    }
                    else
                    {
                        columns.Add(item);
                    }
                }
                // добавляем последний лист
                if (currentSheet != null)
                {
                    sb.AppendLine("  " + currentSheet + ":");
                    sb.AppendLine("  " + string.Join(", ", columns));
                    sb.AppendLine();
                }
            
            return sb.ToString();
        }

        private static int CountFailedDevices(IEnumerable<string> failedItems)
        {
            return (failedItems ?? Enumerable.Empty<string>())
                .Count(item => !string.IsNullOrWhiteSpace(item) && !item.StartsWith("EVA"));
        }

        private static string GetDeviceWord(int count)
        {
            int lastTwoDigits = count % 100;
            if (lastTwoDigits >= 11 && lastTwoDigits <= 14) return "аппаратов";
            switch (count % 10)
            {
                case 1: return "аппарат";
                case 2:
                case 3:
                case 4: return "аппарата";
                default: return "аппаратов";
            }
        }

        private static void AppendQfParameters(System.Text.StringBuilder sb, string title)
        {
            sb.AppendLine(title);
            sb.AppendLine();
            sb.AppendLine("  • Количество фаз (строка 11)");
            sb.AppendLine("  • Ток расцепителя Iав, А (строка 17)");
            sb.AppendLine("  • Характеристика срабатывания (строка 18)");
            sb.AppendLine("  • Ном. отключ. способность, кА (строка 19)");
            sb.AppendLine();
            sb.AppendLine("  Если отключающая способность не указана:");
            sb.AppendLine();
            sb.AppendLine("    - МАХ Ток КЗ на РП Iкд3ф, кА (строка 150)");
            sb.AppendLine("    - МАХ Ток КЗ на РП Iкд1ф, кА (строка 151)");
            sb.AppendLine();
        }
        //private void SaveFolderDialog()
        //{
        //    try
        //    {
        //        string folderPath = string.Empty;

        //        // Пытаемся использовать современный CommonOpenFileDialog
        //        try
        //        {
        //            var modernDialog = new CommonOpenFileDialog
        //            {
        //                IsFolderPicker = true,
        //                Title = "Выберите папку для сохранения базы данных"
        //            };

        //            if (modernDialog.ShowDialog() == CommonFileDialogResult.Ok)
        //            {
        //                folderPath = modernDialog.FileName;
        //            }
        //        }
        //        catch
        //        {
        //            // Если не удалось → fallback на классический FolderBrowserDialog
        //            using (var classicDialog = new FolderBrowserDialog())
        //            {
        //                classicDialog.Description = "Выберите папку для сохранения базы данных";
        //                classicDialog.ShowNewFolderButton = true;

        //                if (classicDialog.ShowDialog() == DialogResult.OK)
        //                {
        //                    folderPath = classicDialog.SelectedPath;
        //                }
        //            }
        //        }

        //        // Если пользователь выбрал папку → сохраняем путь
        //        if (!string.IsNullOrEmpty(folderPath))
        //        {
        //            PathHelper path = new PathHelper();
        //            path.SaveLinkForDB(folderPath);
        //            IsExcelDataAvailable = path.CheckLinkForDB();
        //            LinkForDB = path.GetLinkForDB();
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        System.Windows.MessageBox.Show("Ошибка: " + ex.Message);
        //    }
        //}




        private void  OpenWindowSettingsModularCircuitBreakers()
        {

            {
                currentTableFromDB = ModularCircuitBreakersSettings;
                SettingsHelper.Instance.SetTypeOfDevice(currentTableFromDB);
                WindowSettingsModularCircuitBreakers windowSettingsModularCircuitBreakers = new WindowSettingsModularCircuitBreakers();
                windowSettingsModularCircuitBreakers.ShowDialog();

            }

        }
    private void OpenWindowSettingsModularResidualCurrentBreakers()
        {
            
            {
                currentTableFromDB = ModularResidualCurrentCircuitBreakersSettings;
                SettingsHelper.Instance.SetTypeOfDevice(currentTableFromDB);
                WindowSettingsModularCircuitBreakers windowSettingsModularCircuitBreakers = new WindowSettingsModularCircuitBreakers();
                windowSettingsModularCircuitBreakers.ShowDialog();
           

            }

        }
        public ICommand EquipmentSelection { protected set; get; }
        public ICommand Accept { get; }
        public ICommand Cancel { get; }
        public ICommand OpenWindowSettingsModularCircuitBreakersCommand { set; get; }
        public ICommand OpenWindowSettingsModularResidualCurrentBreakersCommand { get; }
        public ICommand RefreshCataloguesCommand { get; set; }
        public ICommand ExportSettingsCommand { get; set; }
        public ICommand ImportSettingsCommand { get; set; }

    }

}
