using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.Common;
using EVA_Catalogue;

namespace EVA_CatalogueManual
{
    class DBHelper
    {
        public DBHelper()
        {
            PathHelper pathHelper = new PathHelper();
            string sourceDirectoryDB = pathHelper.PathDBHelper();
            CatalogueCacheService.Instance.ConfigureDirectory(sourceDirectoryDB);
        }
        public DataSet GetSeriesDataFromDB(string dbName, string tableName)
        {

            return CatalogueCacheService.Instance.GetSeries(
                dbName, tableName == MainViewModel.TableNameModularResidualCurrentCircuitBreakers);
        }
        public DataSet GetDeviceDataFromDBbyDBNameSeriesName(string dbName, string tableName, string SeriesName, object RatedCurrent, object NumberOfPoles, object ResponseCharacteristics, object MaximumBreakingCapacity, object ThermalOverloadRelease)
        {
            return CatalogueCacheService.Instance.FindDevices(dbName, false, SeriesName,
                RatedCurrent, NumberOfPoles, ResponseCharacteristics, MaximumBreakingCapacity,
                ThermalOverloadRelease);
        }
        public DataSet GetDeviceDataFromDBbyDBName(string dbName, string tableName, object RatedCurrent, object NumberOfPoles, object ResponseCharacteristics, object MaximumBreakingCapacity, object ThermalOverloadRelease)
        {
            return CatalogueCacheService.Instance.FindDevices(dbName, false, null,
                RatedCurrent, NumberOfPoles, ResponseCharacteristics, MaximumBreakingCapacity,
                ThermalOverloadRelease);
        }
        public DataSet GetDeviceQFDDataFromDBbyDBNameSeriesName(string dbName, string tableName, string SeriesName, object RatedCurrent, object NumberOfPoles, object ResponseCharacteristics, object MaximumBreakingCapacity, object ThermalOverloadRelease, object leakageСurrent, object residualCurrentType)
        {
            return CatalogueCacheService.Instance.FindDevices(dbName, true, SeriesName,
                RatedCurrent, NumberOfPoles, ResponseCharacteristics, MaximumBreakingCapacity,
                ThermalOverloadRelease, leakageСurrent, residualCurrentType);
        }
        public DataSet GetDeviceQFDDataFromDBbyDBName(string dbName, string tableName, object RatedCurrent, object NumberOfPoles, object ResponseCharacteristics, object MaximumBreakingCapacity, object ThermalOverloadRelease, object leakageСurrent, object residualCurrentType)
        {
            return CatalogueCacheService.Instance.FindDevices(dbName, true, null,
                RatedCurrent, NumberOfPoles, ResponseCharacteristics, MaximumBreakingCapacity,
                ThermalOverloadRelease, leakageСurrent, residualCurrentType);
        }
        public bool TableExists(string dbName, string tableName)
        {
            string requiredSheet = tableName == MainViewModel.TableModularResidualCurrentCircuitBreakers
                ? "QFD"
                : "QF";
            return CatalogueCacheService.Instance.GetProducerNames(requiredSheet)
                .Contains(dbName, StringComparer.OrdinalIgnoreCase);
        }

    }
}
