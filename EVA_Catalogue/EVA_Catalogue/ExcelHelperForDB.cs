using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;


namespace EVA_Catalogue
{
    class ExcelHelperForDB
    {
        string path;
        Excel.Workbook excelWB;
        Excel.Worksheet excelWS;
        Excel.Application excel = new Excel.Application();
        public ExcelHelperForDB(string path)
        {
            this.path = path;
            excelWB = excel.Workbooks.Open(path);
            //excelWS = (Excel.Worksheet)excelWB.Sheets.get_Item(nameSheet);
        }
        public (List<object[]>, List<object[]>) GetListOfDevicesTypeFromDB()
        {
            List<object[]> DevicesQFForBD = new List<object[]>();
            List<object[]> DevicesQFDForBD = new List<object[]>();
            // object[][] DevicesForBD = new object[10 - 1][];
            // Перебираем листы
            foreach (Excel.Worksheet sheet in excelWB.Sheets)
            {
                if (sheet.Name == "QFD")
                {
                    // Определяем диапазон используемых ячеек на листе
                    Excel.Range usedRange = sheet.UsedRange;
                    //Задаем колчество строк в массиве

                    // Перебираем строки в диапазоне
                    for (int row = 2; row <= usedRange.Rows.Count; row++)
                    {
                        object[] devicesForBD = new object[10];
                        int i = 0;
                        // Перебираем ячейки в строке
                        for (int col = 1; col <= usedRange.Columns.Count; col++)
                        {
                            Excel.Range cell = usedRange.Cells[row, col] as Excel.Range;
                            string cellValue = cell.Value?.ToString() ?? "";
                            devicesForBD[i] = cellValue;
                            i++;
                        }
                        DevicesQFDForBD.Add(devicesForBD);
                        //Console.WriteLine();  // Переход на новую строку
                    }
                }
                if (sheet.Name == "QF")
                {
                    // Определяем диапазон используемых ячеек на листе
                    Excel.Range usedRange = sheet.UsedRange;
                    //Задаем колчество строк в массиве

                    // Перебираем строки в диапазоне
                    for (int row = 2; row <= usedRange.Rows.Count; row++)
                    {
                        object[] devicesForBD =  new object[10];
                        int i = 0;
                        // Перебираем ячейки в строке
                        for (int col = 1; col <= usedRange.Columns.Count; col++)
                        {
                            Excel.Range cell = usedRange.Cells[row, col] as Excel.Range;
                            string cellValue = cell.Value?.ToString() ?? "";
                            devicesForBD[i] = cellValue;
                            i++;
                        }
                        DevicesQFForBD.Add(devicesForBD);
                        //Console.WriteLine();  // Переход на новую строку
                    }
                }
      

                // Здесь можно добавить код для обработки данных листа
            }
            // Закрываем файл и приложение
            excelWB.Close(false);
            excel.Quit();

            // Освобождаем ресурсы
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excelWB);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);

            excelWB = null;
            excel = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            return (DevicesQFForBD, DevicesQFDForBD);
        }



    }
}