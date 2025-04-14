using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace EVA_Catalogue
{
    class EquipmentSelection
    {
        
        public void SelectDevicecs_ModularCircuitBreaker(List<string> producersListQF, List<string> seriesListQF, List<string> producersListQFD, List<string> seriesListQFD)
        {
            //ExcelHelperForEva excel = new ExcelHelperForEva(MainViewModel.SourceDirectoryExcel);

            ExcelHelperForEva excel = new ExcelHelperForEva();
            List<string> sheetsStartingWithEVA = excel.GetSheetsStartingWithEVA();
            foreach (string sheet in sheetsStartingWithEVA)
            {
                // получение массива данных из листа Excel
                object[][] dataFromExcelPage = excel.GetListDeviceFromExcel(MainViewModel.ModularCircuitBreakersSettings, sheet);
                DBHelper dBHelper = new DBHelper();
                int amountOfGroups = dataFromExcelPage.Length;
                List<string> produserOfDeviceList = new List<string>(); //инициализация списка для вывода в Excel
                List<string> codeOfDeviceList = new List<string>(); //инициализация списка для вывода в Excel
                List<string> nameOfDeviceList = new List<string>(); //инициализация списка для вывода в Excel
                                                                    // перебор массива данных из листа Excel(1 итерация - 1 устройство)
                for (int i = 0; i < amountOfGroups; i++)
                {
                    object typeOfDevice = dataFromExcelPage[i][0]; //переменные для поиска в БД
                    object ratedCurrent = dataFromExcelPage[i][1];
                    object numberOfPoles = dataFromExcelPage[i][2];
                    object maximumBreakingCapacity = dataFromExcelPage[i][3];
                    object responseCharacteristics = dataFromExcelPage[i][4];
                    object thermalOverloadRelease = dataFromExcelPage[i][5];
                    object leakageCurrent = "";
                    object additionalDevice11;
                    object ratedСurrentOfMouldedCase;
                    if (dataFromExcelPage[i].Length == 7)
                    {
                        additionalDevice11 = dataFromExcelPage[i][6];
                    }
                    else if (dataFromExcelPage[i].Length == 9)
                    {
                        ratedСurrentOfMouldedCase = dataFromExcelPage[i][7];
                        leakageCurrent = dataFromExcelPage[i][8];
                    }
                    DataSet ds = new DataSet();
                    if (typeOfDevice.ToString() == "Модульный автоматический выключатель")
                    {
                        if (seriesListQF.All(s => !string.IsNullOrEmpty(s)))
                        {
                            int amountOfSeriesQF = seriesListQF.Count;
                            int j = 0;
                            foreach (string series in seriesListQF)
                            {
                                string bdName = series.Split(':')[0];
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
                                    nameOfDeviceList.Add(DataForExcel(dtP)[0]);
                                    codeOfDeviceList.Add(DataForExcel(dtP)[1]);
                                    produserOfDeviceList.Add(bdName);
                                    break;
                                }
                                j++;

                            }
                        }
                        else if (producersListQF.All(s => !string.IsNullOrEmpty(s)))
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
                                    nameOfDeviceList.Add(DataForExcel(dtP)[0]);
                                    codeOfDeviceList.Add(DataForExcel(dtP)[1]);
                                    produserOfDeviceList.Add(bdName);
                                    break;
                                }
                                j++;
                            }
                        }
                    }
                    else if (typeOfDevice.ToString() == "Автоматический выключатель дифференциального тока")
                    {

                        if (seriesListQFD.All(s => !string.IsNullOrEmpty(s)))
                        {
                            int amountOfSeriesQFD = seriesListQFD.Count;
                            int j = 0;
                            foreach (string series in seriesListQFD)
                            {
                                string bdName = series.Split(':')[0];
                                string seriesName = series.Split(':')[1];

                                    string tableName = MainViewModel.TableNameModularResidualCurrentCircuitBreakers;
                                   ds = dBHelper.GetDeviceQFDDataFromDBbyDBNameSeriesName(bdName, tableName, seriesName, ratedCurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity, thermalOverloadRelease, leakageCurrent);
                                
                                //поиск модульного автомата в БД выбранного производителя и выбранной серии

                             
                                DataTable dtP = new DataTable();
                                dtP = ds.Tables[0];
                                if (dtP.Rows.Count != 0 | j == amountOfSeriesQFD - 1)
                                {   //заполнение списков для вывода в Excel
                                    nameOfDeviceList.Add(DataForExcel(dtP)[0]);
                                    codeOfDeviceList.Add(DataForExcel(dtP)[1]);
                                    produserOfDeviceList.Add(bdName);
                                    break;
                                }
                                j++;

                            }
                        }
                        else if (producersListQFD.All(s => !string.IsNullOrEmpty(s)))
                        {
                            int amountOfProducersQFD = producersListQFD.Count;
                            int j = 0;
                            foreach (string producer in producersListQFD)
                            {
                                string bdName = producer;

                                string tableName = MainViewModel.TableNameModularResidualCurrentCircuitBreakers;
                                ds = dBHelper.GetDeviceQFDDataFromDBbyDBName(bdName, tableName, ratedCurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity, thermalOverloadRelease, leakageCurrent);


                                DataTable dtP = new DataTable();
                                dtP = ds.Tables[0];
                                if (dtP.Rows.Count != 0 | j == amountOfProducersQFD - 1)
                                {   //заполнение списков для вывода в Excel
                                    nameOfDeviceList.Add(DataForExcel(dtP)[0]);
                                    codeOfDeviceList.Add(DataForExcel(dtP)[1]);
                                    produserOfDeviceList.Add(bdName);
                                    break;
                                }
                                j++;
                            }
                        }
                    }

                    //вывод в Excel
                    //excel.WhriteDevice1DataToExcel(produserOfDeviceList, codeOfDeviceList, nameOfDeviceList);
                }
                //вывод в Excel
                excel.WhriteDevice1DataToExcel(produserOfDeviceList, codeOfDeviceList, nameOfDeviceList, sheet);
            }
        }
            private List<string> DataForExcel(DataTable tableFromDB)
            {
                List<string> deviceDataForExsel = new List<string>();
                if (tableFromDB.Rows.Count != 0)
                {
                    DataRow dr = tableFromDB.NewRow();
                    dr = tableFromDB.Rows[0];
                    string nameOfDevice = dr["NameD"].ToString();
                    deviceDataForExsel.Add(nameOfDevice);
                    string codeOfDevice = dr["Code"].ToString();
                    deviceDataForExsel.Add(codeOfDevice);
                }
                else
                {
                    deviceDataForExsel.Add("Устройство не найдено");
                    deviceDataForExsel.Add("");
                }
                return deviceDataForExsel;
            }
        }
    } 


