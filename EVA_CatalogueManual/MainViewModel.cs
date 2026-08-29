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
using Microsoft.Office.Interop.Excel;
using System.Collections.ObjectModel;
using EVA_Catalogue_Shared;
using System.Xml.Linq;
using EVA_Settings;
using EVA_Catalogue;



namespace EVA_CatalogueManual
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public const string TableNameModularCircuitBreakers = @"[Модульные автоматические выключатели]";
        public const string TableNameModularResidualCurrentCircuitBreakers = @"[Модульные автоматические выключатели дифференциального тока]";
        public const string ModularCircuitBreakers = "Модульный автоматический выключатель";
        public const string ModularResidualCurrentCircuitBreakers = "Модульный автоматический выключатель дифференциального тока";
        public const string ModularCircuitBreakersSettings = "ModularCircuitBreakers";
        public const string ModularResidualCurrentCircuitBreakersSettings = "ModularResidualCurrentCircuitBreakers";
        //Имена таблиц для поиска таблиц в БД
        public const string TableModularCircuitBreakers = "Модульные автоматические выключатели";
        public const string TableModularResidualCurrentCircuitBreakers = "Модульные автоматические выключатели дифференциального тока";
        private const string InfoDeviceNotFound = "По текущим параметрам оборудование не подобрано";
        Excel.Workbook excelWB = AppManager.ExcelApp.ActiveWorkbook;
        private bool isApplyingSelection;





        public bool isExcelDataAvailable;
        public bool IsExcelDataAvailable
        {
            get { return isExcelDataAvailable; }
            set
            {
                isExcelDataAvailable = value;
                NotifyPropertyChanged("IsExcelDataAvailable");
                NotifyPropertyChanged(nameof(CatalogueStatusMessage));
                NotifyPropertyChanged(nameof(CanSaveConfiguration));
                NotifyPropertyChanged(nameof(CanResetConfiguration));
                NotifyPropertyChanged(nameof(CanWriteEquipment));
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
        public string chosenTypeOfDevice;
        public string ChosenTypeOfDevice
        {
            get => chosenTypeOfDevice;
            set
            {

                chosenTypeOfDevice = value;
                NotifyPropertyChanged("ChosenTypeOfDevice");
            }
        }
        public string deviceNotFound;
        public string DeviceNotFound
        {
            get => deviceNotFound;
            set
            {

                deviceNotFound = value;
                NotifyPropertyChanged("DeviceNotFound");
            }
        }
        public List<ProducerModel> producerList;
        public List<ProducerModel> ProducerList
        {
            get { return producerList; }

            set
            {
                producerList = value;
                NotifyPropertyChanged("ProducerList");
            }
        }
        public List<SeriesModel> seriesList;
        public List<SeriesModel> SeriesList
        {
            get { return seriesList; }

            set
            {
                seriesList = value;
                NotifyPropertyChanged("SeriesList");
            }
        }
        public List<ProducerModel> newProducerList;
        public List<ProducerModel> NewProducerList
        {
            get { return newProducerList; }

            set
            {
                newProducerList = value;
                NotifyPropertyChanged("NewProducerList");
            }
        }
        public List<SeriesModel> newSeriesList;
        public List<SeriesModel> NewSeriesList
        {
            get { return newSeriesList; }

            set
            {
                newSeriesList = value;
                NotifyPropertyChanged("NewSeriesList");
            }
        }

        private ProducerModel selectedProducer;
        public ProducerModel SelectedProducer
        {
            get { return selectedProducer; }
            set
            {
                selectedProducer = value;
                NotifyPropertyChanged("SelectedProducer");


            }
        }
        private System.Data.DataTable tableOfDevices;
        public System.Data.DataTable TableOfDevices
        {
            get { return tableOfDevices; }
            set
            {
                tableOfDevices = value;
                NotifyPropertyChanged("TableOfDevices");

            }
        }
        private DataRowView selectedRow;

        public DataRowView SelectedRow
        {
            get { return selectedRow; }
            set
            {
                selectedRow = value;
                CheckRowSelected();
                NotifyPropertyChanged("SelectedRow");
            }
        }
        public bool doesConfigurationExist;
        public bool DoesConfigurationExist
        {
            get { return doesConfigurationExist; }
            set
            {
                doesConfigurationExist = value;
                NotifyPropertyChanged("DoesConfigurationExist");
                NotifyPropertyChanged("CanResetConfiguration");

            }
        }
        public bool isAnyProducerSelected;
        public bool IsAnyProducerSelected
        {
            get { return isAnyProducerSelected; }
            set
            {
                isAnyProducerSelected = value;
                NotifyPropertyChanged("IsAnyProducerSelected");
                NotifyPropertyChanged("CanSaveConfiguration");
            }
        }
        public bool isDeviceSelected;
        public bool IsDeviceSelected
        {
            get { return isDeviceSelected; }
            set
            {
                isDeviceSelected = value;
                NotifyPropertyChanged("IsDeviceSelected");
                NotifyPropertyChanged("CanSaveConfiguration");
                NotifyPropertyChanged("CanResetConfiguration");
            }
        }
        public bool isRowSelected;
        public bool IsRowSelected
        {
            get { return isRowSelected; }
            set
            {
                isRowSelected = value;
                NotifyPropertyChanged(nameof(IsRowSelected));
                NotifyPropertyChanged(nameof(CanWriteEquipment));
            }
        }
        public bool CanSaveConfiguration
        {
            get
            {
                return IsExcelDataAvailable && isDeviceSelected &&
                       isAnyProducerSelected;
            }
        }
        public bool CanResetConfiguration
        {
            get
            {
                return IsExcelDataAvailable && isDeviceSelected &&
                       doesConfigurationExist;
            }
        }
        public bool CanWriteEquipment => IsExcelDataAvailable && IsRowSelected;
        private void CancelCommand()
        {
            System.Windows.Application.Current.MainWindow.Close();
            DetachFromExcel();


        }
        private void OkCommand()
        {

            System.Windows.Application.Current.MainWindow.Close();
            DetachFromExcel();

        }


        public MainViewModel()
        {
            this.PropertyChanged += ChosenTypeOfDevice_PropertyChanged;

            // затем можно получить начальное значение
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
                    catch { /* Повторная попытка будет выполнена при обращении к каталогу. */ }
                    finally { Mouse.OverrideCursor = previousCursor; }
                }
                chosenTypeOfDevice = new ExcelHelperForEva().GetCellValue();
                ExcelHelperForEva excelHelper = new ExcelHelperForEva();
                List<string>[] commonListForSettings = new Configurations().LoadConfiguration(chosenTypeOfDevice);
                ProducerList = CreateProducerList();

            //if (commonListForSettings[0] != null)
            if (commonListForSettings != null)
            {
                    ApplyProducerSelection(commonListForSettings[0]);

                }
                TryAttachToExcel();

                CheckIfAnyProducerChecked();
                CheckConfiguration();
                CheckIfDeviceSelected();


                ChosenTypeOfDeviceCommand = new RelayCommand(param => TryAttachToExcel());


                Accept = new RelayCommand(param => OkCommand()); //проброс команды
                Cancel = new RelayCommand(param => CancelCommand());
                EquipmentSelection = new RelayCommand(param => WriteDeviceDataToExcel());
                ResetConfigurationCommand = new RelayCommand(param => ResetChosenConfiguration());
                ImportSettingsCommand = new RelayCommand(param => ImportSettings());
                ExportSettingsCommand = new RelayCommand(param => ExportSettings());
                RefreshCataloguesCommand = new RelayCommand(param => RefreshCatalogues());
            }
        


        private void ResetChosenConfiguration()
        {

            new Configurations().ResetConfiguration(chosenTypeOfDevice);
            List<string>[] commonListForSettings = new Configurations().LoadConfiguration(chosenTypeOfDevice);

            ProducerList=CreateProducerList();
            CheckIfAnyProducerChecked();
            CheckConfiguration();
            CheckIfDeviceSelected();
        }
        private void SaveConfiguration()
        {
            NewProducerList = (ProducerList ?? new List<ProducerModel>())
                .Where(producer => producer.IsSelected)
                .ToList();
            NewSeriesList = (SeriesList ?? new List<SeriesModel>())
                .Where(series => series.IsSelected)
                .ToList();

            new Configurations().CreateConfiguration(NewProducerList, NewSeriesList, chosenTypeOfDevice);
            CheckConfiguration();
            CheckIfAnyProducerChecked();
        }
        private void CheckConfiguration()
        {
            List<string>[] commonListForSettings = new Configurations().LoadConfiguration(chosenTypeOfDevice);

            //if (commonListForSettings[0] == null)
            if (commonListForSettings == null)
            { doesConfigurationExist = false; }
            else
            { doesConfigurationExist = true; }
            DoesConfigurationExist = doesConfigurationExist;
        }
        private void CheckRowSelected()
        {
            if (selectedRow == null)
            { isRowSelected = false; }
            else
            { isRowSelected = true; }
            IsRowSelected = isRowSelected;
        }
        private void CheckIfAnyProducerChecked()
        {
            isAnyProducerSelected = false;
            foreach (ProducerModel producer in ProducerList)
            {
                if (producer.IsSelected)
                {
                    isAnyProducerSelected = true;
                    break;
                }
            }
            IsAnyProducerSelected = isAnyProducerSelected;
        }
        private void CheckIfDeviceSelected()
        {
            if (ChosenTypeOfDevice != "Выберете оборудование")
            {

                isDeviceSelected = true;

            }
            else isDeviceSelected = false;

            IsDeviceSelected = isDeviceSelected;
        }
        private List<ProducerModel> CreateProducerList()
        {

            // 1) Отписываем старые подписки
            if (ProducerList != null)
            {
                foreach (var p in ProducerList)
                    p.PropertyChanged -= ProducerModel_PropertyChanged;
            }
            producerList = new List<ProducerModel>();
            NewProducerList = new List<ProducerModel>(); // очищаем
            SeriesList = new List<SeriesModel>();// очищаем
            NewSeriesList = new List<SeriesModel>();// очищаем
            TableOfDevices = new System.Data.DataTable();// очищаем
            string sourceDirectoryDB = new PathHelper().PathDBHelper();
            //List<string>[] commonListForSettings = new List<string>[2];
            List<string>[] commonListForSettings = new Configurations().LoadConfiguration(chosenTypeOfDevice);

            if (chosenTypeOfDevice == ModularCircuitBreakers)
            {

                try
                {
                    CatalogueCacheService.Instance.ConfigureDirectory(sourceDirectoryDB);
                    foreach (string producer in GetProducerNamesWithWait("QF"))
                    {
                        ProducerModel producerModel = new ProducerModel();
                        producerModel.producer = producer;
                        producerModel.PropertyChanged += ProducerModel_PropertyChanged;
                        ProducerList.Add(producerModel);
                    }

                    //ProducerList = producerList;
                    if (commonListForSettings[0] != null)
                    {
                        ApplyProducerSelection(commonListForSettings[0]);

                    }
                   
                    return ProducerList;
                }
                catch
                {
                    return ProducerList;
                }
            }

            else if (chosenTypeOfDevice == ModularResidualCurrentCircuitBreakers)
            {

                try
                {
                    CatalogueCacheService.Instance.ConfigureDirectory(sourceDirectoryDB);
                    foreach (string producer in GetProducerNamesWithWait("QFD"))
                    {
                        var producerModel = new ProducerModel
                        {
                            producer = producer
                        };

                        producerModel.PropertyChanged += ProducerModel_PropertyChanged;

                        producerList.Add(producerModel);
                    }

                   // ProducerList = producerList;
                    if (commonListForSettings[0] != null)
                    {
                        ApplyProducerSelection(commonListForSettings[0]);
                    }
                    return ProducerList;
                }
                catch
                {
                    return ProducerList;
                }
            }
            else
            {
                ProducerList = new List<ProducerModel>(); // очищаем

                //NotifyPropertyChanged(nameof(SeriesList));
                return ProducerList;

            }

        }
        private static IReadOnlyList<string> GetProducerNamesWithWait(string requiredSheet)
        {
            var previousCursor = Mouse.OverrideCursor;
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                return CatalogueCacheService.Instance.GetProducerNames(requiredSheet);
            }
            finally
            {
                Mouse.OverrideCursor = previousCursor;
            }
        }
        private void RefreshCatalogues()
        {
            var previousCursor = Mouse.OverrideCursor;
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                string directory = new PathHelper().PathDBHelper();
                LinkForDB = directory;
                if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                {
                    IsExcelDataAvailable = false;
                    string message = string.IsNullOrWhiteSpace(directory)
                        ? "Папка с каталогами не выбрана. Укажите путь к папке в настройках каталогов."
                        : "Папка с каталогами недоступна или была перемещена. Укажите путь к папке заново.";
                    MessageBox.Show(message, "Обновление каталогов",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                CatalogueCacheService.Instance.ConfigureDirectory(directory);
                int refreshed = CatalogueCacheService.Instance.RefreshAll(true);
                IsExcelDataAvailable = CatalogueCacheService.Instance.HasCatalogueFiles();
                if (IsExcelDataAvailable)
                {
                    ProducerList = CreateProducerList();
                    CheckIfAnyProducerChecked();
                    CheckConfiguration();
                    CheckIfDeviceSelected();
                }
                MessageBox.Show(
                    IsExcelDataAvailable
                        ? "Каталоги обновлены: " + refreshed + "."
                        : "Excel-каталоги не найдены.",
                    "Обновление каталогов", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось обновить каталоги: " + ex.Message,
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
                FileName = "EVA_Ручной_" + DateTime.Now.ToString("yyyy-MM-dd")
            };
            if (dialog.ShowDialog() != true) return;
            Excel.Workbook workbook = AppManager.ExcelApp.ActiveWorkbook;
            SettingsProfileService.Instance.Export(SettingsProfileService.ManualMode, dialog.FileName,
                workbook.FullName, workbook.Name, new PathHelper().PathSettingsHelper());
            MessageBox.Show("Настройки экспортированы.", "Настройки", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    SettingsProfileService.ManualMode, dialog.FileName);
                if (availableDeviceTypes.Count == 0)
                    throw new System.IO.InvalidDataException("В файле нет настроек оборудования.");
                System.Windows.Window owner = System.Windows.Application.Current.Windows
                    .Cast<System.Windows.Window>().FirstOrDefault(window => window.IsActive);
                List<string> selectedDeviceTypes = SettingsImportDeviceDialog.Show(availableDeviceTypes, owner);
                if (selectedDeviceTypes == null) return;

                Excel.Workbook workbook = AppManager.ExcelApp.ActiveWorkbook;
                SettingsProfileService.Instance.Import(SettingsProfileService.ManualMode, dialog.FileName,
                    workbook.FullName, workbook.Name, selectedDeviceTypes);
                ProducerList = CreateProducerList();
                CheckIfAnyProducerChecked();
                CheckConfiguration();
                CheckIfDeviceSelected();
                MessageBox.Show("Настройки импортированы.", "Настройки", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось импортировать настройки: " + ex.Message,
                    "Настройки", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyProducerSelection(List<string> selectedProducers)
        {
            if (selectedProducers == null || ProducerList == null)
                return;

            var set = new HashSet<string>(selectedProducers);
            bool previousValue = isApplyingSelection;
            isApplyingSelection = true;
            try
            {
                foreach (var producer in ProducerList)
                    producer.IsSelected = set.Contains(producer.producer);
            }
            finally
            {
                isApplyingSelection = previousValue;
            }
        }
        private void ApplySeriesSelection(List<string> selectedSeries)
        {
            if (selectedSeries == null || SeriesList == null)
                return;

            var set = new HashSet<string>(selectedSeries);
            bool previousValue = isApplyingSelection;
            isApplyingSelection = true;
            try
            {
                foreach (var series in SeriesList)
                    series.IsSelected = set.Contains(series.series);
            }
            finally
            {
                isApplyingSelection = previousValue;
            }
        }
        private List<SeriesModel> CreateSeriesList()  // формирование списка серий оборудования для выбранного производителя для ComboBox
        {
            string tableName = "";
            seriesList = new List<SeriesModel>();
            if (chosenTypeOfDevice == ModularCircuitBreakers || chosenTypeOfDevice == ModularResidualCurrentCircuitBreakers)
            {
                if (chosenTypeOfDevice == ModularCircuitBreakers)
                {
                    tableName = TableNameModularCircuitBreakers;
                }
                else
                {
                    tableName = TableNameModularResidualCurrentCircuitBreakers;
                }
                DBHelper dBHelper = new DBHelper();
                foreach (ProducerModel newProducer in newProducerList)
                {
                    DataSet dsS = dBHelper.GetSeriesDataFromDB(newProducer.producer, tableName);
                    System.Data.DataTable dtS = new System.Data.DataTable();
                    dtS = dsS.Tables[0];

                    for (int i = 0; i < dtS.Rows.Count; i++)
                    {
                        DataRow dr = dtS.NewRow();
                        dr = dtS.Rows[i];
                        SeriesModel seriesModel = new SeriesModel();
                        seriesModel.series = newProducer.producer + ":" + dr["SeriesName"].ToString();
                        seriesModel.PropertyChanged += SeriesModel_PropertyChanged;

                        SeriesList.Add(seriesModel);
                    }
                }
            }
            List<string>[] commonListForSettings = new Configurations().LoadConfiguration(chosenTypeOfDevice);
            if (commonListForSettings[1] != null)
            {
                ApplySeriesSelection(commonListForSettings[1]);

            }
            return SeriesList;
        }

        private void ProducerModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProducerModel.IsSelected))
            {
                bool saveAfterChange = !isApplyingSelection;
                bool seriesListWasAlreadyDisplayed = SeriesList != null && SeriesList.Count > 0;
                List<string> selectedSeries = SeriesList?
                    .Where(series => series.IsSelected)
                    .Select(series => series.series)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();

                // Обновляем список выбранных производителей
                UpdateNewProducerList();

                CheckIfAnyProducerChecked();
                CheckConfiguration();
                CheckIfDeviceSelected();
                // Обновляем SeriesList
                SeriesList = CreateSeriesList();
                if (seriesListWasAlreadyDisplayed && selectedSeries != null)
                {
                    ApplySeriesSelection(selectedSeries);
                    UpdateNewSeriesList();
                }
                if (saveAfterChange)
                {
                    SaveConfiguration();
                }
            }
        }
        private void SeriesModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SeriesModel.IsSelected))
            {
                // Обновляем список выбранных серий
                UpdateNewSeriesList();
                if (!isApplyingSelection)
                {
                    SaveConfiguration();
                }
            }
        }
        private void ChosenTypeOfDevice_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ChosenTypeOfDevice))
            {

               
                // Обновляем список производителей
                ProducerList = CreateProducerList();
                //CreateProducerList();
            }
        }
        private void UpdateNewProducerList()
        {
            // Берём только выбранные элементы
            NewProducerList = ProducerList
                .Where(p => p.IsSelected)
                .ToList();
            CheckIfAnyProducerChecked();
            CheckConfiguration();
            CheckIfDeviceSelected();
            TableOfDevices = new System.Data.DataTable();
            NotifyPropertyChanged(nameof(NewProducerList));


        }
        private void UpdateNewSeriesList()
        {
            // 1) Собираем выбранные модели серий
            NewSeriesList = SeriesList
                .Where(s => s.IsSelected)
                .ToList();

            NotifyPropertyChanged(nameof(NewSeriesList));

            // 2) Формируем List<string> с именами серий
            var seriesNames = NewSeriesList
                .Select(s => s.series)   // или .Series, если свойство называется иначе
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();

            // 3) Вызов метода, который принимает List<string>
            if (seriesNames.Count > 0)
            {
                EquipmentSelection equipmentSelection = new EquipmentSelection();
                var devices = equipmentSelection.SelectDevicecs(seriesNames);

                if (devices != null)
                {
                    DeviceNotFound = "";
                    deviceNotFound = DeviceNotFound;
                    TableOfDevices = devices;

                    // Удаляем колонку "id", если есть
                    if (TableOfDevices.Columns.Contains("id"))
                    {
                        TableOfDevices.Columns.Remove("id");
                    }

                    NotifyPropertyChanged(nameof(TableOfDevices));
                }
                //удалить
                //if (devices.Rows.Count == 0)
                //{
                //    TableOfDevices = new System.Data.DataTable();
                //    TableOfDevices.Columns.Add("NameD", typeof(string));
                //    var row = TableOfDevices.NewRow();
                //    row["NameD"] = "По текущим параметрам оборудование не подобрано";
                //    TableOfDevices.Rows.Add(row);
                //    tableOfDevices = TableOfDevices;

                //    NotifyPropertyChanged(nameof(TableOfDevices));
                //}
                if (devices.Rows.Count == 0)
                {
                    DeviceNotFound = InfoDeviceNotFound;
                    deviceNotFound = DeviceNotFound;
                }
            }
            else
            {
                // очистить таблицу, если ничего не выбрано
                TableOfDevices = null;
                NotifyPropertyChanged(nameof(TableOfDevices));
                DeviceNotFound = "";
                //DeviceNotFound = InfoDeviceNotFound;
                deviceNotFound = DeviceNotFound;
            }
        }

        public void TryAttachToExcel()
        {
            try
            {

                excelWB.SheetSelectionChange += ExcelWB_CellSelectionChange;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подключиться к Excel: " + ex.Message);
            }
        }
        public void DetachFromExcel()
        {
            try
            {
                excelWB.SheetSelectionChange -= ExcelWB_CellSelectionChange;
            }
            catch
            {
                // Excel может быть закрыт, просто игнорируем
            }
        }
        private void ExcelWB_CellSelectionChange(object Sh, Excel.Range Target)
        {
            ChosenTypeOfDevice = new ExcelHelperForEva().GetCellValue();
            CheckIfDeviceSelected();

        }
        private List<string> DataForExcel()
        {
            List<string> deviceDataForExсel = new List<string>();
            if (selectedRow != null)
            {

                string nameOfDevice = SelectedRow["NameD"]?.ToString();
                deviceDataForExсel.Add(nameOfDevice);
                string codeOfDevice = SelectedRow["Code"]?.ToString();
                deviceDataForExсel.Add(codeOfDevice);
                string markOfDevice = SelectedRow["Mark"]?.ToString();
                deviceDataForExсel.Add(markOfDevice);
                string maximumBreakingCapacity = SelectedRow["MaximumBreakingCapacity"]?.ToString();
                deviceDataForExсel.Add(maximumBreakingCapacity);
                string producerOfDevice = SelectedRow["Producer"]?.ToString();
                deviceDataForExсel.Add(producerOfDevice);
                string deviceInfo = SelectedRow["DeviceInfo"]?.ToString();
                deviceDataForExсel.Add(deviceInfo);
            }
            else
            {
                deviceDataForExсel.Add(" ");
                deviceDataForExсel.Add("");
                deviceDataForExсel.Add("");
                deviceDataForExсel.Add("");
                deviceDataForExсel.Add("");
            }
            return deviceDataForExсel;
        }
        public void WriteDeviceDataToExcel()
        {
            List<string> deviceDataForExсel = DataForExcel();
            ExcelHelperForEva excelHelperForEva = new ExcelHelperForEva();
            excelHelperForEva.WhriteDeviceDataToExcel(deviceDataForExсel);

        }

        public ICommand Accept { get; }
        public ICommand Cancel { get; }
        public ICommand ChosenTypeOfDeviceCommand { get; set; }
        public ICommand EquipmentSelection { protected set; get; }
        public ICommand ResetConfigurationCommand { get; set; }
        public ICommand ImportSettingsCommand { get; set; }
        public ICommand ExportSettingsCommand { get; set; }
        public ICommand RefreshCataloguesCommand { get; set; }


    }
}
