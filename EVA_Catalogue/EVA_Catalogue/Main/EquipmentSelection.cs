using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace EVA_Catalogue
{
    class EquipmentSelection
    {
        public const int PositionInArray_deviceInfo = 0;// информация с параметрами аппарата из Excel для последующей вставки в строку 275
        public const int PositionInArray_typeOfDevice = 1;
        public const int PositionInArray_ratedCurrent = 2;
        public const int PositionInArray_numberOfPoles = 3;
        public const int PositionInArray_maximumBreakingCapacity = 4;
        public const int PositionInArray_responseCharacteristics = 5;
        public const int PositionInArray_thermalOverloadRelease = 6;
        public const int PositionInArray_additionalDevice11 = 7;
        public const int PositionInArray_ratedСurrentOfMouldedCase = 8;
        public const int PositionInArray_leakageCurrent = 9;
        public const int PositionInArray_residualCurrentType = 10;
        public List<List<string>> SelectDevicecs_ModularCircuitBreaker(List<string> producersListQF, List<string> seriesListQF, List<string> producersListQFD, List<string> seriesListQFD, bool isQFEnabled, bool isQFDEnabled, string selectedSheetName = null)
        {
            //ExcelHelperForEva excel = new ExcelHelperForEva(MainViewModel.SourceDirectoryExcel);
            List<List<string>> failedItems = new List<List<string>>();
            List<string> failedItemsQF = new List<string>();
            List<string> failedItemsQFD = new List<string>();
            ExcelHelperForEva excel = new ExcelHelperForEva();
            List<string> sheetsStartingWithEVA = string.IsNullOrWhiteSpace(selectedSheetName)
                ? excel.GetSheetsStartingWithEVA()
                : new List<string> { selectedSheetName };
            foreach (string sheet in sheetsStartingWithEVA)
            {
                // получение массива данных из листа Excel
                object[][] dataFromExcelPage = excel.GetListDeviceFromExcel(MainViewModel.ModularCircuitBreakersSettings, sheet);
                DBHelper dBHelper = new DBHelper();
                int amountOfGroups = dataFromExcelPage.Length;
                List<string> producerOfDeviceList = new List<string>(); //инициализация списка для вывода в Excel
                List<string> codeOfDeviceList = new List<string>(); //инициализация списка для вывода в Excel
                List<string> markOfDeviceList = new List<string>(); //инициализация списка для вывода в Excel
                List<string> nameOfDeviceList = new List<string>(); //инициализация списка для вывода в Excel
                List<string> maximumBreakingCapacityList = new List<string>(); //инициализация списка для вывода в Excel
                List<string> deviceInfoList = new List<string>(); //инициализация списка для вывода в Excel

                failedItemsQF.Add(sheet);
                failedItemsQFD.Add(sheet);
                int a = 0;// для отслеживания элементов QF, которые не удалось подобрать 
                int b = 0;// для отслеживания элементов QFD, которые не удалось подобрать 

                // перебор массива данных из листа Excel(1 итерация - 1 устройство)
                for (int i = 0; i < amountOfGroups; i++)
                {

                    object deviceInfo = dataFromExcelPage[i][PositionInArray_deviceInfo];
                    object typeOfDevice = dataFromExcelPage[i][PositionInArray_typeOfDevice]; //переменные для поиска в БД
                    object ratedCurrent = dataFromExcelPage[i][PositionInArray_ratedCurrent];
                    object numberOfPoles = dataFromExcelPage[i][PositionInArray_numberOfPoles];
                    object maximumBreakingCapacity = dataFromExcelPage[i][PositionInArray_maximumBreakingCapacity];
                    object responseCharacteristics = dataFromExcelPage[i][PositionInArray_responseCharacteristics];
                    object thermalOverloadRelease = dataFromExcelPage[i][PositionInArray_thermalOverloadRelease];
                    object leakageCurrent = "";
                    object additionalDevice11;
                    object ratedСurrentOfMouldedCase;
                    object residualCurrentType = "";
                    string effectiveResidualCurrentType = "";
                    if (dataFromExcelPage[i].Length == 8)
                    {
                        additionalDevice11 = dataFromExcelPage[i][PositionInArray_additionalDevice11];
                    }
                    else if (dataFromExcelPage[i].Length == 11)
                    {
                        ratedСurrentOfMouldedCase = dataFromExcelPage[i][PositionInArray_ratedСurrentOfMouldedCase];
                        leakageCurrent = dataFromExcelPage[i][PositionInArray_leakageCurrent];
                        residualCurrentType = dataFromExcelPage[i][PositionInArray_residualCurrentType];
                        effectiveResidualCurrentType = string.IsNullOrEmpty(residualCurrentType?.ToString()) ? "A" : residualCurrentType.ToString();//если нет значения, то умолчанию выбираем характеристику А

                    }
                    DataSet ds = new DataSet();
                    if (typeOfDevice.ToString().Contains("Модульный автоматический выключатель")& isQFEnabled == true)
                    {
                        if (seriesListQF.Count > 0 && seriesListQF.All(s => !string.IsNullOrEmpty(s)))
                        {
                            int amountOfSeriesQF = seriesListQF.Count;
                            int j = 0;
                            foreach (string series in seriesListQF)
                            {
                                string bdName = series.Split(':')[0];
                                // string seriesName = "["+series.Split(':')[1]+"]";
                                string seriesName = series.Split(':')[1];

                                // поиск модульного диф автомата в БД выбранного производителя и выбранной серии
                                // if (typeOfDevice.ToString().Contains("дифференциального")) 
                                // {
                                //     string tableName = MainViewModel.TableNameModularResidualCurrentCircuitBreakers;
                                //     ds = dBHelper.GetDeviceDataFromDB2(bdName, tableName, seriesID, ratedCurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity, thermalOverloadRelease, leakageCurrent);
                                // }
                                //поиск модульного автомата в БД выбранного производителя и выбранной серии

                                string tableName = MainViewModel.TableNameModularCircuitBreakers;
                                ds = dBHelper.GetDeviceDataFromDBbyDBNameSeriesName(bdName, tableName, seriesName, ratedCurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity, thermalOverloadRelease);


                                DataTable dtP = new DataTable();
                                dtP = ds.Tables[0];
                                if (dtP.Rows.Count != 0 | j == amountOfSeriesQF - 1)
                                {   //заполнение списков для вывода в Excel
                                    List<string> deviceDataForExсel = DataForExcel(dtP);
                                    nameOfDeviceList.Add(deviceDataForExсel[0]);
                                    codeOfDeviceList.Add(deviceDataForExсel[1]);
                                    markOfDeviceList.Add(deviceDataForExсel[2]);
                                    maximumBreakingCapacityList.Add(deviceDataForExсel[3]);
                                    producerOfDeviceList.Add(bdName.ToString().Split('_')[0]);
                                    deviceInfoList.Add(deviceInfo + deviceDataForExсel[3] + deviceDataForExсel[2] + deviceDataForExсel[0] + deviceDataForExсel[1] + bdName.ToString().Split('_')[0]);
                                    if (deviceDataForExсel[0] == " ")
                                    {
                                        failedItemsQF.Add(GetExcelColumnName(i + 3));
                                        a++;
                                    }
                                    break;
                                }
                                j++;

                            }
                        }
                        else if (producersListQF.Count > 0 && producersListQF.All(s => !string.IsNullOrEmpty(s)))
                        {
                            int amountOfProducersQF = producersListQF.Count;
                            int j = 0;
                            foreach (string producer in producersListQF)
                            {
                                string bdName = producer;

                                string tableName = MainViewModel.TableNameModularCircuitBreakers;
                                ds = dBHelper.GetDeviceDataFromDBbyDBName(bdName, tableName, ratedCurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity, thermalOverloadRelease);


                                DataTable dtP = new DataTable();
                                dtP = ds.Tables[0];
                                if (dtP.Rows.Count != 0 | j == amountOfProducersQF - 1)
                                {   //заполнение списков для вывода в Excel
                                    List<string> deviceDataForExсel = DataForExcel(dtP);
                                    nameOfDeviceList.Add(deviceDataForExсel[0]);
                                    codeOfDeviceList.Add(deviceDataForExсel[1]);
                                    markOfDeviceList.Add(deviceDataForExсel[2]);
                                    maximumBreakingCapacityList.Add(deviceDataForExсel[3]);
                                    producerOfDeviceList.Add(bdName.ToString().Split('_')[0]);
                                    deviceInfoList.Add(deviceInfo + deviceDataForExсel[3] + deviceDataForExсel[2] + deviceDataForExсel[0] + deviceDataForExсel[1] + bdName.ToString().Split('_')[0]);
                                    if (deviceDataForExсel[0] == " ")
                                    {
                                        failedItemsQF.Add(GetExcelColumnName(i + 3));
                                        a++;
                                    }
                                    break;
                                }
                                j++;
                            }
                        }
                    }
                    else if (typeOfDevice.ToString() == "Автоматический выключатель дифференциального тока" & isQFDEnabled == true)
                    {

                        if (seriesListQFD.Count > 0 && seriesListQFD.All(s => !string.IsNullOrEmpty(s)))
                        {
                            int amountOfSeriesQFD = seriesListQFD.Count;
                            int j = 0;
                            foreach (string series in seriesListQFD)
                            {
                                string bdName = series.Split(':')[0];
                                //string seriesName = series.Split(':')[1];
                                string seriesName = series.Split(':')[1];

                                string tableName = MainViewModel.TableNameModularResidualCurrentCircuitBreakers;
                                ds = dBHelper.GetDeviceQFDDataFromDBbyDBNameSeriesName(bdName, tableName, seriesName, ratedCurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity, thermalOverloadRelease, leakageCurrent, effectiveResidualCurrentType);

                                //поиск модульного автомата в БД выбранного производителя и выбранной серии


                                DataTable dtP = new DataTable();
                                dtP = ds.Tables[0];
                                if (dtP.Rows.Count != 0 | j == amountOfSeriesQFD - 1)
                                {   //заполнение списков для вывода в Excel
                                    List<string> deviceDataForExсel = DataForExcel(dtP);
                                    nameOfDeviceList.Add(deviceDataForExсel[0]);
                                    codeOfDeviceList.Add(deviceDataForExсel[1]);
                                    markOfDeviceList.Add(deviceDataForExсel[2]);
                                    maximumBreakingCapacityList.Add(deviceDataForExсel[3]);
                                    producerOfDeviceList.Add(bdName.ToString().Split('_')[0]);
                                    deviceInfoList.Add(deviceInfo + deviceDataForExсel[3] + deviceDataForExсel[2] + deviceDataForExсel[0] + deviceDataForExсel[1] + bdName.ToString().Split('_')[0]);
                                    if (deviceDataForExсel[0] == " ")
                                    {
                                        failedItemsQFD.Add(GetExcelColumnName(i + 3));
                                        b++;
                                    }
                                    break;
                                }
                                j++;

                            }
                        }
                        else if (producersListQFD.Count > 0 && producersListQFD.All(s => !string.IsNullOrEmpty(s)))
                        {
                            int amountOfProducersQFD = producersListQFD.Count;
                            int j = 0;
                            foreach (string producer in producersListQFD)
                            {
                                string bdName = producer;

                                string tableName = MainViewModel.TableNameModularResidualCurrentCircuitBreakers;
                                ds = dBHelper.GetDeviceQFDDataFromDBbyDBName(bdName, tableName, ratedCurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity, thermalOverloadRelease, leakageCurrent, effectiveResidualCurrentType);


                                DataTable dtP = new DataTable();
                                dtP = ds.Tables[0];
                                if (dtP.Rows.Count != 0 | j == amountOfProducersQFD - 1)
                                {   //заполнение списков для вывода в Excel
                                    //nameOfDeviceList.Add(DataForExcel(dtP)[0]);
                                    List<string> deviceDataForExсel = DataForExcel(dtP);
                                    nameOfDeviceList.Add(deviceDataForExсel[0]);
                                    codeOfDeviceList.Add(deviceDataForExсel[1]);
                                    markOfDeviceList.Add(deviceDataForExсel[2]);
                                    maximumBreakingCapacityList.Add(deviceDataForExсel[3]);
                                    producerOfDeviceList.Add(bdName.ToString().Split('_')[0]);
                                    deviceInfoList.Add(deviceInfo + deviceDataForExсel[3] + deviceDataForExсel[2] + deviceDataForExсel[0] + deviceDataForExсel[1] + bdName.ToString().Split('_')[0]);
                                    if (deviceDataForExсel[0] == " ")
                                    {
                                        failedItemsQFD.Add(GetExcelColumnName(i + 3));
                                        b++;
                                    }
                                    break;
                                }
                                j++;
                            }
                        }                
                       
                    }
                    else
                    {
                        nameOfDeviceList.Add("");
                        codeOfDeviceList.Add("");
                        markOfDeviceList.Add("");
                        maximumBreakingCapacityList.Add("");
                        producerOfDeviceList.Add("");
                        deviceInfoList.Add("");

                    }
                
                    //вывод в Excel
                    //excel.WhriteDevice1DataToExcel(produserOfDeviceList, codeOfDeviceList, nameOfDeviceList);
                }
                //вывод в Excel
                excel.WhriteDevice1DataToExcel(deviceInfoList,producerOfDeviceList, codeOfDeviceList, nameOfDeviceList, markOfDeviceList, sheet, maximumBreakingCapacityList,  isQFEnabled, isQFDEnabled);
                if (a == 0)
                {
                    failedItemsQF.RemoveAt(failedItemsQF.Count - 1);
                }
                if (b == 0)
                {
                    failedItemsQFD.RemoveAt(failedItemsQFD.Count - 1);
                }
            }
            failedItems.Add(failedItemsQF);
            failedItems.Add(failedItemsQFD);
            return failedItems;
        }
        public static string GetExcelColumnName(int columnNumber)
        {
            string columnName = "";

            while (columnNumber > 0)
            {
                int remainder = (columnNumber - 1) % 26;
                columnName = (char)(65 + remainder) + columnName;
                columnNumber = (columnNumber - 1) / 26;
            }

            return columnName;
        }
        private List<string> DataForExcel(DataTable tableFromDB)
            {
                List<string> deviceDataForExсel = new List<string>();
                if (tableFromDB.Rows.Count != 0)
                {
                    DataRow dr = tableFromDB.NewRow();
                    dr = tableFromDB.Rows[0];
                    string nameOfDevice = dr["NameD"].ToString();
                    deviceDataForExсel.Add(nameOfDevice);
                    string codeOfDevice = dr["Code"].ToString();
                    deviceDataForExсel.Add(codeOfDevice);
                    string markOfDevice = dr["Mark"].ToString();
                    deviceDataForExсel.Add(markOfDevice);
                    string maximumBreakingCapacity = dr["MaximumBreakingCapacity"].ToString();
                    deviceDataForExсel.Add(maximumBreakingCapacity);
                }
                else
                {
                    deviceDataForExсel.Add(" ");
                    deviceDataForExсel.Add("");
                    deviceDataForExсel.Add("");
                    deviceDataForExсel.Add("");
                   
                }
            
                return deviceDataForExсel;
            }
        }
    } 


