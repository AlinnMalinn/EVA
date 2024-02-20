
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace EVA_Catalogue
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
  
        public List<ProducerModel> producerList;
        public List<ProducerModel> ProducerList
        {
            get { return producerList; }

            set
            {
                producerList = value;
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
   
        private ProducerModel selectedProducer;
        public ProducerModel SelectedProducer
        {
            get { return selectedProducer; }
            set
            {
                selectedProducer = value;
                NotifyPropertyChanged("SelectedProducer");
                SeriesList=CreateSeriesList(selectedProducer);
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

        public MainViewModel()
        {
            Accept = new RelayCommand(param => OkCommand()); //проброс команды
            Cancel = new RelayCommand(param => CancelCommand());
            EquipmentSelection = new RelayCommand(param => SayResult());
            CreateProducerList();

        }
           
        private void CancelCommand()
        {
            Application.Current.MainWindow.Close();
        }
        private void OkCommand()
        {
            
            Application.Current.MainWindow.Close();
        }                                 

        private void SayResult() // подбор оборудования и вывод результатов

        {

            if ((selectedProducer == null) & (selectedSeries == null))
            {
                MessageBox.Show("Производитель оборудования не выбран");
            }
            else if ((selectedProducer != null) & (selectedSeries == null))
            {
                MessageBox.Show("Серия оборудования не выбрана");
            }
            else
                try
                {
                    EquipmentSelection es = new EquipmentSelection();
                    es.selectDevicecs(selectedProducer.producer, selectedSeries.seriesID);
                    MessageBox.Show("Оборудование подобрано");
                }
                catch
                {
                    MessageBox.Show("Возникла ошибка =(");
                }
        }

        private List<ProducerModel> CreateProducerList() // формирование списка производителей для ComboBox
        {
            producerList = new List<ProducerModel>();
            string sourceDirectory = @"C:\Users\79126\source\EVA\EVA_Catalogue\EVA_Catalogue";
            foreach (string file in Directory.EnumerateFiles(sourceDirectory, "*.mdf"))
            {
                ProducerModel producerModel = new ProducerModel();
                producerModel.producer=Path.GetFileNameWithoutExtension(file).ToString();
                ProducerList.Add(producerModel);
            }
            return ProducerList;
        }

        private List<ProducerModel> CreateSeriesList(ProducerModel selectedProducer)  // формирование списка серий оборудования для выбранного производителя для ComboBox
        {
            DBHelper dBHelper = new DBHelper();
            DataSet dsS = dBHelper.GetSeriesDataFromDB(selectedProducer.producer); 

            DataTable dtS = new DataTable();

            dtS = dsS.Tables[0];
            seriesList = new List<ProducerModel>();

            for (int i = 0; i < dtS.Rows.Count; i++)
            {
                DataRow dr = dtS.NewRow();
                dr = dtS.Rows[i];
                ProducerModel producerModel = new ProducerModel();
                producerModel.series = dr["SeriesName"].ToString();
                producerModel.seriesID = (int)dr["id"];
                SeriesList.Add(producerModel);
            }
            return SeriesList;
        }

        public ICommand EquipmentSelection { protected set; get; }
        public ICommand Accept { get; }
        public ICommand Cancel { get; }
    }

}
