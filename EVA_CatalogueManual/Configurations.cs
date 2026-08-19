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


namespace EVA_CatalogueManual
{
    class Configurations
    {
        List<string>[] commonListForSettings = new List<string>[2];
        public void CreateConfiguration(List<ProducerModel> newProducerList, List<SeriesModel> newSeriesList, string chosenTypeOfDevice)
        {
            PathHelper pathHelper = new PathHelper();
            string sourceDirectorySettings = pathHelper.PathSettingsHelper();
            List<string> producerListForSettings = new List<string>();
            List<string> seriesListForSettings = new List<string>();
            List<string> seriesListWhithProducersForSettings = new List<string>();
            

            SettingsHelper.Instance.SetTypeOfDevice(chosenTypeOfDevice);

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
                foreach (SeriesModel newSeries in newSeriesList)
                {
                    seriesListWhithProducersForSettings.Add(newSeries.series.ToString());
                }
            }
            else
            {
                seriesListForSettings.Add("%");
            }

            string producerStringForSettings = string.Join("#", producerListForSettings);
            string seriesStringForSettings = string.Join("#", seriesListWhithProducersForSettings);
            string newEntry = string.Join("%", SettingsHelper.Instance.TypeOfDevice, producerStringForSettings, seriesStringForSettings);

            // Читаем существующий файл
            List<string> lines = new List<string>();
            if (File.Exists(sourceDirectorySettings))
            {
                lines = File.ReadAllLines(sourceDirectorySettings).ToList();
            }

            bool updated = false;
            for (int i = 0; i < lines.Count; i++)
            {
                 
                if (lines[i].StartsWith(SettingsHelper.Instance.TypeOfDevice + "%"))
                {
                    lines[i] = newEntry;
                    updated = true;
                    break;
                }
            }

            // Если строка не была найдена, добавляем новую запись
            if (!updated)
            {
                lines.Add(newEntry);
            }

            Excel.Workbook workbook = AppManager.ExcelApp.ActiveWorkbook;
            SettingsProfileService.Instance.SaveSelection(
                SettingsProfileService.ManualMode,
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
        public void ResetConfiguration(string chosenTypeOfDevice)
        {
            PathHelper pathHelper = new PathHelper();
            string sourceDirectorySettings = pathHelper.PathSettingsHelper();
            SettingsHelper.Instance.SetTypeOfDevice(chosenTypeOfDevice);

            // Читаем существующий файл
            List<string> lines = new List<string>();
            if (File.Exists(sourceDirectorySettings))
            {
                lines = File.ReadAllLines(sourceDirectorySettings).ToList();
            }

            for (int i = 0; i < lines.Count; i++)
            {

                if (lines[i].StartsWith(SettingsHelper.Instance.TypeOfDevice + "%"))
                {
                    lines[i] = "";
                    break;
                }
            }
            
            Excel.Workbook workbook = AppManager.ExcelApp.ActiveWorkbook;
            SettingsProfileService.Instance.ResetSelection(
                SettingsProfileService.ManualMode,
                SettingsHelper.Instance.TypeOfDevice,
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
        public List<string>[] LoadConfiguration(string chosenTypeOfDevice)

        {
            SettingsHelper.Instance.SetTypeOfDevice(chosenTypeOfDevice);


            List<string> producerListForSettings = new List<string>();
            List<string> seriesListForSettings = new List<string>();
            PathHelper pathHelper = new PathHelper();
            string sourceDirectorySettings = pathHelper.PathSettingsHelper();
            if (sourceDirectorySettings == null)
            {
                return null;
            }
            Excel.Workbook workbook = AppManager.ExcelApp.ActiveWorkbook;
            DeviceSelectionSettings selection = SettingsProfileService.Instance.GetSelection(
                SettingsProfileService.ManualMode,
                SettingsHelper.Instance.TypeOfDevice,
                workbook.FullName,
                workbook.Name,
                sourceDirectorySettings);
            commonListForSettings[0] = selection.Producers;
            commonListForSettings[1] = selection.Series;
            return commonListForSettings;
        }

    }
}
