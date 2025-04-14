
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
            Mouse.OverrideCursor = Cursors.Wait;
            PathHelper pathHelper = new PathHelper();
            string sourceDirectorySettings = pathHelper.PathSettingsHelper();
            List<string> producerListForSettingsQF = new List<string>();
            List<string> seriesListForSettingsQF = new List<string>();
            List<string> producerListForSettingsQFD = new List<string>();
            List<string> seriesListForSettingsQFD = new List<string>();
            if (File.Exists(sourceDirectorySettings))
            {
                if (IsAutomaticSelectionEnabledForModularCircuitBreakers == true)
                {
                    try
                    {
                        using (StreamReader reader = new StreamReader(sourceDirectorySettings))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (line.Split('%')[0] == ModularCircuitBreakersSettings)
                                {
                                    string lineWhithProducers = line.Split('%')[1];
                                    string lineWhithSries = line.Split('%')[2];

                                    foreach (string subLine in lineWhithProducers.Split('#'))
                                    {
                                        producerListForSettingsQF.Add(subLine);
                                    }
                                    foreach (string subLine in lineWhithSries.Split('#'))
                                    {
                                        //producerForSeriesListForSettings.Add(subLine.Split(':')[0]);
                                        seriesListForSettingsQF.Add(subLine);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
                    if (IsAutomaticSelectionEnabledForModularResidualCircuitBreakers == true)
                    {
                        try
                        {
                            using (StreamReader reader = new StreamReader(sourceDirectorySettings))
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (line.Split('%')[0] == ModularResidualCurrentCircuitBreakersSettings)
                                    {
                                        string lineWhithProducers = line.Split('%')[1];
                                        string lineWhithSries = line.Split('%')[2];

                                        foreach (string subLine in lineWhithProducers.Split('#'))
                                        {
                                            producerListForSettingsQFD.Add(subLine);
                                        }
                                        foreach (string subLine in lineWhithSries.Split('#'))
                                        {
                                            //producerForSeriesListForSettings.Add(subLine.Split(':')[0]);
                                            seriesListForSettingsQFD.Add(subLine);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.Message);
                        }
                    }
                    if (producerListForSettingsQF.All(s => string.IsNullOrEmpty(s)) & IsAutomaticSelectionEnabledForModularCircuitBreakers == true)
                    {
                        MessageBox.Show("Производители '" + TableNameModularCircuitBreakers + "' не выбраны");
                    Mouse.OverrideCursor = null;
                }
                    if (producerListForSettingsQFD.All(s => string.IsNullOrEmpty(s)) & IsAutomaticSelectionEnabledForModularResidualCircuitBreakers == true)
                    {
                        MessageBox.Show("Производители '" + TableNameModularResidualCurrentCircuitBreakers + "' не выбраны");
                    Mouse.OverrideCursor = null;
                }

                    if (((producerListForSettingsQF.All(s => !string.IsNullOrEmpty(s)) & producerListForSettingsQF.Count!=0) & IsAutomaticSelectionEnabledForModularCircuitBreakers == true) | ((producerListForSettingsQFD.All(s => !string.IsNullOrEmpty(s)) & producerListForSettingsQFD.Count != 0 )& IsAutomaticSelectionEnabledForModularResidualCircuitBreakers == true))
                    {
                    Mouse.OverrideCursor = Cursors.Wait;
                    EquipmentSelection es = new EquipmentSelection();
                        es.SelectDevicecs_ModularCircuitBreaker(producerListForSettingsQF, seriesListForSettingsQF, producerListForSettingsQFD, seriesListForSettingsQFD);
                        MessageBox.Show("Оборудование подобрано");
                        Mouse.OverrideCursor = null;
                }

                }
                



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
                currentTableFromDB = ModularCircuitBreakersSettings;
                SettingsHelper.Instance.SetTypeOfDevice(currentTableFromDB);
                WindowSettingsModularCircuitBreakers windowSettingsModularCircuitBreakers = new WindowSettingsModularCircuitBreakers();
                windowSettingsModularCircuitBreakers.ShowDialog();

            }

        }
    private void OpenWindowSettingsModularResidualCurrentBreakers()
        {
            //   WindowSettingsModularResidualCurrentBreakers windowSettingsModularResidualCurrentBreakers = new WindowSettingsModularResidualCurrentBreakers();
            //   windowSettingsModularResidualCurrentBreakers.ShowDialog();
            {
                currentTableFromDB = ModularResidualCurrentCircuitBreakersSettings;
                SettingsHelper.Instance.SetTypeOfDevice(currentTableFromDB);
                WindowSettingsModularCircuitBreakers windowSettingsModularCircuitBreakers = new WindowSettingsModularCircuitBreakers();
                windowSettingsModularCircuitBreakers.ShowDialog();
           

            }

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
