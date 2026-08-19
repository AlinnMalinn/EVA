using System;
using System.Collections.Generic;
using System.Windows;
using ClosedXML.Excel;

namespace EVA_Catalogue
{
    class ExcelHelperForDB
    {
        private readonly string path;

        public ExcelHelperForDB(string path)
        {
            this.path = path;
        }

        public (List<object[]>, List<object[]>) GetListOfDevicesTypeFromDB()
        {
            List<object[]> devicesQFForBD = new List<object[]>();
            List<object[]> devicesQFDForBD = new List<object[]>();

            try
            {
                using (var workbook = new XLWorkbook(path))
                {
                    foreach (IXLWorksheet sheet in workbook.Worksheets)
                    {
                        if (sheet.Name != "QF" && sheet.Name != "QFD")
                            continue;

                        var lastRow = sheet.LastRowUsed();

                        if (lastRow == null)
                            continue;

                        int lastRowNumber = lastRow.RowNumber();

                        // Начинаем со второй строки (первая - заголовки)
                        for (int row = 2; row <= lastRowNumber; row++)
                        {
                            object[] devicesForBD = new object[11];

                            for (int col = 1; col <= 11; col++)
                            {
                                devicesForBD[col - 1] = sheet.Cell(row, col).GetValue<string>();
                            }

                            if (sheet.Name == "QF")
                                devicesQFForBD.Add(devicesForBD);
                            else
                                devicesQFDForBD.Add(devicesForBD);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при чтении Excel: " + ex.Message +
                                "\n\n" + ex.StackTrace);
                throw;
            }

            return (devicesQFForBD, devicesQFDForBD);
        }
    }
}