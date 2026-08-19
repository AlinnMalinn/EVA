using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVA_CatalogueManual
{
    class EquipmentSelection
    {
        public const int PositionInArray_deviceInfo = 0;
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

        public DataTable SelectDevicecs(List<string> seriesList)
        {
            ExcelHelperForEva excel = new ExcelHelperForEva();
           
                // получение массива данных из листа Excel
                object[] dataFromExcelPage = excel.GetListDeviceFromExcel();
                DBHelper dBHelper = new DBHelper();
                List<string> producerOfDeviceList = new List<string>(); //инициализация списка для вывода в Excel
                List<string> codeOfDeviceList = new List<string>(); //инициализация списка для вывода в Excel
                List<string> markOfDeviceList = new List<string>(); //инициализация списка для вывода в Excel
                List<string> nameOfDeviceList = new List<string>(); //инициализация списка для вывода в Excel
                List<string> maximumBreakingCapacityList = new List<string>(); //инициализация списка для вывода в Excel


            object deviceInfo = dataFromExcelPage[PositionInArray_deviceInfo];
            object typeOfDevice = dataFromExcelPage[PositionInArray_typeOfDevice]; //переменные для поиска в БД
            object ratedCurrent = dataFromExcelPage[PositionInArray_ratedCurrent];
            object numberOfPoles = dataFromExcelPage[PositionInArray_numberOfPoles];
            object maximumBreakingCapacity = dataFromExcelPage[PositionInArray_maximumBreakingCapacity];
            object responseCharacteristics = dataFromExcelPage[PositionInArray_responseCharacteristics];
            object thermalOverloadRelease = dataFromExcelPage[PositionInArray_thermalOverloadRelease];
            object leakageCurrent = "";
            object additionalDevice11;
            object ratedСurrentOfMouldedCase;
            object residualCurrentType = "";
            string effectiveResidualCurrentType = "";
            DataTable dtP = new DataTable();
            //DataTable dtK = new DataTable();
            if (dataFromExcelPage.Length == 8)
                    {
                        additionalDevice11 = dataFromExcelPage[PositionInArray_additionalDevice11];
            }
                    else if (dataFromExcelPage.Length == 11)
                    {
                ratedСurrentOfMouldedCase = dataFromExcelPage[PositionInArray_ratedСurrentOfMouldedCase];
                leakageCurrent = dataFromExcelPage[PositionInArray_leakageCurrent];
                residualCurrentType = dataFromExcelPage[PositionInArray_residualCurrentType];
                effectiveResidualCurrentType = string.IsNullOrEmpty(residualCurrentType?.ToString()) ? "A" : residualCurrentType.ToString();//если нет значения, то умолчанию выбираем характеристику А
            }
                    DataSet ds = new DataSet();
                    if (typeOfDevice.ToString().Contains("Модульный автоматический выключатель"))
                    {
                        if (seriesList.All(s => !string.IsNullOrEmpty(s)))
                        {
                           foreach (string series in seriesList)
                            {
                                string bdName = series.Split(':')[0];
                        //string seriesName = "[" + series.Split(':')[1] + "]";                             
                        string seriesName = series.Split(':')[1];

                        string tableName = MainViewModel.TableNameModularCircuitBreakers;
                                ds = dBHelper.GetDeviceDataFromDBbyDBNameSeriesName(bdName, tableName, seriesName, ratedCurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity, thermalOverloadRelease);
                        ds.Tables[0].Columns.RemoveAt(0);
                        DataColumn newColumn = new DataColumn("Producer", typeof(string));
                        // Вставляем столбец на первую позицию (индекс 0)
                        ds.Tables[0].Columns.Add(newColumn);
                        newColumn.SetOrdinal(0); // перемещаем в начало
                        DataColumn newColumn2 = new DataColumn("DeviceInfo", typeof(string));
                        ds.Tables[0].Columns.Add(newColumn2);

                        // 2. Заполняем все строки значением producer
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            row["Producer"] = bdName.ToString().Split('_')[0];
                            row["DeviceInfo"] =
                               deviceInfo +
                               row["MaximumBreakingCapacity"].ToString() +
                               row["Mark"].ToString() +
                               row["NameD"].ToString() +
                               row["Code"].ToString() +
                               bdName.ToString().Split('_')[0];
                        }
                       
                       
                        dtP.Merge(ds.Tables[0]);                              
                        }
                        }      
                    }
                    else if (typeOfDevice.ToString() == "Автоматический выключатель дифференциального тока")
                    {

                if (seriesList.All(s => !string.IsNullOrEmpty(s)))
                {
                    foreach (string series in seriesList)
                    {
                        string bdName = series.Split(':')[0];
                        string seriesName = series.Split(':')[1];

                        string tableName = MainViewModel.TableNameModularResidualCurrentCircuitBreakers;
                        ds = dBHelper.GetDeviceQFDDataFromDBbyDBNameSeriesName(bdName, tableName, seriesName, ratedCurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity, thermalOverloadRelease, leakageCurrent, effectiveResidualCurrentType);
                        ds.Tables[0].Columns.RemoveAt(0);
                        //поиск модульного автомата в БД выбранного производителя и выбранной серии
                        DataColumn newColumn = new DataColumn("Producer", typeof(string));
                        // Вставляем столбец на первую позицию (индекс 0)
                        ds.Tables[0].Columns.Add(newColumn);
                        newColumn.SetOrdinal(0); // перемещаем в начало
                        DataColumn newColumn2 = new DataColumn("DeviceInfo", typeof(string));
                        ds.Tables[0].Columns.Add(newColumn2);

                        // 2. Заполняем все строки значением "a"
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            row["Producer"] = bdName.ToString().Split('_')[0];
                            row["DeviceInfo"] =
                               deviceInfo +
                               row["MaximumBreakingCapacity"].ToString() +
                               row["Mark"].ToString() +
                               row["NameD"].ToString() +
                               row["Code"].ToString() +
                               bdName.ToString().Split('_')[0];


                        }

                        dtP.Merge(ds.Tables[0]);
                    }

                }
                    }               
                
                //вывод в Excel
                //excel.WhriteDevice1DataToExcel(producerOfDeviceList, codeOfDeviceList, nameOfDeviceList, markOfDeviceList, sheet, maximumBreakingCapacityList);
            return dtP;
}
    }
}
