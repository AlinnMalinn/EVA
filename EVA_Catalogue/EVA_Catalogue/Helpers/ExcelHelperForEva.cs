using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;
using Color = System.Drawing.Color;
using Excel = Microsoft.Office.Interop.Excel;
using EVA_Catalogue_Shared;
using System.Reflection;



namespace EVA_Catalogue
{
    public class ExcelHelperForEva
    {
        Excel.Workbook excelWB = AppManager.ExcelApp.ActiveWorkbook;

        public List<string> GetSheetsStartingWithEVA()
        {

            List<string> sheetsStartingWithEVA = new List<string>();
            //MessageBox.Show("Всё есть! ", excelWB.Name);

            try
            {

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

        private float MaximumBreakingCapacity(float currentMaximumBreakingCapacityFromExcel, float threePhaseShortCircuitCurrentOnPanel, float singlePhaseShortCircuitCurrentOnPanel, int NumberOfPolesFromExcel)
        {
            float maximumBreakingCapacity = 0;
            if (currentMaximumBreakingCapacityFromExcel == 0)
            {
                if (NumberOfPolesFromExcel >= 3)
                {
                    maximumBreakingCapacity = threePhaseShortCircuitCurrentOnPanel;
                }
                else
                {
                    maximumBreakingCapacity = singlePhaseShortCircuitCurrentOnPanel;
                }
            }
            else
            {
                maximumBreakingCapacity = currentMaximumBreakingCapacityFromExcel;
            }
            return maximumBreakingCapacity;
        }

        public object[][] GetListDeviceFromExcel(string deviceType, string nameSheet)
        {
            Excel.Workbook excelWB = AppManager.ExcelApp.ActiveWorkbook;
            Excel.Worksheet excelWS = (Excel.Worksheet)excelWB.Sheets.get_Item(nameSheet);

            int startCol = 3;
            int col = startCol;
            int counter = 0;

            // Ищем количество устройств до символа "#"
            while ((excelWS.Cells[1, col].Value2?.ToString() ?? "") != "#")
            {
                counter++;
                col++;
            }

            object[][] Devices = new object[counter][];

            if (deviceType != MainViewModel.ModularCircuitBreakersSettings)
                return Devices;

            int endCol = startCol + counter - 1;

            // Считываем все нужные строки одним диапазоном: строки 1-275, столбцы 3-endCol
            Excel.Range range = excelWS.Range[
                excelWS.Cells[1, startCol],
                excelWS.Cells[275, endCol]
            ];

            object[,] data = range.Value2 as object[,];

            float threePhaseShortCircuitCurrentOnPanel = ToFloat(GetValue(data, 150, 3, startCol));
            float singlePhaseShortCircuitCurrentOnPanel = ToFloat(GetValue(data, 151, 3, startCol));

            for (int i = 0; i < counter; i++)
            {
                int excelCol = startCol + i;

                object typeValue = GetValue(data, 15, excelCol, startCol);

                if (typeValue == null)
                {
                    Devices[i] = new object[1];
                    Devices[i][EquipmentSelection.PositionInArray_typeOfDevice] = "0";
                    continue;
                }

                string currentTypeOfDevice1FromExcel = typeValue.ToString();

                float currentRatedCurrentOfMouldedCaseFromExcel =
                    ToFloat(GetValue(data, 20, excelCol, startCol));

                float currentRatedCurrentFromExcel =
                    ToFloat(GetValue(data, 17, excelCol, startCol));

                string currentResponseCharacteristicsFromExcel =
                    GetValue(data, 18, excelCol, startCol)?.ToString() ?? string.Empty;

                object NumberOfPolesFromExcel =
                    GetValue(data, 11, excelCol, startCol);

                int currentNumberOfPolesFromExcel =
                    ToInt(NumberOfPolesFromExcel);

                string leakageRaw =
                    GetValue(data, 22, excelCol, startCol)?.ToString() ?? string.Empty;

                string[] leakageCurrentAndResidualCurrentType = leakageRaw.Split(',');

                float currentLeakageCurrentFromExcel = 0;

                if (leakageCurrentAndResidualCurrentType.Length > 0)
                {
                    string leakageText = leakageCurrentAndResidualCurrentType[0].Trim();

                    if (leakageText.Length > 2)
                    {
                        leakageText = leakageText.Substring(0, leakageText.Length - 2);
                        currentLeakageCurrentFromExcel = ToFloat(leakageText) / 1000;
                    }
                }

                float currentMaximumBreakingCapacityFromExcel =
                    ToFloat(GetValue(data, 19, excelCol, startCol));

                string residualCurrentTypeFromExcel = string.Empty;

                if (leakageCurrentAndResidualCurrentType.Length > 1 &&
                    !string.IsNullOrEmpty(leakageCurrentAndResidualCurrentType[1]) &&
                    leakageCurrentAndResidualCurrentType[1].Length > 1)
                {
                    residualCurrentTypeFromExcel = leakageCurrentAndResidualCurrentType[1].Trim();
                }

                if (currentTypeOfDevice1FromExcel.Contains("QF"))
                {
                    Devices[i] = new object[8];

                    if (currentResponseCharacteristicsFromExcel == "МА" &&
                        currentRatedCurrentOfMouldedCaseFromExcel == 0)
                    {
                        Devices[i][EquipmentSelection.PositionInArray_deviceInfo] =
                            currentTypeOfDevice1FromExcel +
                            NumberOfPolesFromExcel +
                            currentRatedCurrentFromExcel +
                            currentResponseCharacteristicsFromExcel;

                        Devices[i][EquipmentSelection.PositionInArray_typeOfDevice] =
                            "Модульный автоматический выключатель без теплового расцепителя";

                        Devices[i][EquipmentSelection.PositionInArray_thermalOverloadRelease] = 0;
                    }
                    else if (currentRatedCurrentOfMouldedCaseFromExcel == 0)
                    {
                        Devices[i][EquipmentSelection.PositionInArray_deviceInfo] =
                            currentTypeOfDevice1FromExcel +
                            NumberOfPolesFromExcel +
                            currentRatedCurrentFromExcel +
                            currentResponseCharacteristicsFromExcel;

                        Devices[i][EquipmentSelection.PositionInArray_typeOfDevice] =
                            "Модульный автоматический выключатель";

                        Devices[i][EquipmentSelection.PositionInArray_thermalOverloadRelease] = 1;
                    }
                    else
                    {
                        Devices[i] = new object[1];
                        Devices[i][EquipmentSelection.PositionInArray_typeOfDevice] = "0";
                        continue;
                    }

                    Devices[i][EquipmentSelection.PositionInArray_ratedCurrent] =
                        currentRatedCurrentFromExcel;

                    Devices[i][EquipmentSelection.PositionInArray_numberOfPoles] =
                        currentNumberOfPolesFromExcel;

                    Devices[i][EquipmentSelection.PositionInArray_maximumBreakingCapacity] =
                        MaximumBreakingCapacity(
                            currentMaximumBreakingCapacityFromExcel,
                            threePhaseShortCircuitCurrentOnPanel,
                            singlePhaseShortCircuitCurrentOnPanel,
                            currentNumberOfPolesFromExcel);

                    Devices[i][EquipmentSelection.PositionInArray_responseCharacteristics] =
                        currentResponseCharacteristicsFromExcel;

                    Devices[i][EquipmentSelection.PositionInArray_additionalDevice11] =
                        "В будущем тут будет указание о наличии второго устройства";
                }

                if (currentTypeOfDevice1FromExcel.Contains("QFD"))
                {
                    Devices[i] = new object[11];

                    Devices[i][EquipmentSelection.PositionInArray_deviceInfo] =
                        currentTypeOfDevice1FromExcel +
                        NumberOfPolesFromExcel +
                        currentRatedCurrentFromExcel +
                        currentResponseCharacteristicsFromExcel +
                        leakageRaw;

                    Devices[i][EquipmentSelection.PositionInArray_typeOfDevice] =
                        "Автоматический выключатель дифференциального тока";

                    Devices[i][EquipmentSelection.PositionInArray_ratedCurrent] =
                        currentRatedCurrentFromExcel;

                    Devices[i][EquipmentSelection.PositionInArray_numberOfPoles] =
                        currentNumberOfPolesFromExcel + 1;

                    Devices[i][EquipmentSelection.PositionInArray_maximumBreakingCapacity] =
                        MaximumBreakingCapacity(
                            currentMaximumBreakingCapacityFromExcel,
                            threePhaseShortCircuitCurrentOnPanel,
                            singlePhaseShortCircuitCurrentOnPanel,
                            currentNumberOfPolesFromExcel);

                    Devices[i][EquipmentSelection.PositionInArray_responseCharacteristics] =
                        currentResponseCharacteristicsFromExcel;

                    Devices[i][EquipmentSelection.PositionInArray_thermalOverloadRelease] = 1;

                    Devices[i][EquipmentSelection.PositionInArray_leakageCurrent] =
                        currentLeakageCurrentFromExcel;

                    Devices[i][EquipmentSelection.PositionInArray_residualCurrentType] =
                        residualCurrentTypeFromExcel;
                }
            }

            return Devices;
        }
        private object GetValue(object[,] data, int excelRow, int excelCol, int startCol)
        {
            int arrayRow = excelRow;
            int arrayCol = excelCol - startCol + 1;

            return data[arrayRow, arrayCol];
        }

        private float ToFloat(object value)
        {
            if (value == null)
                return 0;

            string text = value.ToString().Replace(',', '.');

            if (float.TryParse(
                text,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out float result))
            {
                return result;
            }

            return 0;
        }

        private int ToInt(object value)
        {
            if (value == null)
                return 0;

            if (int.TryParse(value.ToString(), out int result))
                return result;

            return 0;
        }

        public void WhriteDevice1DataToExcel(
    List<string> deviceInfoList,
    List<string> produserNameList,
    List<string> codeOfDeviceList,
    List<string> nameOfDeviceList,
    List<string> markOfDeviceList,
    string nameSheet,
    List<string> maximumBreakingCapacity,
    bool isQFEnabled,
    bool isQFDEnabled)
        {
            Excel.Application excelApp = AppManager.ExcelApp;
            Excel.Workbook excelWB = excelApp.ActiveWorkbook;
            Excel.Worksheet excelWS = (Excel.Worksheet)excelWB.Sheets.get_Item(nameSheet);

            bool oldScreenUpdating = excelApp.ScreenUpdating;
            bool oldEnableEvents = excelApp.EnableEvents;
            Excel.XlCalculation oldCalculation = excelApp.Calculation;

            try
            {
                excelApp.ScreenUpdating = false;
                excelApp.EnableEvents = false;
                excelApp.Calculation = Excel.XlCalculation.xlCalculationManual;

                int count = codeOfDeviceList.Count;
                int startCol = 3;
                int endCol = startCol + count - 1;

                object[,] row16 = GetRowValues(excelWS, 16, startCol, endCol);
                object[,] row19 = GetRowValues(excelWS, 19, startCol, endCol);
                object[,] row162 = GetRowValues(excelWS, 162, startCol, endCol);
                object[,] row164 = GetRowValues(excelWS, 164, startCol, endCol);
                object[,] row165 = GetRowValues(excelWS, 165, startCol, endCol);
                object[,] row275 = GetRowValues(excelWS, 275, startCol, endCol);

                for (int i = 0; i < count; i++)
                {
                    int col = startCol + i;

                    Excel.Range typeCell = excelWS.Cells[15, col];

                    if (IsTextRed(typeCell) || IsTextGreen(typeCell))
                        continue;

                    string deviceType = typeCell.Value?.ToString();

                    bool canWrite =
                        (deviceType == "QF" && isQFEnabled) ||
                        (deviceType == "QFD" && isQFDEnabled);

                    if (!canWrite)
                        continue;

                    int arrayCol = i + 1;

                    if (nameOfDeviceList[i] != " ")
                    {
                        row165[1, arrayCol] = produserNameList[i];
                        row164[1, arrayCol] = codeOfDeviceList[i];
                        row162[1, arrayCol] = nameOfDeviceList[i];
                        row16[1, arrayCol] = markOfDeviceList[i];
                        row19[1, arrayCol] = maximumBreakingCapacity[i];
                        row275[1, arrayCol] = deviceInfoList[i];
                    }
                    else
                    {
                        row162[1, arrayCol] = nameOfDeviceList[i];
                        row164[1, arrayCol] = " ";
                        row165[1, arrayCol] = " ";
                        row16[1, arrayCol] = " ";
                        row275[1, arrayCol] = "";
                    }
                }

                SetRowValues(excelWS, 16, startCol, endCol, row16);
                SetRowValues(excelWS, 19, startCol, endCol, row19);
                SetRowValues(excelWS, 162, startCol, endCol, row162);
                SetRowValues(excelWS, 164, startCol, endCol, row164);
                SetRowValues(excelWS, 165, startCol, endCol, row165);
                SetRowValues(excelWS, 275, startCol, endCol, row275);

                excelWB.Save();
            }
            finally
            {
                excelApp.ScreenUpdating = oldScreenUpdating;
                excelApp.EnableEvents = oldEnableEvents;
                excelApp.Calculation = oldCalculation;
            }
        }

        private object[,] GetRowValues(
            Excel.Worksheet sheet,
            int row,
            int startCol,
            int endCol)
        {
            Excel.Range range = sheet.Range[
                sheet.Cells[row, startCol],
                sheet.Cells[row, endCol]
            ];

            object value = range.Value2;

            if (value is object[,] values)
                return values;

            object[,] singleValue = new object[1, 1];
            singleValue[1, 1] = value;
            return singleValue;
        }

        private void SetRowValues(
            Excel.Worksheet sheet,
            int row,
            int startCol,
            int endCol,
            object[,] values)
        {
            Excel.Range range = sheet.Range[
                sheet.Cells[row, startCol],
                sheet.Cells[row, endCol]
            ];

            range.Value2 = values;
        }
        static bool IsTextRed(Excel.Range cell)
        {
            if (cell.Font.Color == null)
                return false; // Если цвет не задан, считаем, что он не красный

            int colorValue = Convert.ToInt32(cell.Font.Color); // Конвертируем OLE_COLOR
            Color fontColor = Color.FromArgb(colorValue); // Преобразуем в ARGB

            // Поскольку в Excel цвет в формате BGR, корректируем порядок:
            Color correctedColor = Color.FromArgb(fontColor.B, fontColor.G, fontColor.R);

            // Проверяем, что цвет содержит много красного и мало других оттенков
            return correctedColor.R == 192;
        }
        static bool IsTextGreen(Excel.Range cell)
        {
            if (cell.Font.Color == null)
                return false; // Если цвет не задан, считаем, что он не зеленый

            int colorValue = Convert.ToInt32(cell.Font.Color); // Конвертируем OLE_COLOR
            Color fontColor = Color.FromArgb(colorValue); // Преобразуем в ARGB

            // Поскольку в Excel цвет в формате BGR, корректируем порядок:
            Color correctedColor = Color.FromArgb(fontColor.B, fontColor.G, fontColor.R);

            // Проверяем, что цвет содержит много зеленого и мало других оттенков
            return correctedColor.G == 128;
        }


        //public void QuitExcel()
        //{
        //    excel.Quit();
        //}

        public void CleanCatalogue()
        {

            List<string> listOfSheets = GetSheetsStartingWithEVA();
            foreach (string sheet in listOfSheets)
            {
                Excel.Workbook excelWB = AppManager.ExcelApp.ActiveWorkbook;
                Excel.Worksheet excelWS;
                excelWS = (Excel.Worksheet)excelWB.Sheets.get_Item(sheet);
                int i = 3;
                int counter = 0;
                while (excelWS.Cells[2, i].Value != null)
                {
                    ++counter;
                    ++i;
                }
                for (int j = 0; j < 0 + counter + 0; j++)
                {
                    if (IsTextRed(excelWS.Cells[15, j + 3]) == false)
                    {
                        if (IsTextGreen(excelWS.Cells[15, j + 3]) == true)
                        {
                            excelWS.Cells[15, j + 3].Font.Color = 0;
                        }
                        excelWS.Cells[162, 3 + j].Value = "";
                        excelWS.Cells[164, 3 + j].Value = "";
                        excelWS.Cells[165, 3 + j].Value = "";
                        excelWS.Cells[16, 3 + j].Value = "";
                        excelWS.Cells[275, 3 + j].Value = "";
                    }
                }
            }
            excelWB.Save();
        }
        public void CleanGreen()
        {

                Excel.Range selection = AppManager.ExcelApp.Selection;

                foreach (Excel.Range col in selection.Columns)
                {
          
                    int colIndex = col.Column;
                    var cell = AppManager.ExcelApp.ActiveSheet.Cells[15, colIndex];
                if (IsTextGreen(cell) == true)
                {
                    cell.Font.Color = 0;
                }
                
                }
            
            excelWB.Save();
        }
    }
}