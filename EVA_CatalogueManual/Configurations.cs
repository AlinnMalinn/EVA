using System.Collections.Generic;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;
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
