using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;


namespace EVA_Catalogue
{
    class ExcelHelper
    {      
        string path;
        Excel.Workbook excelWB;
        Excel.Worksheet excelWS;
        Excel.Application excel = new Excel.Application();        
        public ExcelHelper(string path, string nameSheet)
        {
            this.path = path;
            excelWB = excel.Workbooks.Open(path);
            excelWS = (Excel.Worksheet)excelWB.Sheets.get_Item(nameSheet);
        }
        public object[][] GetListDevice1FromExcel()
        {
            int i = 3;
            int counter = 0;
            while (excelWS.Cells[2, i].Value != null)
            {
                ++counter;
                ++i;
            }
            object [][] Devices1 = new object[counter][];
            i = 0;                
            for (int j=3; j<3+counter+0; j++)
            {
                if (excelWS.Cells[15, j].Value != null)
                {
                    string currentTypeOfDevice1FromExcel = excelWS.Cells[15, j].Value.ToString();                   
                    float currentRatedСurrentOfMouldedCaseFromExcel = float.Parse((excelWS.Cells[20, j].Value ?? 0).ToString());                 
                    float currentRatedСurrentFromExcel = float.Parse((excelWS.Cells[17, j].Value ?? 0).ToString());
                    string currentResponseCharacteristicsFromExcel = (excelWS.Cells[18, j].Value ?? string.Empty).ToString();
                    object NumberOfPolesFromExcel = excelWS.Cells[12, j][0].Value;
                    int currentNumberOfPolesFromExcel = int.Parse((excelWS.Cells[12, j][0].Value ?? 0).ToString());
                    float currentLeakageСurrentFromExcel = float.Parse((excelWS.Cells[21, j].Value.Substring(0, excelWS.Cells[21, j].Value.Length - 2) ?? 0).ToString())/1000;
                    float currentMaximumBreakingCapacityFromExcel = float.Parse((excelWS.Cells[19, j].Value ?? 0).ToString()); //.Replace('.', ',')

                    if (currentTypeOfDevice1FromExcel.Contains("QFD"))
                    {
                        Devices1[i] = new object[9];

                        Devices1[i][0] = "Автоматический выключатель дифференциального тока";
                        Devices1[i][1] = currentRatedСurrentFromExcel;
                        Devices1[i][2] = currentNumberOfPolesFromExcel + 1;
                        Devices1[i][3] = currentMaximumBreakingCapacityFromExcel;
                        Devices1[i][4] = currentResponseCharacteristicsFromExcel;
                        Devices1[i][5] = 1;// наличие теплового расцепителя 
                        if (currentTypeOfDevice1FromExcel.Contains("Н.Р."))
                        {
                            Devices1[i][6] = "Независимый расцепитель";
                        }
                        else if (currentTypeOfDevice1FromExcel.Contains("AFDD"))
    
                        {
                            Devices1[i][6] = "Устройство защиты от дугового пробоя";
                        }
                        Devices1[i][7] = currentRatedСurrentOfMouldedCaseFromExcel;
                        Devices1[i][8] = currentLeakageСurrentFromExcel;
                    }
                    else if (currentTypeOfDevice1FromExcel.Contains("FU"))
                    {
                        Devices1[i] = new object[5];

                        Devices1[i][0] = "Предохранитель с плавкой вставкой";
                        Devices1[i][1] = currentRatedСurrentFromExcel;
                        Devices1[i][2] = currentNumberOfPolesFromExcel;
                        Devices1[i][3] = currentMaximumBreakingCapacityFromExcel;
                        Devices1[i][4] = currentResponseCharacteristicsFromExcel;
                    }                    
                    else if (currentTypeOfDevice1FromExcel.Contains("QF"))
                    {
                        Devices1[i] = new object[7];
                        if (currentTypeOfDevice1FromExcel.Contains("Выкатной"))
                        {
                            Devices1[i][0] = "Автоматический выключатель выкатной";
                            Devices1[i][5] = 1;
                        }
                        else if (currentTypeOfDevice1FromExcel.Contains("без_тепл.р.") && currentRatedСurrentOfMouldedCaseFromExcel == 0)
                        {
                            Devices1[i][0] = "Модульный автоматический выключатель без теплового расцепителя";
                            Devices1[i][5] = 0;
                        }
                        else if (currentTypeOfDevice1FromExcel.Contains("без_тепл.р.") && currentRatedСurrentOfMouldedCaseFromExcel != 0)
                        {
                            Devices1[i][0] = "Автоматический выключатель без теплового расцепителя в литом корпусе";
                            Devices1[i][5] = 1;
                        }
                        else if (currentRatedСurrentOfMouldedCaseFromExcel == 0)
                        {
                            Devices1[i][0] = "Модульный автоматический выключатель";
                            Devices1[i][5] = 1;
                        }
                        else
                        {
                            Devices1[i][0] = "Автоматический выключатель в литом корпусе";
                            Devices1[i][5] = 1;
                        }

                        Devices1[i][1] = currentRatedСurrentFromExcel;
                        if (currentTypeOfDevice1FromExcel == "QF+N")
                        {
                            Devices1[i][2] = currentNumberOfPolesFromExcel + 1;
                        }
                        else Devices1[i][2] = currentNumberOfPolesFromExcel;
                        Devices1[i][3] = currentMaximumBreakingCapacityFromExcel;
                        Devices1[i][4] = currentResponseCharacteristicsFromExcel;
                        Devices1[i][5] = 1;
                        if (currentTypeOfDevice1FromExcel.Contains("Н.Р."))
                        {
                            Devices1[i][6] = "Независимый расцепитель";
                        }
                        else if (currentTypeOfDevice1FromExcel.Contains("AFDD"))

                        {
                            Devices1[i][6] = "Устройство защиты от дугового пробоя";
                        }
                    }
                    else
                    {
                        Devices1[i] = new object[1];
                        Devices1[i][0] = "0";
                    }
                }
                else
                {
                    Devices1[i] = new object[1];
                    Devices1[i][0] = "0";
                }
                ++i;
            }          
            return Devices1;
        }
        public void WhriteDevice1DataToExcel(string produserName, List<string> codeOfDeviceList, List<string> nameOfDeviceList)
        {
            for (int i = 0; i < codeOfDeviceList.Count; i++)
            {
                if (nameOfDeviceList[i].ToString() != "Устройство не найдено")
                {
                    excelWS.Cells[157, 3 + i].Value = produserName.ToString();
                    excelWS.Cells[156, 3 + i].Value = codeOfDeviceList[i].ToString();
                    excelWS.Cells[154, 3 + i].Value = nameOfDeviceList[i].ToString();
                }
                else
                {
                    excelWS.Cells[154, 3 + i].Value = nameOfDeviceList[i].ToString();
                    excelWS.Cells[156, 3 + i].Value = " ";
                    excelWS.Cells[157, 3 + i].Value = " ";
                }                
            }
            excelWB.Save();
            excel.Quit();
        }
    }
}
