using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using DataTable = System.Data.DataTable;
using Excel = Microsoft.Office.Interop.Excel;

namespace EVA_Catalogue
{
    class DBHelper
    {
              
        //private string CnnStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + MainViewModel.SourceDirectoryDB + @"\";
        private string CnnStr;
        string connectionString = @"Data Source = (LocalDB)\MSSQLLocalDB; Integrated Security = True";
        PathHelper pathHelper;
        string sourceDirectoryDB;
        //string comandSelect;
        public DBHelper()
        {
            PathHelper pathHelper = new PathHelper();
            sourceDirectoryDB = pathHelper.PathDBHelper();
            CnnStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + sourceDirectoryDB + @"\";
        }

        public DataSet GetSeriesDataFromDB(string dbName, string tableName)
        {

            string comandSelect = "select DISTINCT SeriesName from "+ tableName;
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(CnnStr + dbName + ".mdf; Integrated Security = True"))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = new SqlCommand(comandSelect, connection);
                dataAdapter.Fill(ds);
            }
            return ds;
        }
        public DataSet GetSeriesDataFromDBforSettings(string dbName, string seriesName, string tableName)
        {
            string comandSelect = "select * from "+ tableName + " WHERE SeriesName LIKE '" + seriesName + "'";
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(CnnStr + dbName + ".mdf; Integrated Security = True"))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = new SqlCommand(comandSelect, connection);
                dataAdapter.Fill(ds);
            }
            return ds;
        }
        public DataSet GetDeviceDataFromDBbyDBNameSeriesName(string dbName, string tableName, string SeriesName, object RatedCurrent, object NumberOfPoles, object ResponseCharacteristics, object MaximumBreakingCapacity, object ThermalOverloadRelease)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection connection = new SqlConnection(CnnStr + dbName + ".mdf; Integrated Security = True"))
                {
                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    dataAdapter.SelectCommand = new SqlCommand("select * from " + tableName + " WHERE SeriesName LIKE '" + SeriesName + "' AND RatedСurrent = '" + RatedCurrent.ToString().Replace(',', '.') + "' AND NumberOfPoles LIKE '" + NumberOfPoles + "' AND ResponseCharacteristics LIKE '" + ResponseCharacteristics + "' AND ThermalOverloadRelease LIKE '" + ThermalOverloadRelease + "' AND MaximumBreakingCapacity >= '" + MaximumBreakingCapacity.ToString().Replace(',', '.') + "' order by MaximumBreakingCapacity ", connection);
                    dataAdapter.Fill(ds);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            return ds;
        }
        public DataSet GetDeviceDataFromDBbyDBName(string dbName, string tableName, object RatedCurrent, object NumberOfPoles, object ResponseCharacteristics, object MaximumBreakingCapacity, object ThermalOverloadRelease)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(CnnStr + dbName + ".mdf; Integrated Security = True"))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = new SqlCommand("select * from " + tableName + " WHERE RatedСurrent = '" + RatedCurrent.ToString().Replace(',', '.') + "' AND NumberOfPoles LIKE '" + NumberOfPoles + "' AND ResponseCharacteristics LIKE '" + ResponseCharacteristics + "' AND ThermalOverloadRelease LIKE '" + ThermalOverloadRelease + "' AND MaximumBreakingCapacity >= '" + MaximumBreakingCapacity.ToString().Replace(',', '.') + "' order by MaximumBreakingCapacity ", connection);
                dataAdapter.Fill(ds);
            }
            return ds;
        }
        public DataSet GetDeviceQFDDataFromDBbyDBNameSeriesName(string dbName, string tableName, string SeriesName, object RatedCurrent, object NumberOfPoles, object ResponseCharacteristics, object MaximumBreakingCapacity, object ThermalOverloadRelease, object leakageСurrent)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(CnnStr + dbName + ".mdf; Integrated Security = True"))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = new SqlCommand("select * from " + tableName + " WHERE SeriesName LIKE '" + SeriesName + "' AND RatedСurrent = '" + RatedCurrent.ToString().Replace(',', '.') + "' AND NumberOfPoles LIKE '" + NumberOfPoles + "' AND ResponseCharacteristics LIKE '" + ResponseCharacteristics + "' AND ThermalOverloadRelease LIKE '" + ThermalOverloadRelease + "' AND MaximumBreakingCapacity >= '" + MaximumBreakingCapacity.ToString().Replace(',', '.') + "' AND LeakageСurrent = '" + leakageСurrent.ToString().Replace(',', '.') + "' order by MaximumBreakingCapacity ", connection);
                dataAdapter.Fill(ds);
            }
            return ds;
        }
        public DataSet GetDeviceQFDDataFromDBbyDBName(string dbName, string tableName, object RatedCurrent, object NumberOfPoles, object ResponseCharacteristics, object MaximumBreakingCapacity, object ThermalOverloadRelease, object leakageСurrent)
        {
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(CnnStr + dbName + ".mdf; Integrated Security = True"))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = new SqlCommand("select * from " + tableName + " WHERE RatedСurrent = '"  + RatedCurrent.ToString().Replace(',', '.') + "' AND NumberOfPoles LIKE '" + NumberOfPoles + "' AND ResponseCharacteristics LIKE '" + ResponseCharacteristics + "' AND ThermalOverloadRelease LIKE '" + ThermalOverloadRelease + "' AND MaximumBreakingCapacity >= '" + MaximumBreakingCapacity.ToString().Replace(',', '.') + "' AND LeakageСurrent = '" + leakageСurrent.ToString().Replace(',', '.') + "' order by MaximumBreakingCapacity ", connection);
                dataAdapter.Fill(ds);
            }
            return ds;
        }
        public void AddDataBase(string selectedFile)
        {
            //проверка наличия БД
            int i = 0;
            string nameOfInsertedDB = Path.GetFileNameWithoutExtension(selectedFile).ToString();
            foreach (string file in Directory.EnumerateFiles(sourceDirectoryDB, "*.mdf"))
            {

                if (nameOfInsertedDB == Path.GetFileNameWithoutExtension(file).ToString())
                {
                    i++;
                    break;
                }
            }
            if (i != 0)
            {
                // SQL-запрос для удаления базы данных
                string queryDropDB = $"DROP DATABASE {nameOfInsertedDB}";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
  
                    // Открываем подключение
                    connection.Open();
                    // Выполняем команду на удаление базы данных
                    using (SqlCommand command = new SqlCommand(queryDropDB, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                    connection.Dispose();
                }


            }
                try
                {
                    //перебираем листы в экселе, для каждого создаем таблицу с данными
                    ExcelHelperForDB excel = new ExcelHelperForDB(selectedFile);
                    (List<object[]>, List<object[]>)dataQFFromExcelPage = excel.GetListOfDevicesTypeFromDB();
                // Создание базы данных
                // Ниже раньше испльзовалась MainViewModel.SourceDirectoryDB2!!!
                using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string createDatabaseQuery = "CREATE DATABASE "
                                                     + nameOfInsertedDB
                                                     + " ON PRIMARY ("
                                                     + "NAME = '"
                                                     + nameOfInsertedDB
                                                     + "', "
                                                     + "FILENAME = '"
                                                     + sourceDirectoryDB
                                                     + "\\"
                                                     + nameOfInsertedDB
                                                     + ".mdf'"
                                                     + ") LOG ON ("
                                                     + "NAME = '"
                                                     + nameOfInsertedDB
                                                     + "_log', "
                                                     + "FILENAME = '"
                                                     + sourceDirectoryDB
                                                     + "\\"
                                                     + nameOfInsertedDB
                                                     + "_log.ldf'"
                                                     + ")";


                        SqlCommand command = new SqlCommand(createDatabaseQuery, connection);

                        command.ExecuteNonQuery();
                        MessageBox.Show("БЗ успешно создана");
                    connection.Close();
                    connection.Dispose();
                }
                // Ниже раньше испльзовалась MainViewModel.SourceDirectoryDB!!!
                string connectionForTable = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + sourceDirectoryDB + @"\" + nameOfInsertedDB + ".mdf; Integrated Security = True";
                    string createTableQueryQF = @"
                         CREATE TABLE [Модульные автоматические выключатели] (
    [id]                      INT            IDENTITY (1, 1) NOT NULL,
    [SeriesName]              NVARCHAR (MAX) NULL,
    [NameD]                   NVARCHAR (MAX) NULL,
    [NumberOfPoles]           INT            NULL,
    [RatedСurrent]            FLOAT (53)     NULL,
    [ResponseCharacteristics] NVARCHAR (50)  NULL,
    [MaximumBreakingCapacity] FLOAT (53)     NULL,
    [ThermalOverloadRelease]  INT            NULL,
    [Code]                    NVARCHAR (50)  NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
)";
                string createTableQueryQFD = @"
                         CREATE TABLE [Модульные автоматические выключатели дифференциального тока] (
    [id]                      INT            IDENTITY (1, 1) NOT NULL,
    [SeriesName]              NVARCHAR (MAX) NULL,
    [NameD]                   NVARCHAR (MAX) NULL,
    [NumberOfPoles]           INT            NULL,
    [RatedСurrent]            FLOAT (53)     NULL,
    [LeakageСurrent]          decimal (3,2)     NULL,
    [ResponseCharacteristics] NVARCHAR (50)  NULL,
    [MaximumBreakingCapacity] FLOAT (53)     NULL,
    [ThermalOverloadRelease]  INT            NULL,
    [Code]                    NVARCHAR (50)  NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
)";
                string insertQueryQF = @"INSERT INTO[Модульные автоматические выключатели](SeriesName, NameD, NumberOfPoles, RatedСurrent, Code, ResponseCharacteristics, MaximumBreakingCapacity, ThermalOverloadRelease)  VALUES (@SeriesName, @NameD, @NumberOfPoles, @RatedСurrent, @Code, @ResponseCharacteristics, @MaximumBreakingCapacity, @ThermalOverloadRelease);";
                string insertQueryQFD = @"INSERT INTO[Модульные автоматические выключатели дифференциального тока](SeriesName, NameD, NumberOfPoles, RatedСurrent, LeakageСurrent, Code, ResponseCharacteristics, MaximumBreakingCapacity, ThermalOverloadRelease)  VALUES (@SeriesName, @NameD, @NumberOfPoles, @RatedСurrent, @LeakageСurrent, @Code, @ResponseCharacteristics, @MaximumBreakingCapacity, @ThermalOverloadRelease);";
                using (SqlConnection dataBaseConnection = new SqlConnection(connectionForTable))
                    {
                        dataBaseConnection.Open();


                    if (dataQFFromExcelPage.Item1 != null)
                    {
                        using (SqlCommand comm = new SqlCommand(createTableQueryQF, dataBaseConnection))
                        {
                            // Выполнение команды
                            comm.ExecuteNonQuery();
                            MessageBox.Show("Таблица успешно создана");
                        }
                        foreach (var dataQF in dataQFFromExcelPage.Item1)
                        {
                            using (SqlCommand command = new SqlCommand(insertQueryQF, dataBaseConnection))
                            {
                                // Параметры запроса
                                command.Parameters.AddWithValue("@SeriesName", dataQF[0].ToString());
                                command.Parameters.AddWithValue("@NameD", dataQF[1].ToString());
                                command.Parameters.AddWithValue("@NumberOfPoles", int.Parse(dataQF[2].ToString()));
                                command.Parameters.AddWithValue("@RatedСurrent", float.Parse(dataQF[3].ToString()));
                                command.Parameters.AddWithValue("@Code", dataQF[7].ToString());
                                command.Parameters.AddWithValue("@ResponseCharacteristics", dataQF[4].ToString());
                                command.Parameters.AddWithValue("@MaximumBreakingCapacity", float.Parse(dataQF[5].ToString()));
                                command.Parameters.AddWithValue("@ThermalOverloadRelease", int.Parse(dataQF[6].ToString()));
                                // Выполняем команду
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    if (dataQFFromExcelPage.Item2 != null)
                    {
                        using (SqlCommand comm = new SqlCommand(createTableQueryQFD, dataBaseConnection))
                        {
                            // Выполнение команды
                            comm.ExecuteNonQuery();
                            MessageBox.Show("Таблица успешно создана");
                        }
                        foreach (var dataQF in dataQFFromExcelPage.Item2)
                        {
                            using (SqlCommand command = new SqlCommand(insertQueryQFD, dataBaseConnection))
                            {
                                // Параметры запроса
                                command.Parameters.AddWithValue("@SeriesName", dataQF[0].ToString());
                                command.Parameters.AddWithValue("@NameD", dataQF[1].ToString());
                                command.Parameters.AddWithValue("@NumberOfPoles", int.Parse(dataQF[2].ToString()));
                                command.Parameters.AddWithValue("@RatedСurrent", float.Parse(dataQF[3].ToString()));
                                command.Parameters.AddWithValue("@LeakageСurrent", decimal.Parse(dataQF[4].ToString()));
                                command.Parameters.AddWithValue("@Code", dataQF[8].ToString());
                                command.Parameters.AddWithValue("@ResponseCharacteristics", dataQF[5].ToString());
                                command.Parameters.AddWithValue("@MaximumBreakingCapacity", float.Parse(dataQF[6].ToString()));
                                command.Parameters.AddWithValue("@ThermalOverloadRelease", int.Parse(dataQF[7].ToString()));
                                // Выполняем команду
                                command.ExecuteNonQuery();
                            }
                        }

                    }
                    dataBaseConnection.Close();
                    dataBaseConnection.Dispose();

                }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("SQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }


        }



        public void UploadDB(string selectedDB, string filePath)
        {
            {
                try
                {
                    // Создаем новый Excel-документ
                    ExcelUploadDB excel = new ExcelUploadDB();
                    // Подключаемся к базе данных
                    using (SqlConnection connection = new SqlConnection(CnnStr + selectedDB + ".mdf; Integrated Security = True"))
                    {
                        connection.Open();

                        // Получаем список всех таблиц в базе данных
                        DataTable tables = connection.GetSchema("Tables");

                        foreach (DataRow row in tables.Rows)
                        {
                            // Извлекаем имя таблицы
                            string tableName = row["TABLE_NAME"].ToString();
                            string sheetName = "";
                            if (tableName == "Модульные автоматические выключатели")
                            {
                                sheetName = "QF";
                            }
                            else if (tableName == "Модульные автоматические выключатели дифференциального тока")
                            {
                                sheetName = "QFD";
                            }

                            // SQL-запрос для выборки всех данных из текущей таблицы
                            string query = $"SELECT * FROM [{row["TABLE_NAME"]}]";


                            // Создаем DataTable для хранения данных из текущей таблицы
                            DataTable dataTable = new DataTable();
                           
                            // Выполняем запрос и заполняем DataTable данными
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                SqlDataAdapter adapter = new SqlDataAdapter(command);
                               
                                adapter.Fill(dataTable);
                            }
                            dataTable.Columns.Remove("id");
                            // Добавляем данные текущей таблицы как новый лист в Excel
                            excel.CreateSheet(dataTable, sheetName);
                        }
                    }

                    // Сохраняем файл Excel
                    excel.SaveAs(filePath);
                    MessageBox.Show("Все данные успешно сохранены в файл Excel: " + filePath);
                    // Освобождаем ресурсы COM-объектов
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(excelWS);
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(excelWB);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                }


                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        public void DeleteDataBase(string selectedFile)
        {
            string querySetSingleUser = $"ALTER DATABASE {selectedFile} SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
          

            string queryDropDB = $"DROP DATABASE {selectedFile}";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Открываем подключение
                connection.Open();
                using (SqlCommand command = new SqlCommand(querySetSingleUser, connection))
                {
                    command.ExecuteNonQuery();  // Закрываем все соединения
                }
                // Выполняем команду на удаление базы данных
                using (SqlCommand command = new SqlCommand(queryDropDB, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            PathHelper pathHelper = new PathHelper();
            string sourceDirectorySettings = pathHelper.PathSettingsHelper();
            if (File.Exists(sourceDirectorySettings))
            {
                // Очистка содержимого файла
                File.WriteAllText(sourceDirectorySettings, string.Empty);
             
            }

        }

        public void ToFullFillDataBase(string selectedFile)
        {
            string nameOfInsertedDB = Path.GetFileNameWithoutExtension(selectedFile).ToString();

        }
    }
}
