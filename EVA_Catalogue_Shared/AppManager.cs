using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;


namespace EVA_Catalogue_Shared
{
    public static class AppManager
    {
        public static Excel.Application ExcelApp { get; private set; }

        public static void Initialize(Excel.Application excelApp)
        {
            if (ExcelApp == null) // Чтобы не перезаписывать
            {
                ExcelApp = excelApp;
            }
        }
    }
}
