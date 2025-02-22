
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text;
using Microsoft.Win32;
using System;

namespace EVA_Catalogue
{
    class SettingsDataBasesVM : INotifyPropertyChanged
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
        private ProducerModel selectedBD;
        public ProducerModel SelectedBD
        {
            get { return selectedBD; }
            set
            {
                selectedBD = value;
                NotifyPropertyChanged("SelectedBD");

            }
        }
        public SettingsDataBasesVM()
        {
            CreateProducerList();

            Accept = new RelayCommand(param => OkCommand()); //проброс команды
            //Cancel = new RelayCommand(param => CancelCommand());
            OpenFileDialogCommand = new RelayCommand(param => OpenFileDialog());
            SaveFileDialogCommand = new RelayCommand(param => SaveFileDialog());
            Delete = new RelayCommand(param => DeleteCommand());
        }
        private List<ProducerModel> CreateProducerList() // формирование списка БД для ComboBox
        {
            producerList = new List<ProducerModel>();
            foreach (string file in Directory.EnumerateFiles(MainViewModel.SourceDirectoryDB, "*.mdf"))
            {
                ProducerModel producerModel = new ProducerModel();
                producerModel.producer = Path.GetFileNameWithoutExtension(file).ToString();
                ProducerList.Add(producerModel);
                
            }
            ProducerList = producerList;
            return ProducerList;
        }

        private void OkCommand()
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
        private void OpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                if (selectedFile.EndsWith(".xlsx") == true)
                {
                    try
                    {
                        DBHelper dBHelper = new DBHelper();
                        dBHelper.AddDataBase(selectedFile);
                        //dBHelper.ToFullFillDataBase(selectedFile);
                        CreateProducerList();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
                // Выполните действия с выбранным файлом
                else
                {
                    MessageBox.Show("Неверный формат файла");
                }
               
            }
        }
        private void SaveFileDialog()
        {
            try
            {
                if (selectedBD != null)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                        FileName = selectedBD.producer // Название файла по умолчанию

                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string selectedFile = saveFileDialog.FileName;
                        // Выполните действия по сохранению файла
                        DBHelper dBHelper = new DBHelper();
                        dBHelper.UploadDB(selectedBD.producer, selectedFile);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        
        }
        private void DeleteCommand()
        {
            DBHelper dBHelper = new DBHelper();
            dBHelper.AddDataBase(selectedBD.producer);
            //dBHelper.ToFullFillDataBase(selectedFile);
            CreateProducerList();
        }

        public ICommand Accept { get; }
        //public ICommand Cancel { get; }
        public ICommand OpenFileDialogCommand { set; get; }
        public ICommand SaveFileDialogCommand { set; get; }
        public ICommand Delete { set; get; }
    }
}
