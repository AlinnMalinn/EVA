using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using DataTable = System.Data.DataTable;
using Excel = Microsoft.Office.Interop.Excel;
using ClosedXML.Excel;


namespace EVA_Catalogue
{
    public class DBHelper
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
            CatalogueCacheService.Instance.ConfigureDirectory(sourceDirectoryDB);
        }


        public DataSet GetSeriesDataFromDB(string dbName, string tableName)
        {
            return CatalogueCacheService.Instance.GetSeries(
                dbName, tableName == MainViewModel.TableNameModularResidualCurrentCircuitBreakers);
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
        public void AddDataBase(string selectedFile)
        {
            string nameOfInsertedDB = Path.GetFileNameWithoutExtension(selectedFile).ToString();
            if (IsDataBaseExistInTheFolder(nameOfInsertedDB) == true)
            {
                DeleteDataBase(nameOfInsertedDB);
                CreateEmptyDataBase(nameOfInsertedDB);
                FillDataBaseWithDataFromExcel(selectedFile, nameOfInsertedDB);
                MessageBox.Show(
               $"База данных '{nameOfInsertedDB}' обновлена.",
               "Информация о базе данных",
               MessageBoxButton.OK,
               MessageBoxImage.Information);
            }
            else if (IsDataBaseExist(nameOfInsertedDB) == false)
            {
                CreateEmptyDataBase(nameOfInsertedDB);
                FillDataBaseWithDataFromExcel(selectedFile, nameOfInsertedDB);
                MessageBox.Show(
              $"База данных '{nameOfInsertedDB}' добавлена.",
              "Информация о базе данных",
              MessageBoxButton.OK,
              MessageBoxImage.Information);
            }
        }
        public void CreateEmptyDataBase(string nameOfInsertedDB)
        {
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
                // MessageBox.Show("БЗ успешно создана");
                connection.Close();
                connection.Dispose();
            }
        }
        public void FillDataBaseWithDataFromExcel (string selectedFile, string nameOfInsertedDB)
        {
            //перебираем листы в экселе, для каждого создаем таблицу с данными
            ExcelHelperForDB excel = new ExcelHelperForDB(selectedFile);
            (List<object[]>, List<object[]>) dataQFFromExcelPage = excel.GetListOfDevicesTypeFromDB();
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
    [Mark]                    NVARCHAR (50)  NULL,
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
    [Mark]                    NVARCHAR (50)  NULL,
    [ResidualCurrentType]     NVARCHAR (50)  NULL,


    PRIMARY KEY CLUSTERED ([id] ASC)
)";
            string insertQueryQF = @"INSERT INTO[Модульные автоматические выключатели](SeriesName, NameD, NumberOfPoles, RatedСurrent, Code, Mark, ResponseCharacteristics, MaximumBreakingCapacity, ThermalOverloadRelease)  VALUES (@SeriesName, @NameD, @NumberOfPoles, @RatedСurrent, @Code, @Mark, @ResponseCharacteristics, @MaximumBreakingCapacity, @ThermalOverloadRelease);";
            string insertQueryQFD = @"INSERT INTO[Модульные автоматические выключатели дифференциального тока](SeriesName, NameD, NumberOfPoles, RatedСurrent, LeakageСurrent, Code, Mark,  ResidualCurrentType, ResponseCharacteristics, MaximumBreakingCapacity, ThermalOverloadRelease)  VALUES (@SeriesName, @NameD, @NumberOfPoles, @RatedСurrent, @LeakageСurrent, @Code, @Mark, @ResidualCurrentType, @ResponseCharacteristics, @MaximumBreakingCapacity, @ThermalOverloadRelease);";
            using (SqlConnection dataBaseConnection = new SqlConnection(connectionForTable))
            {
                dataBaseConnection.Open();


                if (dataQFFromExcelPage.Item1 != null)
                {
                    using (SqlCommand comm = new SqlCommand(createTableQueryQF, dataBaseConnection))
                    {
                        // Выполнение команды
                        comm.ExecuteNonQuery();
                        // MessageBox.Show("Таблица успешно создана");
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
                            command.Parameters.AddWithValue("@Mark", dataQF[8].ToString());
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
                        //MessageBox.Show("Таблица успешно создана");
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
                            command.Parameters.AddWithValue("@Mark", dataQF[9].ToString());
                            command.Parameters.AddWithValue("@ResidualCurrentType", dataQF[10].ToString());
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
        public bool IsDataBaseExist(string nameOfInsertedDB)//проверка наличия БД и вывод сообщения с указанием пути сохранения
        {
            //проверка наличия БД
            
            string query = @"
        SELECT 
            mf.physical_name AS FilePath,
            mf.type_desc AS FileType
        FROM sys.databases d
        JOIN sys.master_files mf ON d.database_id = mf.database_id
        WHERE d.name = @nameOfInsertedDB";
            string folderPath = null;
            var sb = new StringBuilder();
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@nameOfInsertedDB", nameOfInsertedDB); 


                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        return false;
                    }

                    sb.AppendLine($"База данных '{nameOfInsertedDB}' существует:\n");


                    while (reader.Read())
                    {
                        string type = reader["FileType"].ToString();

                        // берём только основной файл (.mdf)
                        if (type == "ROWS")
                        {
                            string fullPath = reader["FilePath"].ToString();
                            folderPath = Path.GetDirectoryName(fullPath);
                            break;
                        }
                    }
                }
            }

            MessageBox.Show(
                $"База данных '{nameOfInsertedDB}' существует и находится по следующему пути:\n{folderPath}\n\nУдалите существующую базу данных или назовите загружаемый файл excel другим именем",
                "Информация о базе данных",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return true;
        }
        public bool IsDataBaseExistInTheFolder (string nameOfInsertedDB)//проверка наличия БД и вывод сообщения с указанием пути сохранения
        {
            //проверка наличия БД
            foreach (string file in Directory.EnumerateFiles(sourceDirectoryDB, "*.mdf"))
            {

                if (nameOfInsertedDB == Path.GetFileNameWithoutExtension(file).ToString())
                {
                    return true;
                    
                }
            }

            return false;
        }

        public void UploadDB(string selectedDB, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                using (var connection = new SqlConnection(
                           CnnStr + selectedDB + ".mdf;Integrated Security=True"))
                {
                    connection.Open();

                    DataTable tables = connection.GetSchema("Tables");

                    foreach (DataRow row in tables.Rows)
                    {
                        string tableName = row["TABLE_NAME"]?.ToString();
                        string sheetName;
                        string[] columnNames;

                        switch (tableName)
                        {
                            case "Модульные автоматические выключатели":
                                sheetName = "QF";

                                columnNames = new[]
                                {
                            "Серия",
                            "Наименование",
                            "Количество полюсов",
                            "Номинальный ток, А",
                            "Характеристика срабатывания",
                            "Отключающая способность, кА",
                            "Наличие теплового расцепителя (1 - есть, 0 - нет)",
                            "Код оборудования",
                            "Марка оборудования"
                        };
                                break;

                            case "Модульные автоматические выключатели дифференциального тока":
                                sheetName = "QFD";

                                columnNames = new[]
                                {
                            "Серия",
                            "Наименование",
                            "Количество полюсов",
                            "Номинальный ток, А",
                            "Номинальный ток утечки, А",
                            "Характеристика срабатывания",
                            "Отключающая способность, кА",
                            "Наличие теплового расцепителя (1 - есть, 0 - нет)",
                            "Код оборудования",
                            "Марка оборудования",
                            "Тип тока утечки"
                        };
                                break;

                            default:
                                continue;
                        }

                        string query = $"SELECT * FROM [{tableName}]";

                        DataTable dataTable = new DataTable();

                        using (var command = new SqlCommand(query, connection))
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }

                        // Удаляем столбец id, если он есть
                        if (dataTable.Columns.Contains("id"))
                        {
                            dataTable.Columns.Remove("id");
                        }

                        // Проверяем соответствие количества столбцов
                        if (dataTable.Columns.Count != columnNames.Length)
                        {
                            throw new InvalidOperationException(
                                $"Для листа {sheetName} количество столбцов в базе " +
                                $"({dataTable.Columns.Count}) не совпадает с количеством заданных " +
                                $"заголовков ({columnNames.Length}).");
                        }

                        // Переименовываем столбцы по порядку
                        for (int i = 0; i < columnNames.Length; i++)
                        {
                            dataTable.Columns[i].ColumnName = columnNames[i];
                        }

                        var worksheet = workbook.Worksheets.Add(sheetName);

                        worksheet.Cell(1, 1).InsertTable(dataTable);

                        worksheet.Columns().AdjustToContents();
                    }

                    workbook.SaveAs(filePath);
                    Excel.Application excelApp = new Excel.Application();

                    excelApp.Visible = true;

                    excelApp.Workbooks.Open(filePath);

                }

                MessageBox.Show(
                    "Все данные успешно сохранены в файл Excel:\n" + filePath,
                    "Результат",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при экспорте в Excel:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void DeleteDataBase(string selectedFile)
        {
            string querySetSingleUser = $"ALTER DATABASE {selectedFile} SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
          

            string queryDropDB = $"DROP DATABASE {selectedFile}";
            using (SqlConnection connection = new SqlConnection(connectionString))
            //using (SqlConnection connection = new SqlConnection(CnnStr + selectedFile + ".mdf; Integrated Security = True")) 
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

        public void ReattachAllDatabases() // ничего не получилось, наверное можно удалить
        {
            PathHelper pathHelper = new PathHelper();
            sourceDirectoryDB = pathHelper.PathDBHelper();

            string masterConnection = @"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(masterConnection))
            {
                connection.Open();

                string sql = @"
            SELECT DISTINCT d.name, mf.physical_name
            FROM sys.databases d
            INNER JOIN sys.master_files mf ON d.database_id = mf.database_id
            WHERE mf.type_desc = 'ROWS' AND d.database_id > 4
        ";

                using (SqlCommand cmd = new SqlCommand(sql, connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    var databases = new System.Collections.Generic.List<(string name, string path)>();

                    while (reader.Read())
                    {
                        string dbName = reader.GetString(0);
                        string physicalPath = reader.GetString(1);

                        databases.Add((dbName, physicalPath));
                    }

                    reader.Close();

                    foreach (var (dbName, physicalPath) in databases)
                    {
                        string expectedMdf = Path.Combine(sourceDirectoryDB, dbName + ".mdf");
                        string expectedLdf = Path.Combine(sourceDirectoryDB, dbName + "_log.ldf");

                        if (!File.Exists(expectedMdf))
                            continue; // Файл не найден, пропускаем

                        if (physicalPath.StartsWith(sourceDirectoryDB, StringComparison.OrdinalIgnoreCase))
                            continue;

                        // Отключаем все подключения
                        KillAllConnections(connection, dbName);

                        // Удаляем базу
                        DropDatabaseIfExists(connection, dbName);

                        // Небольшая пауза, чтобы сервер освободил ресурсы
                        Thread.Sleep(500);

                        // Подключаем заново по новому пути
                        using (var attachCmd = new SqlCommand($@"
                    CREATE DATABASE [{dbName}]
                    ON (FILENAME = '{expectedMdf}'),
                       (FILENAME = '{expectedLdf}')
                    FOR ATTACH", connection))
                        {
                            attachCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private void KillAllConnections(SqlConnection connection, string dbName) // ничего не получилось, наверное можно удалить
        {
            string sql = $@"
        DECLARE @kill varchar(max) = '';

        SELECT @kill = @kill + 'KILL ' + CONVERT(varchar(5), session_id) + ';'
        FROM sys.dm_exec_sessions
        WHERE database_id = DB_ID('{dbName}')

        EXEC(@kill);
    ";

            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void DropDatabaseIfExists(SqlConnection connection, string dbName) // ничего не получилось, наверное можно удалить
        {
            string sql = $@"
        IF DB_ID('{dbName}') IS NOT NULL
        BEGIN
            ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP DATABASE [{dbName}];
        END
    ";

            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

    }
}
