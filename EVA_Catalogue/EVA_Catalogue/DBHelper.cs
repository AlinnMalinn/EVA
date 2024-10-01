using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;

namespace EVA_Catalogue
{
    class DBHelper
    {

        private string CnnStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + MainViewModel.SourceDirectoryDB + @"\";
        string connectionString = @"Data Source = (LocalDB)\MSSQLLocalDB; Integrated Security = True";

        public DataSet GetSeriesDataFromDB(string dbName)
        {
            string comandSelect = "select * from Серии order by SeriesName";
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(CnnStr + dbName + ".mdf; Integrated Security = True"))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = new SqlCommand(comandSelect, connection);
                dataAdapter.Fill(ds);
            }
            return ds;
        }
        public DataSet GetSeriesDataFromDBforSettings(string dbName, string seriesName)
        {
            string comandSelect = "select * from Серии WHERE SeriesName LIKE '" + seriesName + "'";
            DataSet ds = new DataSet();
            using (SqlConnection connection = new SqlConnection(CnnStr + dbName + ".mdf; Integrated Security = True"))
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
                dataAdapter.SelectCommand = new SqlCommand("select * from " + tableName + " WHERE SeriesID LIKE '" + SeriesID + "' AND RatedСurrent = '" + RatedCurrent.ToString().Replace(',', '.') + "' AND NumberOfPoles LIKE '" + NumberOfPoles + "' AND ResponseCharacteristics LIKE '" + ResponseCharacteristics + "' AND ThermalOverloadRelease LIKE '" + ThermalOverloadRelease + "' AND MaximumBreakingCapacity >= '" + MaximumBreakingCapacity.ToString().Replace(',', '.') + "' order by MaximumBreakingCapacity ", connection);
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
        public void AddDataBase(string selectedFile)
        {
            //проверка наличия БД
            int i = 0;
            string nameOfInsertedDB = Path.GetFileNameWithoutExtension(selectedFile).ToString();
            foreach (string file in Directory.EnumerateFiles(MainViewModel.SourceDirectoryDB, "*.mdf"))
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
                }

            }
                try
                {
                    //перебираем листы в экселе, для каждого создаем таблицу с данными
                    ExcelHelperForDB excel = new ExcelHelperForDB(selectedFile);
                    List<object[]> dataQFFromExcelPage = excel.GetListOfDevicesTypeFromDB();
                    // Создание базы данных
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
                                                     + MainViewModel.SourceDirectoryDB2
                                                     + "\\"
                                                     + nameOfInsertedDB
                                                     + ".mdf'"
                                                     + ") LOG ON ("
                                                     + "NAME = '"
                                                     + nameOfInsertedDB
                                                     + "_log', "
                                                     + "FILENAME = '"
                                                     + MainViewModel.SourceDirectoryDB2
                                                     + "\\"
                                                     + nameOfInsertedDB
                                                     + "_log.ldf'"
                                                     + ")";


                        SqlCommand command = new SqlCommand(createDatabaseQuery, connection);

                        command.ExecuteNonQuery();
                        MessageBox.Show("БЗ успешно создана");
                    }

                    string connectionForTable = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + MainViewModel.SourceDirectoryDB + @"\" + nameOfInsertedDB + ".mdf; Integrated Security = True";
                    string createTableQuery = @"
                         CREATE TABLE [Модульные автоматические выключатели] (
    [id]                      INT            IDENTITY (1, 1) NOT NULL,
    [SeriesID]                INT            NULL,
    [NameD]                   NVARCHAR (MAX) NULL,
    [NumberOfPoles]           INT            NULL,
    [RatedСurrent]            FLOAT (53)     NULL,
    [ResponseCharacteristics] NVARCHAR (50)  NULL,
    [MaximumBreakingCapacity] FLOAT (53)     NOT NULL,
    [ThermalOverloadRelease]  INT            NULL,
    [Code]                    NVARCHAR (50)  NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
)";
                    string insertQuery = @"INSERT INTO[Модульные автоматические выключатели](SeriesID, NameD, NumberOfPoles, RatedСurrent, Code, ResponseCharacteristics, MaximumBreakingCapacity, ThermalOverloadRelease)  VALUES (@SeriesID, @NameD, @NumberOfPoles, @RatedСurrent, @Code, @ResponseCharacteristics, @MaximumBreakingCapacity, @ThermalOverloadRelease);";

                    using (SqlConnection dataBaseConnection = new SqlConnection(connectionForTable))
                    {
                        dataBaseConnection.Open();

                        using (SqlCommand comm = new SqlCommand(createTableQuery, dataBaseConnection))
                        {
                            // Выполнение команды
                            comm.ExecuteNonQuery();
                            MessageBox.Show("Таблица успешно создана");
                        }
                        foreach (var dataQF in dataQFFromExcelPage)
                        {
                            using (SqlCommand command = new SqlCommand(insertQuery, dataBaseConnection))
                            {
                                // Параметры запроса
                                command.Parameters.AddWithValue("@SeriesID", int.Parse(dataQF[0].ToString()));
                                command.Parameters.AddWithValue("@NameD", dataQF[7].ToString());
                                command.Parameters.AddWithValue("@NumberOfPoles", int.Parse(dataQF[2].ToString()));
                                command.Parameters.AddWithValue("@RatedСurrent", float.Parse(dataQF[1].ToString()));
                                command.Parameters.AddWithValue("@Code", dataQF[9].ToString());
                                command.Parameters.AddWithValue("@ResponseCharacteristics", dataQF[3].ToString());
                                command.Parameters.AddWithValue("@MaximumBreakingCapacity", float.Parse(dataQF[4].ToString()));
                                command.Parameters.AddWithValue("@ThermalOverloadRelease", int.Parse(dataQF[5].ToString()));
                                // Выполняем команду
                                command.ExecuteNonQuery();
                            }
                        }
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

                            // Добавляем данные текущей таблицы как новый лист в Excel
                            excel.CreateSheet(dataTable, sheetName);
                        }
                    }

                    // Сохраняем файл Excel
                    excel.SaveAs(filePath);
                    MessageBox.Show("Все данные успешно сохранены в файл Excel: " + filePath);
                
                }


                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        public void ToFullFillDataBase(string selectedFile)
        {
            string nameOfInsertedDB = Path.GetFileNameWithoutExtension(selectedFile).ToString();

        }
    }
}
