
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
using EVA_Catalogue;


namespace EVA_Catalogue_DataBaseSetting
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public const string TableNameModularCircuitBreakers = @"[Модульные автоматические выключатели]";
        public const string TableNameModularResidualCurrentCircuitBreakers = @"[Модульные автоматические выключатели дифференциального тока]";

        public const string ModularCircuitBreakersSettings = "ModularCircuitBreakers";
        public const string ModularResidualCurrentCircuitBreakersSettings = "ModularResidualCurrentCircuitBreakers";

        public static string currentTableFromDB;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public bool isExcelDataAvailable;
        public bool IsExcelDataAvailable
        {
            get { return isExcelDataAvailable; }
            set
            {
                isExcelDataAvailable = value;
                NotifyPropertyChanged("IsExcelDataAvailable");
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
            }
        }



        public MainViewModel()
        {
            IsExcelDataAvailable = new PathHelper().CheckLinkForDB();
            LinkForDB = new PathHelper().GetLinkForDB();
            Accept = new RelayCommand(param => OkCommand()); //проброс команды
            Cancel = new RelayCommand(param => CancelCommand());
            
            SaveFolderDialogCommand = new RelayCommand(param => SaveFolderDialog());           
            //CheckExcelDataCommand = new RelayCommand(param =>
            //{
            //    IsExcelDataAvailable = new PathHelper().CheckLinkForDB();
            //});
            LinkForDBCommand = new RelayCommand(param =>
            {
                LinkForDB = new PathHelper().GetLinkForDB();
            });
           

        }

        private void CancelCommand()
        {
            System.Windows.Application.Current.MainWindow.Close();


        }
        private void OkCommand()
        {

            System.Windows.Application.Current.MainWindow.Close();

        }
      
        
        private void SaveFolderDialog()
        {
            try
            {
                string folderPath = string.Empty;

                // Пытаемся использовать современный CommonOpenFileDialog
                try
                {
                    var modernDialog = new CommonOpenFileDialog
                    {
                        IsFolderPicker = true,
                        Title = "Выберите папку для сохранения базы данных"
                    };

                    if (modernDialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        folderPath = modernDialog.FileName;
                    }
                }
                catch
                {
                    // Если не удалось → fallback на классический FolderBrowserDialog
                    using (var classicDialog = new FolderBrowserDialog())
                    {
                        classicDialog.Description = "Выберите папку для сохранения базы данных";
                        classicDialog.ShowNewFolderButton = true;

                        if (classicDialog.ShowDialog() == DialogResult.OK)
                        {
                            folderPath = classicDialog.SelectedPath;
                        }
                    }
                }

                // Если пользователь выбрал папку → сохраняем путь
                if (!string.IsNullOrEmpty(folderPath))
                {
                    PathHelper path = new PathHelper();
                    path.SaveLinkForDB(folderPath);
                    IsExcelDataAvailable = path.CheckLinkForDB();
                    LinkForDB = path.GetLinkForDB();
                }

                // Вернуть фокус окну приложения
                //  Application.Current.MainWindow.Activate();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
          
    
        public ICommand Accept { get; }
        public ICommand Cancel { get; }
        
        public ICommand SaveFolderDialogCommand { set; get; }
        //public ICommand CheckExcelDataCommand { get; set; }
        public ICommand LinkForDBCommand { get; set; }

    }
}
