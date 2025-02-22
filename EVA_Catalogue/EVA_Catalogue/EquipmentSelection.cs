using System.Collections.Generic;
using System.Data;

namespace EVA_Catalogue
{
    class EquipmentSelection
    {
        //const string PathToExcel = @"C:\Users\79126\Desktop\EVA\TEST.xlsm";
        //const string Page = "EVA_1РП1";
        //const string TableNameModularCircuitBreakers = @"[Модульные автоматические выключатели]";
        //const string TableNameModularResidualCurrentCircuitBreakers = @"[Модульные автоматические выключатели дифференциального тока]";

        public void SelectDevicecs_ModularCircuitBreaker(List<string> producersList, List<string> seriesList)
        {
            ExcelHelperForEva excel = new ExcelHelperForEva(MainViewModel.SourceDirectoryExcel);
            List<string> sheetsStartingWithEVA = excel.GetSheetsStartingWithEVA();
            foreach (string sheet in sheetsStartingWithEVA)
            {
                // получение массива данных из листа Excel
                //ExcelHelperForEva excel = new ExcelHelperForEva(MainViewModel.SourceDirectoryExcel, MainViewModel.ExcelPage);
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
                    if (seriesList[0] != "")
                    {
                        foreach (string series in seriesList)
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
                            if (dtP.Rows.Count != 0)
                            {   //заполнение списков для вывода в Excel
                                nameOfDeviceList.Add(DataForExcel(dtP)[0]);
                                codeOfDeviceList.Add(DataForExcel(dtP)[1]);
                                produserOfDeviceList.Add(bdName);
                                break;
                            }

                        }
                    }
                    else if (producersList[0] != "")
                    {
                        foreach (string producer in producersList)
                        {
                            string bdName = producer;

                            string tableName = MainViewModel.TableNameModularCircuitBreakers;
                            ds = dBHelper.GetDeviceDataFromDBbyDBName(bdName, tableName, ratedCurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity, thermalOverloadRelease);


                            DataTable dtP = new DataTable();
                            dtP = ds.Tables[0];
                            if (dtP.Rows.Count != 0)
                            {   //заполнение списков для вывода в Excel
                                nameOfDeviceList.Add(DataForExcel(dtP)[0]);
                                codeOfDeviceList.Add(DataForExcel(dtP)[1]);
                                produserOfDeviceList.Add(bdName);
                                break;
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


