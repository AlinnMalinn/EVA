
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text;
using System.Linq;
using EVA_Settings;
using EVA_Catalogue_Shared;


namespace EVA_Catalogue.ViewModels
{
    class SettingsModularCircuitBreakersVM: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        List<string>[] commonListForSettings = new List<string>[2];



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
 

        public List<ProducerModel> seriesList;
        public List<ProducerModel> SeriesList
        {
            get { return seriesList; }

            set
            {
                seriesList = value;
                NotifyPropertyChanged("SeriesList");                
            }
        }
        public List<ProducerModel> newSeriesList;
        public List<ProducerModel> NewSeriesList
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
                //SeriesList = CreateSeriesList();
            }
        }
        private ProducerModel selectedNewProducer;
        public ProducerModel SelectedNewProducer
        {
            get { return selectedNewProducer; }
            set
            {
                selectedNewProducer = value;
                NotifyPropertyChanged("SelectedNewProducer");                
            }
        }
        private ProducerModel selectedSeries;
        public ProducerModel SelectedSeries
        {
            get { return selectedSeries; }
            set
            {
                selectedSeries = value;
                NotifyPropertyChanged("SelectedSeries");
            }
        }
        private ProducerModel selectedNewSeries;
        public ProducerModel SelectedNewSeries
        {
            get { return selectedNewSeries; }
            set
            {
                selectedNewSeries = value;
                NotifyPropertyChanged("SelectedNewSeries");
            }
        }
        private bool isSelectedProducer;
        public bool IsSelectedProducer
        {
            get
            {
                return isSelectedProducer;
            }
            set
            {
                isSelectedProducer = value;
                NotifyPropertyChanged("IsSelectedProducer");
            }
        }

        private string windowTitle;

        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                windowTitle = value;                   
            }
            
        }
        public SettingsModularCircuitBreakersVM()
        {
            if (SettingsHelper.Instance.TypeOfDevice == "ModularCircuitBreakers")
            {
                WindowTitle = "Настройка модульных автоматических выключателей";
            }
            else if (SettingsHelper.Instance.TypeOfDevice == "ModularResidualCurrentCircuitBreakers")
            {
                WindowTitle = "Настройка модульных автоматических диф. выключателей";
            }
            CreateProducerList();
            LoadSettings(SettingsHelper.Instance.TypeOfDevice);
            CreatProduserListFromSettings();
            CreatSeriesListFromSettings();

            Accept = new RelayCommand(param => OkCommand()); //проброс команды
            Cancel = new RelayCommand(param => CancelCommand());
            IncludeToNewProduserListCommand = new RelayCommand(param => IncludeToNewProduserList());
            ExcludeFromNewProduserListCommand = new RelayCommand(param => ExcludeFromNewProduserList());
            IncludeToNewSeriesListCommand = new RelayCommand(param => IncludeToNewSeriesList());
            ExcludeFromNewSeriesListCommand = new RelayCommand(param => ExcludeFromNewSeriesList());
            MoveDownSeriesListCommand = new RelayCommand(param => MoveDownSeriesList());
            MoveDownProducerListCommand = new RelayCommand(param => MoveDownProducerList());
            MoveUpSeriesListCommand = new RelayCommand(param => MoveUpSeriesList());
            MoveUpProducerListCommand = new RelayCommand(param => MoveUpProducerList());
            ImportSettingsCommand = new RelayCommand(param => ImportSettings());
            ExportSettingsCommand = new RelayCommand(param => ExportSettings());
        }

        private void CancelCommand()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }
        private void OkCommand()
        {
            PathHelper pathHelper = new PathHelper();
            string sourceDirectorySettings = pathHelper.PathSettingsHelper();
            List<string> producerListForSettings = new List<string>();
            List<string> seriesListWhithProducersForSettings = new List<string>();

            if (newProducerList.Count != 0)
            {
                foreach (ProducerModel newProducer in newProducerList)
                {
                    producerListForSettings.Add(newProducer.producer.ToString());
                }
            }
            else
            {
                producerListForSettings.Add("%");
            }

            if (newSeriesList.Count != 0)
            {
                foreach (ProducerModel newSeries in newSeriesList)
                {
                    seriesListWhithProducersForSettings.Add(newSeries.series.ToString());
                }
            }

            Excel.Workbook workbook = AppManager.ExcelApp.ActiveWorkbook;
            SettingsProfileService.Instance.SaveSelection(
                SettingsProfileService.AutomaticMode,
                SettingsHelper.Instance.TypeOfDevice,
                producerListForSettings,
                seriesListWhithProducersForSettings,
                workbook.FullName,
                workbook.Name,
                sourceDirectorySettings);

            // Закрываем текущее окно
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }


        private List<ProducerModel> CreatProduserListFromSettings()
        {
            List<string> producerListForSettings = commonListForSettings[0];
            newProducerList = new List<ProducerModel>();
            if (producerListForSettings != null)
            {
                foreach (string produserForSettings in producerListForSettings)
                {
                    foreach (ProducerModel produserFromDB in producerList)
                    {
                        if (produserForSettings == produserFromDB.producer)
                        {
                            ProducerModel producerModel = new ProducerModel();
                            producerModel.producer = produserForSettings.ToString();
                            NewProducerList.Add(producerModel);
                        }
                    }
                }
                NewProducerList = newProducerList;
                ProducerList = CreateProducerListForListBox();
                SeriesList = CreateSeriesList();
            }
            return NewProducerList;
        }
        private List<ProducerModel> CreatSeriesListFromSettings()
        {
            List<string> seriesListForSettings = commonListForSettings[1];
            newSeriesList = new List<ProducerModel>();
            if (seriesListForSettings != null)
            {
                if (seriesListForSettings.Count > 0 &&
                    !string.IsNullOrEmpty(seriesListForSettings[0]))
                {
                    foreach (string seriesForSettings in seriesListForSettings)
                    {
                        foreach (ProducerModel seriesFromDB in seriesList)
                        {
                            if (seriesForSettings == seriesFromDB.series)
                            {
                                ProducerModel producerModel = new ProducerModel();
                                producerModel.series = seriesForSettings.ToString();
                                NewSeriesList.Add(producerModel);
                                break;
                            }
                        }
                    }

                    SeriesList = CreateSeriesListForListBox();
                }
                NewSeriesList = newSeriesList;
            }
            return NewSeriesList;
        }
        private List<ProducerModel> IncludeToNewProduserList()

        {
            if (selectedProducer!= null)
            {
                int selectedIndex = ProducerList?.IndexOf(selectedProducer) ?? -1;
                List<string> producerListForNewList = new List<string>();
                foreach (ProducerModel newProducer in newProducerList)
                {
                    producerListForNewList.Add(newProducer.producer.ToString());
                }
                producerListForNewList.Add(selectedProducer.producer.ToString());
                newProducerList = new List<ProducerModel>();
                foreach (string newPoducer in producerListForNewList)
                {
                    ProducerModel producerModel = new ProducerModel();
                    producerModel.producer = newPoducer.ToString();
                    NewProducerList.Add(producerModel);
                }
                NewProducerList = newProducerList;
                SeriesList = CreateSeriesList();
                NewSeriesList = UpdateNewSeriesList();
                SeriesList = CreateSeriesListForListBox();

                ProducerList = CreateProducerListForListBox();
                SelectedProducer = GetNextSelection(ProducerList, selectedIndex);
            }
            return NewProducerList;
        }
        private List<ProducerModel> ExcludeFromNewProduserList()
        {
            if (selectedNewProducer != null)
            {
                int selectedIndex = NewProducerList?.IndexOf(selectedNewProducer) ?? -1;
                List<string> producerListForNewList = new List<string>();
                foreach (ProducerModel newProducer in newProducerList)
                {
                    if (newProducer.producer != selectedNewProducer.producer)
                    {
                        producerListForNewList.Add(newProducer.producer.ToString());
                    }
                }

                newProducerList = new List<ProducerModel>();
                foreach (string newPoducer in producerListForNewList)
                {
                    ProducerModel producerModel = new ProducerModel();
                    producerModel.producer = newPoducer.ToString();
                    NewProducerList.Add(producerModel);
                }
                NewProducerList = newProducerList;
                SeriesList = CreateSeriesList();
                NewSeriesList = UpdateNewSeriesList();
                SeriesList = CreateSeriesListForListBox();
                ProducerList = CreateProducerListForListBox();
                SelectedNewProducer = GetNextSelection(NewProducerList, selectedIndex);
            }
            return NewProducerList;
        }
        private List<ProducerModel> IncludeToNewSeriesList()

        {
            if (selectedSeries != null)
            {
                int selectedIndex = SeriesList?.IndexOf(selectedSeries) ?? -1;
                List<string> seriesListForNewList = new List<string>();
                foreach (ProducerModel newSeries in newSeriesList)
                {
                    seriesListForNewList.Add(newSeries.series.ToString());
                }
                seriesListForNewList.Add(selectedSeries.series.ToString());
                newSeriesList = new List<ProducerModel>();
                foreach (string newSeries in seriesListForNewList)
                {
                    ProducerModel producerModel = new ProducerModel();
                    producerModel.series = newSeries.ToString();
                    NewSeriesList.Add(producerModel);
                }
                NewSeriesList = newSeriesList;
                SeriesList = CreateSeriesList();
                SeriesList = CreateSeriesListForListBox();
                SelectedSeries = GetNextSelection(SeriesList, selectedIndex);
            }
            return NewSeriesList;
        }

        private List<ProducerModel> ExcludeFromNewSeriesList()
        {
            if (selectedNewSeries != null)
            {
                int selectedIndex = NewSeriesList?.IndexOf(selectedNewSeries) ?? -1;
                List<string> seriesListForNewList = new List<string>();
                foreach (ProducerModel newSeries in newSeriesList)
                {
                    if (newSeries.series != selectedNewSeries.series)
                    {
                        seriesListForNewList.Add(newSeries.series.ToString());
                    }
                }


                newSeriesList = new List<ProducerModel>();
                foreach (string newSeries in seriesListForNewList)
                {
                    ProducerModel producerModel = new ProducerModel();
                    producerModel.series = newSeries.ToString();
                    NewSeriesList.Add(producerModel);
                }
                NewSeriesList = newSeriesList;
                SeriesList = CreateSeriesList();
                SeriesList = CreateSeriesListForListBox();
                SelectedNewSeries = GetNextSelection(NewSeriesList, selectedIndex);
            }
            return NewSeriesList;
        }

        private static ProducerModel GetNextSelection(List<ProducerModel> items, int removedIndex)
        {
            if (items == null || items.Count == 0)
                return null;

            int nextIndex = removedIndex < 0 ? 0 : System.Math.Min(removedIndex, items.Count - 1);
            return items[nextIndex];
        }


        //private List<ProducerModel> IncludeToNewProduserList()
        //{
        //    //newProducerList = new List<ProducerModel>();
        //    ProducerModel producerModel = new ProducerModel();
        //    producerModel.producer = selectedProducer.producer.ToString();
        //    //NewProducerList = newProducerList;
        //    NewProducerList.Add(producerModel);
        //    NewProducerList = newProducerList;
        //    SeriesList = CreateSeriesList();
        //    ProducerList = CreateProducerListForListBox();

        //    return NewProducerList;
        //}


        private List<ProducerModel> CreateProducerList() // формирование списка производителей для ComboBox
        {
            producerList = new List<ProducerModel>();
            PathHelper pathHelper = new PathHelper();
            string sourceDirectoryDB = pathHelper.PathDBHelper();
            try
            {
                CatalogueCacheService.Instance.ConfigureDirectory(sourceDirectoryDB);
                string requiredSheet = GetRequiredCatalogueSheet();
                foreach (string producer in GetProducerNamesWithWait(requiredSheet))
                {
                    ProducerModel producerModel = new ProducerModel();
                    producerModel.producer = producer;
                    ProducerList.Add(producerModel);
                }
                return ProducerList;
            }
            catch {
                return ProducerList;
            }
        }
        private List<ProducerModel> CreateProducerListForListBox() // формирование списка производителей для ComboBox
        {
            PathHelper pathHelper = new PathHelper();
            string sourceDirectoryDB = pathHelper.PathDBHelper();
            producerList = new List<ProducerModel>();
            CatalogueCacheService.Instance.ConfigureDirectory(sourceDirectoryDB);
            string requiredSheet = GetRequiredCatalogueSheet();
            foreach (string producer in GetProducerNamesWithWait(requiredSheet))
            {
                int i = 0;
                foreach (ProducerModel produserFromList in newProducerList)
                {
                    if (produserFromList.producer == producer)
                    {
                        i++;
                    }
                }
                if (i == 0)
                {
                    ProducerModel producerModel = new ProducerModel();
                    producerModel.producer = producer;
                    ProducerList.Add(producerModel);
                }
            }
            return ProducerList;
        }

        private static string GetRequiredCatalogueSheet()
        {
            return SettingsHelper.Instance.TypeOfDevice == MainViewModel.ModularResidualCurrentCircuitBreakersSettings
                ? "QFD"
                : "QF";
        }

        private static IReadOnlyList<string> GetProducerNamesWithWait(string requiredSheet)
        {
            var previousCursor = System.Windows.Input.Mouse.OverrideCursor;
            try
            {
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                return CatalogueCacheService.Instance.GetProducerNames(requiredSheet);
            }
            finally
            {
                System.Windows.Input.Mouse.OverrideCursor = previousCursor;
            }
        }
        private List<ProducerModel> CreateSeriesListForListBox() // формирование списка производителей для ComboBox
        {
            List<string> seriesListForNewList = new List<string>();
            foreach (ProducerModel series in seriesList)
            {
                seriesListForNewList.Add(series.series.ToString());
            }
            seriesList = new List<ProducerModel>();
            foreach (string seriesList in seriesListForNewList)
            {
                int i = 0;
                foreach (ProducerModel seriesFromList in newSeriesList)
                {
                    if (seriesList == seriesFromList.series)
                    {
                        i++;
                    }
                }
                if (i == 0)
                {
                    ProducerModel producerModel = new ProducerModel();
                    producerModel.series = seriesList;
                    SeriesList.Add(producerModel);
                }
            }
            return SeriesList;
        }
        private List<ProducerModel> CreateSeriesList()  // формирование списка серий оборудования для выбранного производителя для ComboBox
        {
            string tableName = "";
            if (SettingsHelper.Instance.TypeOfDevice == "ModularCircuitBreakers")
            {
                tableName = MainViewModel.TableNameModularCircuitBreakers;
            }
            else if (SettingsHelper.Instance.TypeOfDevice == "ModularResidualCurrentCircuitBreakers")
            {
                tableName = MainViewModel.TableNameModularResidualCurrentCircuitBreakers;
            }
            DBHelper dBHelper = new DBHelper();
            seriesList = new List<ProducerModel>();
            foreach (ProducerModel newProducer in newProducerList)
            {
                DataSet dsS = dBHelper.GetSeriesDataFromDB(newProducer.producer, tableName);
                DataTable dtS = new DataTable();
                dtS = dsS.Tables[0];    

                for (int i = 0; i < dtS.Rows.Count; i++)
                {
                    DataRow dr = dtS.NewRow();
                    dr = dtS.Rows[i];
                    ProducerModel producerModel = new ProducerModel();
                    producerModel.series = newProducer.producer+":" + dr["SeriesName"].ToString();
                    //producerModel.seriesID = (int)dr["id"];
                    SeriesList.Add(producerModel);
                }
            }
            return SeriesList;
        }
        private void LoadSettings(string typeOfDevice)

        {
            List<string> producerListForSettings = new List<string>();
            List<string> seriesListForSettings = new List<string>();
            PathHelper pathHelper = new PathHelper();
            string sourceDirectorySettings = pathHelper.PathSettingsHelper();
            Excel.Workbook workbook = AppManager.ExcelApp.ActiveWorkbook;
            DeviceSelectionSettings selection = SettingsProfileService.Instance.GetSelection(
                SettingsProfileService.AutomaticMode, typeOfDevice,
                workbook.FullName, workbook.Name, sourceDirectorySettings);
            commonListForSettings[0] = selection.Producers;
            commonListForSettings[1] = selection.Series;
            //return commonListForSettings;
        }

        private void ExportSettings()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Настройки EVA (*.evasettings)|*.evasettings",
                DefaultExt = ".evasettings",
                AddExtension = true,
                FileName = "EVA_Автоматический_" + System.DateTime.Now.ToString("yyyy-MM-dd")
            };
            if (dialog.ShowDialog() != true) return;
            Excel.Workbook workbook = AppManager.ExcelApp.ActiveWorkbook;
            SettingsProfileService.Instance.SaveSelection(
                SettingsProfileService.AutomaticMode,
                SettingsHelper.Instance.TypeOfDevice,
                (newProducerList ?? new List<ProducerModel>())
                    .Where(item => item != null && !string.IsNullOrWhiteSpace(item.producer))
                    .Select(item => item.producer),
                (newSeriesList ?? new List<ProducerModel>())
                    .Where(item => item != null && !string.IsNullOrWhiteSpace(item.series))
                    .Select(item => item.series),
                workbook.FullName,
                workbook.Name,
                new PathHelper().PathSettingsHelper());
            SettingsProfileService.Instance.Export(SettingsProfileService.AutomaticMode, dialog.FileName,
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
                    SettingsProfileService.AutomaticMode, dialog.FileName);
                if (availableDeviceTypes.Count == 0)
                    throw new System.IO.InvalidDataException("В файле нет настроек оборудования.");
                Window owner = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window.IsActive);
                List<string> selectedDeviceTypes = SettingsImportDeviceDialog.Show(availableDeviceTypes, owner);
                if (selectedDeviceTypes == null) return;

                Excel.Workbook workbook = AppManager.ExcelApp.ActiveWorkbook;
                SettingsProfileService.Instance.Import(SettingsProfileService.AutomaticMode, dialog.FileName,
                    workbook.FullName, workbook.Name, selectedDeviceTypes);
                commonListForSettings = new List<string>[2];
                ProducerList = CreateProducerList();
                LoadSettings(SettingsHelper.Instance.TypeOfDevice);
                CreatProduserListFromSettings();
                CreatSeriesListFromSettings();
                MessageBox.Show("Настройки импортированы.", "Настройки", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Не удалось импортировать настройки: " + ex.Message,
                    "Настройки", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private List<ProducerModel> UpdateNewSeriesList()  // формирование списка серий оборудования для выбранного производителя для ComboBox
        {
            List<string> seriesListForNewList = new List<string>();
            foreach (ProducerModel series in newSeriesList)
            {
                seriesListForNewList.Add(series.series.ToString());
            }
            newSeriesList = new List<ProducerModel>();
            foreach (string series2 in seriesListForNewList)
            {
                
                foreach (ProducerModel seriesFromList in seriesList)
                {
                    if (series2== seriesFromList.series)
                    {
                        ProducerModel producerModel = new ProducerModel();
                        producerModel.series = series2;
                        NewSeriesList.Add(producerModel);
                        break;
                    }
                }
            }
            return NewSeriesList;
        }
        private List<ProducerModel> MoveDownSeriesList()

        { 
            if (selectedNewSeries != null)
            {
                List<string> seriesListForNewList1 = new List<string>();
                string selected = selectedNewSeries.series;
                foreach (ProducerModel newSeries in newSeriesList)
                {
                    seriesListForNewList1.Add(newSeries.series.ToString());
                }
                List<string> seriesListForNewList2 = new List<string>(new string[seriesListForNewList1.Count]);
                newSeriesList = new List<ProducerModel>();
                int i = 0;
                while (i < (seriesListForNewList1.Count))
                {
                    if (i == (seriesListForNewList1.Count - 1) & (seriesListForNewList1[i] == selectedNewSeries.series))
                    {
                        seriesListForNewList2[i] = seriesListForNewList1[i];
                        i++;
                    }
                    else if (seriesListForNewList1[i] == selectedNewSeries.series)
                    {
                        seriesListForNewList2[i] = seriesListForNewList1[i + 1];
                        seriesListForNewList2[i + 1] = seriesListForNewList1[i];
                        i++;
                        i++;
                    }
                    else
                    {
                        seriesListForNewList2[i] = seriesListForNewList1[i];
                        i++;
                    }
                }
                //ProducerModel producerModel = new ProducerModel();
                foreach (string series in seriesListForNewList2)
                {
                    ProducerModel producerModel = new ProducerModel();
                    producerModel.series = series;
                    NewSeriesList.Add(producerModel);
                }
                //ProducerModel producerModel1 = new ProducerModel();
                //producerModel1.series = selected;
                //SelectedNewSeries = producerModel1;

                //SelectedNewSeries = selectedNewSeries;
                NewSeriesList = newSeriesList;
                SelectedNewSeries = NewSeriesList.FirstOrDefault(item => item.series == selected);

            }
            return NewSeriesList;
        }
        private List<ProducerModel> MoveUpSeriesList()

        {
            if (selectedNewSeries != null)
            {
                string selected = selectedNewSeries.series;
                List<string> seriesListForNewList1 = new List<string>();

                foreach (ProducerModel newSeries in newSeriesList)
                {
                    seriesListForNewList1.Add(newSeries.series.ToString());
                }
                List<string> seriesListForNewList2 = new List<string>(new string[seriesListForNewList1.Count]);
                newSeriesList = new List<ProducerModel>();
                int i = seriesListForNewList1.Count-1;
                while (i >=0 )
                {
                    if (i == 0 & (seriesListForNewList1[i] == selectedNewSeries.series))
                    {
                        seriesListForNewList2[i] = seriesListForNewList1[i];
                        i--;
                    }
                    else if (seriesListForNewList1[i] == selectedNewSeries.series)
                    {
                        seriesListForNewList2[i] = seriesListForNewList1[i - 1];
                        seriesListForNewList2[i - 1] = seriesListForNewList1[i];
                        i--;
                        i--;
                    }
                    else
                    {
                        seriesListForNewList2[i] = seriesListForNewList1[i];
                        i--;
                    }
                }
                foreach (string series in seriesListForNewList2)
                {
                    ProducerModel producerModel = new ProducerModel();
                    producerModel.series = series;
                    NewSeriesList.Add(producerModel);
                }
                NewSeriesList = newSeriesList;
                SelectedNewSeries = NewSeriesList.FirstOrDefault(item => item.series == selected);
            }
            return NewSeriesList;
        }
        private List<ProducerModel> MoveDownProducerList()

        {
            if (selectedNewProducer != null)
            {
                string selected = selectedNewProducer.producer;
                List<string> producerListForNewList1 = new List<string>();

                foreach (ProducerModel newProducer in newProducerList)
                {
                    producerListForNewList1.Add(newProducer.producer.ToString());
                }
                List<string> producerListForNewList2 = new List<string>(new string[producerListForNewList1.Count]);
                newProducerList = new List<ProducerModel>();
                int i = 0;
                while (i < (producerListForNewList1.Count))
                {
                    if (i == (producerListForNewList1.Count - 1) & (producerListForNewList1[i] == selectedNewProducer.producer))
                    {
                        producerListForNewList2[i] = producerListForNewList1[i];
                        i++;
                    }
                    else if (producerListForNewList1[i] == selectedNewProducer.producer)
                    {
                        producerListForNewList2[i] = producerListForNewList1[i + 1];
                        producerListForNewList2[i + 1] = producerListForNewList1[i];
                        i++;
                        i++;
                    }
                    else
                    {
                        producerListForNewList2[i] = producerListForNewList1[i];
                        i++;
                    }
                }
                foreach (string producer in producerListForNewList2)
                {
                    ProducerModel producerModel = new ProducerModel();
                    producerModel.producer = producer;
                    NewProducerList.Add(producerModel);
                }
                NewProducerList = newProducerList;
                SelectedNewProducer = NewProducerList.FirstOrDefault(item => item.producer == selected);
            }
            return NewProducerList;
        }
        private List<ProducerModel> MoveUpProducerList()

        {
            if (selectedNewProducer != null)
            {
                string selected = selectedNewProducer.producer;
                List<string> producerListForNewList1 = new List<string>();

                foreach (ProducerModel newProducer in newProducerList)
                {
                    producerListForNewList1.Add(newProducer.producer.ToString());
                }
                List<string> producerListForNewList2 = new List<string>(new string[producerListForNewList1.Count]);
                newProducerList = new List<ProducerModel>();
                int i = producerListForNewList1.Count-1;
                while (i >=0)
                {
                    if (i == 0 & (producerListForNewList1[i] == selectedNewProducer.producer))
                    {
                        producerListForNewList2[i] = producerListForNewList1[i];
                        i--;
                    }
                    else if (producerListForNewList1[i] == selectedNewProducer.producer)
                    {
                        producerListForNewList2[i] = producerListForNewList1[i - 1];
                        producerListForNewList2[i - 1] = producerListForNewList1[i];
                        i--;
                        i--;
                    }
                    else
                    {
                        producerListForNewList2[i] = producerListForNewList1[i];
                        i--;
                    }
                }
                foreach (string producer in producerListForNewList2)
                {
                    ProducerModel producerModel = new ProducerModel();
                    producerModel.producer = producer;
                    NewProducerList.Add(producerModel);
                }
                NewProducerList = newProducerList;
                SelectedNewProducer = NewProducerList.FirstOrDefault(item => item.producer == selected);
            }
            return NewProducerList;
        }




        public ICommand Accept { get; }
        public ICommand Cancel { get; }
        public ICommand IncludeToNewProduserListCommand { set; get; }
        public ICommand ExcludeFromNewProduserListCommand { set; get; }
        public ICommand IncludeToNewSeriesListCommand { set; get; }
        public ICommand ExcludeFromNewSeriesListCommand { set; get; }
        public ICommand MoveDownSeriesListCommand { set; get; }
        public ICommand MoveDownProducerListCommand { set; get; }
        public ICommand MoveUpSeriesListCommand { set; get; }
        public ICommand MoveUpProducerListCommand { set; get; }
        public ICommand ImportSettingsCommand { get; set; }
        public ICommand ExportSettingsCommand { get; set; }
       
    }
}

