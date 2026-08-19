using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;

namespace EVA_Catalogue
{
    class ExcelUploadDB
    {
        
        string path;
        Excel.Workbook excelWB;
        Excel.Worksheet excelWS;
        Excel.Application excel = new Excel.Application();
        public ExcelUploadDB()
        {
            // Создаем новое приложение Excel
            //Excel.Application excel = new Excel.Application();
            // Создаем новую книгу
            //Excel.Workbook excelWB = new Excel.Workbook();

            excelWB = excel.Workbooks.Add();
            
        }
        public void CreateSheet(DataTable dataTable, string sheetName)
        {
            try
            {
                // Добавляем новый лист
                Excel.Worksheet excelWS = (Excel.Worksheet)excelWB.Worksheets.Add();
                excelWS.Name = sheetName;

                // Заполняем заголовки столбцов
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    excelWS.Cells[1, i + 1] = dataTable.Columns[i].ColumnName;
                }

                // Заполняем данные
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        excelWS.Cells[i + 2, j + 1] = dataTable.Rows[i][j];
                    }
                }

                // Освобождаем ресурс COM-объекта для рабочего листа
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelWS);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
        public void SaveAs(string filePath)
        {
            try
            {
                excelWB.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка при сохранении файла: " + ex.Message);
            }
            finally
            {
                // Всегда завершаем работу с Excel безопасно
                try
                {
                    if (excelWB != null)
                    {
                        try
                        {
                            excelWB.Close(false);
                        }
                        catch (Exception exClose)
                        {
                            System.Diagnostics.Debug.WriteLine("Ошибка при закрытии книги: " + exClose.Message);
                        }

                        Marshal.ReleaseComObject(excelWB);
                        excelWB = null;
                    }

                    if (excel != null)
                    {
                        try
                        {
                            excel.Quit();
                        }
                        catch (Exception exQuit)
                        {
                            System.Diagnostics.Debug.WriteLine("Ошибка при завершении Excel: " + exQuit.Message);
                        }

                        Marshal.ReleaseComObject(excel);
                        excel = null;
                    }
                }
                finally
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
            }
        }
    }
}
