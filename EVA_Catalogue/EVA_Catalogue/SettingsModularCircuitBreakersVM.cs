
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text;


namespace EVA_Catalogue
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
        public SettingsModularCircuitBreakersVM()
        {
            CreateProducerList();
            LoadSettings();
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
            List<string> producerListForSettings = new List<string>();
            List<string> seriesListForSettings = new List<string>();
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
            else
            {
                seriesListForSettings.Add("%");
            }
            string producerSrtingForSettings = string.Join("#", producerListForSettings);
            string seriesSrtingForSettings = string.Join("#", seriesListWhithProducersForSettings);
            List<string> finalList = new List<string>() { MainViewModel.ModularCircuitBreakersSettings, producerSrtingForSettings, seriesSrtingForSettings };
            string finalString = string.Join("%", finalList);
            string[] finalArray = new string[] { finalString };
            File.WriteAllLines(MainViewModel.SourceDirectorySettings, finalArray);

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
                if (seriesListForSettings[0] != "")
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
            }
            return NewSeriesList;
        }
        private List<ProducerModel> IncludeToNewProduserList()

        {
            if (selectedProducer!= null)
            {
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
            }
            return NewProducerList;
        }
        private List<ProducerModel> ExcludeFromNewProduserList()
        {
            if (selectedNewProducer != null)
            {
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
            }
            return NewProducerList;
        }
        private List<ProducerModel> IncludeToNewSeriesList()

        {
            if (selectedSeries != null)
            {
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
            }
            return NewSeriesList;
        }

        private List<ProducerModel> ExcludeFromNewSeriesList()
        {
            if (selectedNewSeries != null)
            {
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
            }
            return NewSeriesList;
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
            foreach (string file in Directory.EnumerateFiles(MainViewModel.SourceDirectoryDB, "*.mdf"))
            {
                ProducerModel producerModel = new ProducerModel();
                producerModel.producer = Path.GetFileNameWithoutExtension(file).ToString();
                ProducerList.Add(producerModel);
            }
            return ProducerList;
        }
        private List<ProducerModel> CreateProducerListForListBox() // формирование списка производителей для ComboBox
        {
            producerList = new List<ProducerModel>();
            foreach (string file in Directory.EnumerateFiles(MainViewModel.SourceDirectoryDB, "*.mdf"))
            {
                int i = 0;
                foreach (ProducerModel produserFromList in newProducerList)
                {
                    if (produserFromList.producer== Path.GetFileNameWithoutExtension(file).ToString())
                    {
                        i++;
                    }
                }
                if (i == 0)
                {
                    ProducerModel producerModel = new ProducerModel();
                    producerModel.producer = Path.GetFileNameWithoutExtension(file).ToString();
                    ProducerList.Add(producerModel);
                }
            }
            return ProducerList;
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
            DBHelper dBHelper = new DBHelper();
            seriesList = new List<ProducerModel>();
            foreach (ProducerModel newProducer in newProducerList)
            {
                DataSet dsS = dBHelper.GetSeriesDataFromDB(newProducer.producer, MainViewModel.TableNameModularCircuitBreakers);
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
        private void LoadSettings()

        {
            List<string> producerListForSettings = new List<string>();
            List<string> seriesListForSettings = new List<string>();
            try
            {
                using (StreamReader reader = new StreamReader(MainViewModel.SourceDirectorySettings))
                {

                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Split('%')[0] == MainViewModel.ModularCircuitBreakersSettings)
                        {
                            //int x = line.Split('%').Length;
                            string lineWhithProducers= line.Split('%')[1];
                            string lineWhithSries = line.Split('%')[2];

                            foreach (string subLine in lineWhithProducers.Split('#'))
                            {
                                //string a = line.Split(' ')[i].ToString();
                                producerListForSettings.Add(subLine);
                                //string newline = line.Split(' ')[i];
                            }
                            foreach (string subLine in lineWhithSries.Split('#'))
                            {
                                //string a = line.Split(' ')[i].ToString();
                                seriesListForSettings.Add(subLine);
                                //string newline = line.Split(' ')[i];
                            }
                            commonListForSettings[0] = producerListForSettings;
                            commonListForSettings[1] = seriesListForSettings;
                            }
                    }
                }
            }
            catch
            {
                File.Create(MainViewModel.SourceDirectorySettings);
                //string finalArray =  MainViewModel.ModularCircuitBreakersSettings + "%%%";
                //File.WriteAllLines(MainViewModel.SourceDirectorySettings, finalArray);
                //using (StreamWriter sw = new StreamWriter(MainViewModel.SourceDirectorySettings))
                //    sw.WriteLine("Text");
            }
            //return commonListForSettings;
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

            }
            return NewSeriesList;
        }
        private List<ProducerModel> MoveUpSeriesList()

        {
            if (selectedNewSeries != null)
            {
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
            }
            return NewSeriesList;
        }
        private List<ProducerModel> MoveDownProducerList()

        {
            if (selectedNewProducer != null)
            {
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
            }
            return NewProducerList;
        }
        private List<ProducerModel> MoveUpProducerList()

        {
            if (selectedNewProducer != null)
            {
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
       
    }
}

