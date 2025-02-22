using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;


namespace EVA_Catalogue
{
    class ExcelHelperForEva
    {
        string path;
        Excel.Workbook excelWB;
        //Excel.Worksheet excelWS;
        Excel.Application excel = new Excel.Application();
        //public ExcelHelperForEva(string path, string nameSheet)
        //{
        //    this.path = path;
        //    excelWB = excel.Workbooks.Open(path);
        //    excelWS = (Excel.Worksheet)excelWB.Sheets.get_Item(nameSheet);
        // }
        public ExcelHelperForEva(string path)
        {
            this.path = path;
            excelWB = excel.Workbooks.Open(path);
        }
        public List<string> GetSheetsStartingWithEVA()
        {
            List<string> sheetsStartingWithEVA = new List<string>();


            try
            {
                // Открытие книги Excel

                // Перебор всех листов в книге
                foreach (Excel.Worksheet sheet in excelWB.Sheets)
                {
                    // Проверка, начинается ли имя листа с "EVA"
                    if (sheet.Name.StartsWith("EVA", StringComparison.OrdinalIgnoreCase))
                    {
                        sheetsStartingWithEVA.Add(sheet.Name);
                    }
                }
                

            }

            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            return sheetsStartingWithEVA;
        }

        public object[][] GetListDeviceFromExcel(string deviceType, string nameSheet)
        {
            Excel.Worksheet excelWS;
            excelWS = (Excel.Worksheet)excelWB.Sheets.get_Item(nameSheet);
            int i = 3;
            int counter = 0;
            while (excelWS.Cells[2, i].Value != null)
            {
                ++counter;
                ++i;
            }
            object[][] Devices = new object[counter][];
            i = 0;
            if (deviceType == MainViewModel.ModularCircuitBreakersSettings)
            {
                for (int j = 3; j < 3 + counter + 0; j++)
                {
                    if (excelWS.Cells[15, j].Value != null)
                    {
                        string currentTypeOfDevice1FromExcel = excelWS.Cells[15, j].Value.ToString();
                        float currentRatedCurrentOfMouldedCaseFromExcel = float.Parse((excelWS.Cells[20, j].Value ?? 0).ToString());
                        float currentRatedCurrentFromExcel = float.Parse((excelWS.Cells[17, j].Value ?? 0).ToString());
                        string currentResponseCharacteristicsFromExcel = (excelWS.Cells[18, j].Value ?? string.Empty).ToString();
                        object NumberOfPolesFromExcel = excelWS.Cells[12, j][0].Value;
                        int currentNumberOfPolesFromExcel = int.Parse((excelWS.Cells[12, j][0].Value ?? 0).ToString());
                        float currentLeakageCurrentFromExcel = float.Parse((excelWS.Cells[21, j].Value.Substring(0, excelWS.Cells[21, j].Value.Length - 2) ?? 0).ToString()) / 1000;
                        float currentMaximumBreakingCapacityFromExcel = float.Parse((excelWS.Cells[19, j].Value ?? 0).ToString()); //.Replace('.', ',')

                        if (currentTypeOfDevice1FromExcel.Contains("QF"))
                        {
                            Devices[i] = new object[7];
                            if (currentTypeOfDevice1FromExcel.Contains("без_тепл.р.") && currentRatedCurrentOfMouldedCaseFromExcel == 0)
                            {
                                Devices[i][0] = "Модульный автоматический выключатель без теплового расцепителя";
                                Devices[i][5] = 0;
                            }
                            //else if (currentTypeOfDevice1FromExcel.Contains("без_тепл.р.") && currentRatedCurrentOfMouldedCaseFromExcel != 0)
                            //{
                            //    Devices[i][0] = "Автоматический выключатель без теплового расцепителя в литом корпусе";
                            //    Devices[i][5] = 1;
                            //}
                            else if (currentRatedCurrentOfMouldedCaseFromExcel == 0)
                            {
                                Devices[i][0] = "Модульный автоматический выключатель";
                                Devices[i][5] = 1;
                            }
                            Devices[i][1] = currentRatedCurrentFromExcel;
                            Devices[i][2] = currentNumberOfPolesFromExcel;
                            Devices[i][3] = currentMaximumBreakingCapacityFromExcel;
                            Devices[i][4] = currentResponseCharacteristicsFromExcel;
                            Devices[i][5] = 1;
                            Devices[i][6] = "В будущем тут будет указание о наличии второго устройства";
                        }
                        else
                        {
                            Devices[i] = new object[1];
                            Devices[i][0] = "0";
                        }
                    }
                    else
                    {
                        Devices[i] = new object[1];
                        Devices[i][0] = "0";
                    }
                    ++i;
                }
            }
            return Devices;
        }
        public object[][] GetListDevice1FromExcel_(string nameSheet) // запасной метод
        {
            Excel.Worksheet excelWS;
            excelWS = (Excel.Worksheet)excelWB.Sheets.get_Item(nameSheet);
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
                    float currentRatedCurrentOfMouldedCaseFromExcel = float.Parse((excelWS.Cells[20, j].Value ?? 0).ToString());                 
                    float currentRatedCurrentFromExcel = float.Parse((excelWS.Cells[17, j].Value ?? 0).ToString());
                    string currentResponseCharacteristicsFromExcel = (excelWS.Cells[18, j].Value ?? string.Empty).ToString();
                    object NumberOfPolesFromExcel = excelWS.Cells[12, j][0].Value;
                    int currentNumberOfPolesFromExcel = int.Parse((excelWS.Cells[12, j][0].Value ?? 0).ToString());
                    float currentLeakageCurrentFromExcel = float.Parse((excelWS.Cells[21, j].Value.Substring(0, excelWS.Cells[21, j].Value.Length - 2) ?? 0).ToString())/1000;
                    float currentMaximumBreakingCapacityFromExcel = float.Parse((excelWS.Cells[19, j].Value ?? 0).ToString()); //.Replace('.', ',')

