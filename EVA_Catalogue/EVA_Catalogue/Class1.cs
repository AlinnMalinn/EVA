using System;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows;

namespace EVA_Catalogue
{
    [ComVisible(true)]
    public class CustomRibbon : Excel.IRibbonExtensibility
    {
        private Excel.Application _excelApp;
        private Excel.Workbook _workbook;
        private Excel.Worksheet _worksheet;

        public string GetCustomUI(string ribbonID)
        {
            return @"
                <customUI xmlns='http://schemas.microsoft.com/office/2006/01/customui'>
                    <ribbon>
                        <tabs>
                            <tab id='customTab' label='Подбор оборудования'>
                                <group id='customGroup' label='Выбор оборудования'>
                                    <button id='customButton' label='Выбрать оборудование'
                                        imageMso='HappyFace' size='large' onAction='OnButtonClick'/>
                                </group>
                            </tab>
                        </tabs>
                    </ribbon>
                </customUI>";
        }

        public void OnButtonClick(Excel.IRibbonControl control)
        {
            try
            {
                // Открываем WPF окно
                MainWindow mainWindow = new MainWindow();
                mainWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        public void Ribbon_Load(Excel.IRibbonUI ribbonUI)
        {
        }

        #region IRibbonExtensibility Members

        public string GetCustomUI(string RibbonID)
        {
            return GetCustomUI(RibbonID);
        }

        #endregion

        #region COM Register/Unregister Methods

        [ComRegisterFunction()]
        public static void RegisterRibbon(Type type)
        {
            Microsoft.Office.Interop.Excel.Application application = null;

            try
            {
                application = (Microsoft.Office.Interop.Excel.Application)Marshal.GetActiveObject("Excel.Application");
                string ribbonXML = GetCustomUI(type.GUID.ToString());
                application.RegisterXLL(type.Assembly.Location);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при регистрации пользовательской панели: " + ex.Message);
            }
            finally
            {
                if (application != null)
                {
                    application.Quit();
                    Marshal.ReleaseComObject(application);
                }
            }
        }

        [ComUnregisterFunction()]
        public static void UnregisterRibbon(Type type)
        {
            Microsoft.Office.Interop.Excel.Application application = null;

            try
            {
                application = (Microsoft.Office.Interop.Excel.Application)Marshal.GetActiveObject("Excel.Application");
                application.UnregisterXLL(type.Assembly.Location);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении пользовательской панели: " + ex.Message);
            }
            finally
            {
                if (application != null)
                {
                    application.Quit();
                    Marshal.ReleaseComObject(application);
                }
            }
        }

        #endregion
    }
}