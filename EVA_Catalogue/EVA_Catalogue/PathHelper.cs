using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVA_Catalogue
{
    class PathHelper
    {
      
            private string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            private string dbFolderPath;
            private string settingsFolderPath;

        public string PathDBHelper()
            {             
            string customFolderPath = Path.Combine(myDocumentsPath, "EVAex3");
            dbFolderPath = Path.Combine(customFolderPath, "DataBase");
            return dbFolderPath;
            }
        public string PathSettingsHelper()
        {
            string customFolderPath = Path.Combine(myDocumentsPath, "EVAex3");
            settingsFolderPath = Path.Combine(customFolderPath, "Settings.txt");
            return settingsFolderPath;
        }

    }
}
