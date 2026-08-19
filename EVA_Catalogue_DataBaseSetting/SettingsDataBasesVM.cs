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
using System.Windows.Forms; // для FolderBrowserDialog
using EVA_Catalogue;


namespace EVA_Catalogue_DataBaseSetting
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
            PathHelper pathHelper = new PathHelper();
            string sourceDirectoryDB = pathHelper.PathDBHelper();
            try
            {
                foreach (string file in Directory.EnumerateFiles(sourceDirectoryDB, "*.mdf"))
                {
                    ProducerModel producerModel = new ProducerModel();
                    producerModel.producer = Path.GetFileNameWithoutExtension(file).ToString();
                    ProducerList.Add(producerModel);

                }
                ProducerList = producerList;
            }
            catch
            {

            }
            return ProducerList;
        }

        private void OkCommand()
        {
            foreach (Window window in System.Windows.Application.Current.Windows
)
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
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                if (selectedFile.EndsWith(".xlsx") == true)
                {
                    try
                    {
                        Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                        DBHelper dBHelper = new DBHelper();
                        dBHelper.AddDataBase(selectedFile);
                        //dBHelper.ToFullFillDataBase(selectedFile);
                        CreateProducerList();
                        Mouse.OverrideCursor = null;

                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Error: " + ex.Message);
                    }
                }
                // Выполните действия с выбранным файлом
                else
                {
                    System.Windows.MessageBox.Show("Неверный формат файла");
                }
            }
        }
        private void SaveFileDialog()
        {
            try
            {
                if (selectedBD != null)
                {
                    Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                        FileName = selectedBD.producer // Название файла по умолчанию

                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                        string selectedFile = saveFileDialog.FileName;
                        // Выполните действия по сохранению файла
                        DBHelper dBHelper = new DBHelper();
                        dBHelper.UploadDB(selectedBD.producer, selectedFile);
                        Mouse.OverrideCursor = null;

                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error: " + ex.Message);
            }

        }
        private void DeleteCommand()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            DBHelper dBHelper = new DBHelper();
            if (selectedBD != null)
            {
                dBHelper.DeleteDataBase(selectedBD.producer);
                //dBHelper.ToFullFillDataBase(selectedFile);
                CreateProducerList();

            }
            Mouse.OverrideCursor = null;
        }

        public ICommand Accept { get; }
        //public ICommand Cancel { get; }
        public ICommand OpenFileDialogCommand { set; get; }
        public ICommand SaveFileDialogCommand { set; get; }
        public ICommand Delete { set; get; }
    }
}
