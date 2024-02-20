using System.Data;
using System.Data.SqlClient;

namespace EVA_Catalogue
{
    class DBHelper
    {
        private string CnnStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\79126\source\EVA\EVA_Catalogue\EVA_Catalogue\";
        public DataSet GetSeriesDataFromDB(string dbName)
        {            
            string comandSelect = "select * from Серии order by SeriesName";
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(CnnStr+ dbName + ".mdf; Integrated Security = True"))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = new SqlCommand(comandSelect, connection);
                dataAdapter.Fill(ds);
            }
            return ds;
        }
        public DataSet GetDeviceDataFromDB1(string dbName, string tableName, int SeriesID, object RatedCurrent, object NumberOfPoles, object ResponseCharacteristics, object MaximumBreakingCapacity, object ThermalOverloadRelease)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(CnnStr + dbName + ".mdf; Integrated Security = True"))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter();                             
                dataAdapter.SelectCommand = new SqlCommand("select * from " + tableName + " WHERE SeriesID LIKE '" + SeriesID + "' AND RatedСurrent = '" + RatedCurrent.ToString().Replace(',', '.') + "' AND NumberOfPoles LIKE '" + NumberOfPoles + "' AND ResponseCharacteristics LIKE '" + ResponseCharacteristics + "' AND ThermalOverloadRelease LIKE '" + ThermalOverloadRelease  + "' AND MaximumBreakingCapacity >= '" + MaximumBreakingCapacity.ToString().Replace(',', '.') + "' order by MaximumBreakingCapacity " , connection);
                dataAdapter.Fill(ds);
            }
            return ds;
        }
        public DataSet GetDeviceDataFromDB2(string dbName, string tableName, int SeriesID, object RatedCurrent, object NumberOfPoles, object ResponseCharacteristics, object MaximumBreakingCapacity, object ThermalOverloadRelease, object leakageСurrent)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(CnnStr + dbName + ".mdf; Integrated Security = True"))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = new SqlCommand("select * from " + tableName + " WHERE SeriesID LIKE '" + SeriesID + "' AND RatedСurrent = '" + RatedCurrent.ToString().Replace(',', '.') + "' AND NumberOfPoles LIKE '" + NumberOfPoles + "' AND ResponseCharacteristics LIKE '" + ResponseCharacteristics + "' AND ThermalOverloadRelease LIKE '" + ThermalOverloadRelease + "' AND MaximumBreakingCapacity >= '" + MaximumBreakingCapacity.ToString().Replace(',', '.') + "' AND LeakageСurrent = '" + leakageСurrent.ToString().Replace(',', '.') + "' order by MaximumBreakingCapacity ", connection);
                dataAdapter.Fill(ds);
            }
            return ds;
        }
    }
}
