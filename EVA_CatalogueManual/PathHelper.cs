using Microsoft.Office.Interop.Excel;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;
using EVA_Catalogue_Shared;

namespace EVA_CatalogueManual
{
    class PathHelper
    {

        private string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string dbFolderPath;
        private string settingsFolderPath;
    

        public string PathDBHelper()
        {
            string customFolderPath = GetLinkForDB();
            //dbFolderPath = Path.Combine(customFolderPath, "DataBase");
            return customFolderPath;

        }

        public bool CheckLinkForDB()
        {

            Excel.Workbook excelWB = AppManager.ExcelApp.ActiveWorkbook;
            Excel.Worksheet excelWS;
            excelWS = (Excel.Worksheet)excelWB.Sheets.get_Item("списки1");
            string pathToBD = excelWS.Cells[200, 1].Value;
            if (pathToBD != "" & pathToBD != null)
            {
                if (Directory.Exists(pathToBD) == true)
                    return true;
            }
            excelWS.Cells[200, 1].Value = "";

            return false;

        }
        public string GetLinkForDB()
        {

            Excel.Workbook excelWB = AppManager.ExcelApp.ActiveWorkbook;
            Excel.Worksheet excelWS;
            excelWS = (Excel.Worksheet)excelWB.Sheets.get_Item("списки1");
            dbFolderPath = excelWS.Cells[200, 1].Value;

            return dbFolderPath;

        }
        public string PathSettingsHelper()
        {
            Excel.Workbook excelWB = AppManager.ExcelApp.ActiveWorkbook;
            string customFolderPath = GetLinkForDB();
            if (customFolderPath == null)
            {
                return null;
            }
            string fileName = "Settings_" + excelWB.Name + ".txt";            
            settingsFolderPath=Path.Combine(customFolderPath, fileName);
            return settingsFolderPath;
        }

    }
}
