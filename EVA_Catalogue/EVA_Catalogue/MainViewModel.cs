﻿
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

namespace EVA_Catalogue
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public const string SourceDirectoryDB = @"C:\Users\79126\source\EVA\EVA_Catalogue\EVA_Catalogue";
        public const string SourceDirectoryDB2 = "C:\\Users\\79126\\source\\EVA\\EVA_Catalogue\\EVA_Catalogue";
        public const string SourceDirectorySettings = @"C:\Users\79126\source\EVA\EVA_Catalogue\EVA_Catalogue\Settings.txt";
        public const string SourceDirectoryExcel = @"C:\Users\79126\Desktop\EVA\TEST2.xlsm";
        //public const string ExcelPage = "EVA_1РП1";

        public const string TableNameModularCircuitBreakers = @"[Модульные автоматические выключатели]";
        public const string TableNameModularResidualCurrentCircuitBreakers = @"[Модульные автоматические выключатели дифференциального тока]";

        public const string ModularCircuitBreakersSettings = "ModularCircuitBreakers";
        public const string ModularResidualCurrentCircuitBreakersSettings = "ModularResidualCurrentCircuitBreakers";

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
        
        public MainViewModel()
        {
            Accept = new RelayCommand(param => OkCommand()); //проброс команды
            Cancel = new RelayCommand(param => CancelCommand());
            EquipmentSelection = new RelayCommand(param => SayResult());

            OpenWindowSettingsModularCircuitBreakersCommand = new RelayCommand(param => OpenWindowSettingsModularCircuitBreakers());
            OpenWindowSettingsModularResidualCurrentBreakersCommand = new RelayCommand(param => OpenWindowSettingsModularResidualCurrentBreakers());
            OpenWindowSettingsDataBasesCommand = new RelayCommand(param => OpenWindowSettingsDataBases());
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
            if (IsAutomaticSelectionEnabledForModularCircuitBreakers == true)
            {
                MessageBox.Show("Проверка положительная");
            }
            else
            {
                MessageBox.Show("Проверка отрицательная");
            }
            List<string> producerListForSettings = new List<string>();
            List<string> seriesListForSettings = new List<string>();
            List<string> producerForSeriesListForSettings = new List<string>();
            if (File.Exists(MainViewModel.SourceDirectorySettings))
                try
                {
                    using (StreamReader reader = new StreamReader(MainViewModel.SourceDirectorySettings))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Split('%')[0] == MainViewModel.ModularCircuitBreakersSettings)
                            {
                                string lineWhithProducers = line.Split('%')[1];
                                string lineWhithSries = line.Split('%')[2];

                                foreach (string subLine in lineWhithProducers.Split('#'))
                                {
                                    producerListForSettings.Add(subLine);
                                }
                                foreach (string subLine in lineWhithSries.Split('#'))
                                {
                                    //producerForSeriesListForSettings.Add(subLine.Split(':')[0]);
                                    seriesListForSettings.Add(subLine);
                                }
                            }
                        }

                        
                            EquipmentSelection es = new EquipmentSelection();
                            es.SelectDevicecs_ModularCircuitBreaker(producerListForSettings, seriesListForSettings);
                        
                        MessageBox.Show("Оборудование подобрано");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            //    if ((selectedProducer == null) & (selectedSeries == null))
            //    {
            //        MessageBox.Show("Производитель оборудования не выбран");
            //    }
            else
                {
                MessageBox.Show("Производители и серии оборудования не выбраны, настройте параметры подбора");
            }
        //    else
        //        try
        //        {
        //            EquipmentSelection es = new EquipmentSelection();
        //            es.SelectDevicecs(selectedProducer.producer, selectedSeries.seriesID);
        //            MessageBox.Show("Оборудование подобрано");
        //        }
        //        catch
        //        {
        //            MessageBox.Show("Возникла ошибка =(");
        //        }
        }

        


        private void  OpenWindowSettingsModularCircuitBreakers()
        {
            //int i = 0;
            //foreach (Window window in Application.Current.Windows)
            //{

            //    if (window.DataContext == this)
            //    {
            //        i++;

            //    }
            //}
            //if (i == 0)
            {
                WindowSettingsModularCircuitBreakers windowSettingsModularCircuitBreakers = new WindowSettingsModularCircuitBreakers();
                windowSettingsModularCircuitBreakers.ShowDialog();
            }

        }
    private void OpenWindowSettingsModularResidualCurrentBreakers()
        {
            WindowSettingsModularResidualCurrentBreakers windowSettingsModularResidualCurrentBreakers = new WindowSettingsModularResidualCurrentBreakers();
            windowSettingsModularResidualCurrentBreakers.ShowDialog();

        }
        private void OpenWindowSettingsDataBases()
        {
            WindowSettingsDataBases windowSettingsDataBases = new WindowSettingsDataBases();
            windowSettingsDataBases.ShowDialog();
        }
        public ICommand EquipmentSelection { protected set; get; }
        public ICommand Accept { get; }
        public ICommand Cancel { get; }
        public ICommand OpenWindowSettingsModularCircuitBreakersCommand { set; get; }
        public ICommand OpenWindowSettingsModularResidualCurrentBreakersCommand { get; }
        public ICommand OpenWindowSettingsDataBasesCommand { set; get; }


    }

}