                    if (currentTypeOfDevice1FromExcel.Contains("QFD"))
                    {
                        Devices1[i] = new object[9];

                        Devices1[i][0] = "Автоматический выключатель дифференциального тока";
                        Devices1[i][1] = currentRatedCurrentFromExcel;
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
                        Devices1[i][7] = currentRatedCurrentOfMouldedCaseFromExcel;
                        Devices1[i][8] = currentLeakageCurrentFromExcel;
                    }
                    else if (currentTypeOfDevice1FromExcel.Contains("FU"))
                    {
                        Devices1[i] = new object[5];

                        Devices1[i][0] = "Предохранитель с плавкой вставкой";
                        Devices1[i][1] = currentRatedCurrentFromExcel;
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
                        else if (currentTypeOfDevice1FromExcel.Contains("без_тепл.р.") && currentRatedCurrentOfMouldedCaseFromExcel == 0)
                        {
                            Devices1[i][0] = "Модульный автоматический выключатель без теплового расцепителя";
                            Devices1[i][5] = 0;
                        }
                        else if (currentTypeOfDevice1FromExcel.Contains("без_тепл.р.") && currentRatedCurrentOfMouldedCaseFromExcel != 0)
                        {
                            Devices1[i][0] = "Автоматический выключатель без теплового расцепителя в литом корпусе";
                            Devices1[i][5] = 1;
                        }
                        else if (currentRatedCurrentOfMouldedCaseFromExcel == 0)
                        {
                            Devices1[i][0] = "Модульный автоматический выключатель";
                            Devices1[i][5] = 1;
                        }
                        else
                        {
                            Devices1[i][0] = "Автоматический выключатель в литом корпусе";
                            Devices1[i][5] = 1;
                        }

                        Devices1[i][1] = currentRatedCurrentFromExcel;
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
        public void WhriteDevice1DataToExcel(List<string> produserNameList, List<string> codeOfDeviceList, List<string> nameOfDeviceList, string nameSheet)
        {
            Excel.Worksheet excelWS;
            excelWS = (Excel.Worksheet)excelWB.Sheets.get_Item(nameSheet);
            for (int i = 0; i < codeOfDeviceList.Count; i++)
            {
                if (nameOfDeviceList[i].ToString() != "Устройство не найдено")
                {
                    excelWS.Cells[157, 3 + i].Value = produserNameList[i].ToString();
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
            //excel.Quit();
        }
        public void QuitExcel()
        {
            excel.Quit();
        }
    }
}
