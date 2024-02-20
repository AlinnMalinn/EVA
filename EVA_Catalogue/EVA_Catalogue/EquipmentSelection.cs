using System.Collections.Generic;
using System.Data;

namespace EVA_Catalogue
{
    class EquipmentSelection
    {
        public void selectDevicecs(string ProducerName, int seriesID)
        {
            // получение массива данных из листа Excel
            ExcelHelper excel = new ExcelHelper(@"C:\Users\79126\Desktop\TEST.xlsm", "EVA_1РП1");  
            object[][] dataFromExcelPage = excel.GetListDevice1FromExcel(); 
            DBHelper dBHelper = new DBHelper();
            int amountOfGroups = dataFromExcelPage.Length;
            List<string> codeOfDeviceList = new List<string>(); //инициализация списка для вывода в Excel
            List<string> nameOfDeviceList = new List<string>(); //инициализация списка для вывода в Excel
            // перебор массива данных из листа Excel(1 итерация - 1 устройство)
            for (int i = 0; i < amountOfGroups; i++) 
            {
                object typeOfDevice = dataFromExcelPage[i][0]; //переменные для поиска в БД
                object ratedСurrent = dataFromExcelPage[i][1];
                object numberOfPoles = dataFromExcelPage[i][2];
                object maximumBreakingCapacity = dataFromExcelPage[i][3];
                object responseCharacteristics = dataFromExcelPage[i][4];
                object thermalOverloadRelease = dataFromExcelPage[i][5];
                object leakageСurrent ="";
                object additionalDevice11;
                object ratedСurrentOfMouldedCase;
                if (dataFromExcelPage[i].Length == 7)
                {
                    additionalDevice11 = dataFromExcelPage[i][6];
                }
                else if (dataFromExcelPage[i].Length == 9)
                {
                    ratedСurrentOfMouldedCase = dataFromExcelPage[i][7];
                    leakageСurrent = dataFromExcelPage[i][8];
                }
                DataSet ds = new DataSet();
                string bdName = ProducerName;
                // поиск модульного диф автомата в БД выбранного производителя и выбранной серии
                if (typeOfDevice.ToString().Contains("дифференциального")) 
                {
                    string tableName = @"[Модульные автоматические выключатели дифференциального тока]";
                    ds = dBHelper.GetDeviceDataFromDB2(bdName, tableName, seriesID, ratedСurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity, thermalOverloadRelease, leakageСurrent);
                }
                //поиск модульного автомата в БД выбранного производителя и выбранной серии
                else if (typeOfDevice.ToString().Contains("втоматический выключатель")) 
                {
                    string tableName = @"[Модульные автоматические выключатели]";
                    ds = dBHelper.GetDeviceDataFromDB1(bdName, tableName, seriesID, ratedСurrent, numberOfPoles, responseCharacteristics, maximumBreakingCapacity, thermalOverloadRelease);
                }
                
                DataTable dtP = new DataTable();
                dtP = ds.Tables[0];
                //заполнение списков для вывода в Excel
                if (dtP.Rows.Count != 0) 
                {
                  DataRow dr = dtP.NewRow();
                  dr = dtP.Rows[0];
                  string nameOfDevice = dr["NameD"].ToString();
                  nameOfDeviceList.Add(nameOfDevice);
                  string codeOfDevice = dr["Code"].ToString();
                  codeOfDeviceList.Add(codeOfDevice);
                }
                else
                {
                  nameOfDeviceList.Add("Устройство не найдено");
                  codeOfDeviceList.Add("");
                }                   
            }
            //вывод в Excel
            excel.WhriteDevice1DataToExcel(ProducerName, codeOfDeviceList, nameOfDeviceList);
        }
    }
}

