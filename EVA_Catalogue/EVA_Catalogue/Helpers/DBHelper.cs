using System.Data;

namespace EVA_Catalogue
{
    public class DBHelper
    {
        public DBHelper()
        {
            CatalogueCacheService.Instance.ConfigureDirectory(new PathHelper().PathDBHelper());
        }

        public DataSet GetSeriesDataFromDB(string dbName, string tableName)
        {
            return CatalogueCacheService.Instance.GetSeries(
                dbName, tableName == MainViewModel.TableNameModularResidualCurrentCircuitBreakers);
        }

        public DataSet GetDeviceDataFromDBbyDBNameSeriesName(string dbName, string tableName,
            string seriesName, object ratedCurrent, object numberOfPoles,
            object responseCharacteristics, object maximumBreakingCapacity,
            object thermalOverloadRelease)
        {
            return CatalogueCacheService.Instance.FindDevices(dbName, false, seriesName,
                ratedCurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity,
                thermalOverloadRelease);
        }

        public DataSet GetDeviceDataFromDBbyDBName(string dbName, string tableName,
            object ratedCurrent, object numberOfPoles, object responseCharacteristics,
            object maximumBreakingCapacity, object thermalOverloadRelease)
        {
            return CatalogueCacheService.Instance.FindDevices(dbName, false, null,
                ratedCurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity,
                thermalOverloadRelease);
        }

        public DataSet GetDeviceQFDDataFromDBbyDBNameSeriesName(string dbName, string tableName,
            string seriesName, object ratedCurrent, object numberOfPoles,
            object responseCharacteristics, object maximumBreakingCapacity,
            object thermalOverloadRelease, object leakageCurrent, object residualCurrentType)
        {
            return CatalogueCacheService.Instance.FindDevices(dbName, true, seriesName,
                ratedCurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity,
                thermalOverloadRelease, leakageCurrent, residualCurrentType);
        }

        public DataSet GetDeviceQFDDataFromDBbyDBName(string dbName, string tableName,
            object ratedCurrent, object numberOfPoles, object responseCharacteristics,
            object maximumBreakingCapacity, object thermalOverloadRelease,
            object leakageCurrent, object residualCurrentType)
        {
            return CatalogueCacheService.Instance.FindDevices(dbName, true, null,
                ratedCurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity,
                thermalOverloadRelease, leakageCurrent, residualCurrentType);
        }
    }
}
