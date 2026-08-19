using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Color = System.Drawing.Color;
using EVA_Catalogue_Shared;

//using Color = System.Drawing.Color;
using Excel = Microsoft.Office.Interop.Excel;

namespace EVA_CatalogueManual
{
    class ExcelHelperForEva
    {
        Excel.Workbook excelWB = AppManager.ExcelApp.ActiveWorkbook;

        public string GetCellValue()
        {
            
            Excel.Worksheet activeSheet = AppManager.ExcelApp.ActiveSheet;

            // Получаем активную (выделенную) ячейку
            Excel.Range activeCell = AppManager.ExcelApp.ActiveCell;

            // Получаем значение
            object value = activeCell.Value;

            string textForTextBlock;

            // Преобразуем в строку, если нужно
            string cellValue = value?.ToString() ?? "";
            if (cellValue == "QF")
            {
                textForTextBlock = MainViewModel.ModularCircuitBreakers;
            }
            else if (cellValue == "QFD")
            {
                textForTextBlock = MainViewModel.ModularResidualCurrentCircuitBreakers;
            }
            else
            {
                textForTextBlock = "Выберете оборудование";
            }
                return textForTextBlock;
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
        public object[] GetListDeviceFromExcel()
        {
            Excel.Workbook excelWB = AppManager.ExcelApp.ActiveWorkbook;
            Excel.Worksheet excelWS = AppManager.ExcelApp.ActiveSheet;

            // Получаем активную (выделенную) ячейку
            Excel.Range activeCell = AppManager.ExcelApp.ActiveCell;
            int columnNumber = activeCell.Column;
            object value = activeCell.Value;
            // Преобразуем в строку, если нужно
            string cellValue = value?.ToString() ?? "";
            object[]Devices = new object[10];
            string currentTypeOfDevice1FromExcel = excelWS.Cells[15, columnNumber].Value.ToString();
            float currentRatedCurrentOfMouldedCaseFromExcel = float.Parse((excelWS.Cells[20, columnNumber].Value ?? 0).ToString());
            float currentRatedCurrentFromExcel = float.Parse((excelWS.Cells[17, columnNumber].Value ?? 0).ToString());
            string currentResponseCharacteristicsFromExcel = (excelWS.Cells[18, columnNumber].Value ?? string.Empty).ToString();
            object NumberOfPolesFromExcel = excelWS.Cells[12, columnNumber][0].Value;
            int currentNumberOfPolesFromExcel = int.Parse((excelWS.Cells[12,columnNumber][0].Value ?? 0).ToString());
            string[] leakageCurrentAndResidualCurrentType = (excelWS.Cells[22, columnNumber].Value).Split(',');
            float currentLeakageCurrentFromExcel = float.Parse((leakageCurrentAndResidualCurrentType[0].Substring(0, (leakageCurrentAndResidualCurrentType[0]).Length - 2)).ToString()) / 1000;
                        //float currentLeakageCurrentFromExcel = float.Parse((excelWS.Cells[22, j].Value.Substring(0, excelWS.Cells[22, j].Value.Length - 2) ?? 0).ToString()) / 1000;
            float currentMaximumBreakingCapacityFromExcel = float.Parse((excelWS.Cells[19, columnNumber].Value ?? 0).ToString()); //.Replace('.', ',')
            float threePhaseShortCircuitCurrentOnPanel = float.Parse((excelWS.Cells[150, 3].Value ?? 0).ToString());
            float singlePhaseShortCircuitCurrentOnPanel = float.Parse((excelWS.Cells[151, 3].Value ?? 0).ToString());

            string residualCurrentTypeFromExcel = string.Empty;

            if (leakageCurrentAndResidualCurrentType != null
               && leakageCurrentAndResidualCurrentType.Length > 1
               && !string.IsNullOrEmpty(leakageCurrentAndResidualCurrentType[1])
               && leakageCurrentAndResidualCurrentType[1].Length > 1)
               {
                  residualCurrentTypeFromExcel = leakageCurrentAndResidualCurrentType[1].Substring(1);
               }
                        if (cellValue == "QF")
                        {
                            if (currentResponseCharacteristicsFromExcel == "МА" && currentRatedCurrentOfMouldedCaseFromExcel == 0)
                            {
                    Devices[EquipmentSelection.PositionInArray_deviceInfo] = currentTypeOfDevice1FromExcel + NumberOfPolesFromExcel + currentRatedCurrentFromExcel + currentResponseCharacteristicsFromExcel;
                    Devices[EquipmentSelection.PositionInArray_typeOfDevice] = "Модульный автоматический выключатель без теплового расцепителя";
                    Devices[EquipmentSelection.PositionInArray_thermalOverloadRelease] = 0;
                    Devices[EquipmentSelection.PositionInArray_ratedCurrent] = currentRatedCurrentFromExcel;
                    Devices[EquipmentSelection.PositionInArray_numberOfPoles] = currentNumberOfPolesFromExcel;
                    Devices[EquipmentSelection.PositionInArray_maximumBreakingCapacity] = MaximumBreakingCapacity(currentMaximumBreakingCapacityFromExcel, threePhaseShortCircuitCurrentOnPanel, singlePhaseShortCircuitCurrentOnPanel, currentNumberOfPolesFromExcel);
                    Devices[EquipmentSelection.PositionInArray_responseCharacteristics] = currentResponseCharacteristicsFromExcel;
                    Devices[EquipmentSelection.PositionInArray_additionalDevice11] = "В будущем тут будет указание о наличии второго устройства";
                }
                            else if (currentRatedCurrentOfMouldedCaseFromExcel == 0)
                            {
                    Devices[EquipmentSelection.PositionInArray_deviceInfo] = currentTypeOfDevice1FromExcel + NumberOfPolesFromExcel + currentRatedCurrentFromExcel + currentResponseCharacteristicsFromExcel;
                    Devices[EquipmentSelection.PositionInArray_typeOfDevice] = "Модульный автоматический выключатель";
                    Devices[EquipmentSelection.PositionInArray_thermalOverloadRelease] = 1;

                    Devices[EquipmentSelection.PositionInArray_ratedCurrent] = currentRatedCurrentFromExcel;
                    Devices[EquipmentSelection.PositionInArray_numberOfPoles] = currentNumberOfPolesFromExcel;
                    Devices[EquipmentSelection.PositionInArray_maximumBreakingCapacity] = MaximumBreakingCapacity(currentMaximumBreakingCapacityFromExcel, threePhaseShortCircuitCurrentOnPanel, singlePhaseShortCircuitCurrentOnPanel, currentNumberOfPolesFromExcel);
                    Devices[EquipmentSelection.PositionInArray_responseCharacteristics] = currentResponseCharacteristicsFromExcel;
                    Devices[EquipmentSelection.PositionInArray_additionalDevice11] = "В будущем тут будет указание о наличии второго устройства";
                }
                            else
                            {
                                Devices[0] = "0";
                            }
                        }
                        else if (cellValue == "QFD")
                        {
                Devices = new object[11];
                Devices[EquipmentSelection.PositionInArray_deviceInfo] = currentTypeOfDevice1FromExcel + NumberOfPolesFromExcel + currentRatedCurrentFromExcel + currentResponseCharacteristicsFromExcel + excelWS.Cells[22, columnNumber].Value;
                Devices[EquipmentSelection.PositionInArray_typeOfDevice] = "Автоматический выключатель дифференциального тока";
                Devices[EquipmentSelection.PositionInArray_ratedCurrent] = currentRatedCurrentFromExcel;
                Devices[EquipmentSelection.PositionInArray_numberOfPoles] = currentNumberOfPolesFromExcel + 1;
                Devices[EquipmentSelection.PositionInArray_maximumBreakingCapacity] = MaximumBreakingCapacity(currentMaximumBreakingCapacityFromExcel, threePhaseShortCircuitCurrentOnPanel, singlePhaseShortCircuitCurrentOnPanel, currentNumberOfPolesFromExcel);
                Devices[EquipmentSelection.PositionInArray_responseCharacteristics] = currentResponseCharacteristicsFromExcel;
                Devices[EquipmentSelection.PositionInArray_thermalOverloadRelease] = 1;// наличие теплового расцепителя 
                Devices[EquipmentSelection.PositionInArray_leakageCurrent] = currentLeakageCurrentFromExcel;
                Devices[EquipmentSelection.PositionInArray_residualCurrentType] = residualCurrentTypeFromExcel;
            }                  
                    else
                    {
                        Devices[0] = "0";
                    }
                   
                  return Devices;
        }
        public void WhriteDeviceDataToExcel(List<string> listOfDevices)
        {
            Excel.Worksheet activeSheet = AppManager.ExcelApp.ActiveSheet;
            // Получаем активную (выделенную) ячейку
            Excel.Range activeCell = AppManager.ExcelApp.ActiveCell;
            // Получаем значение
 
            int i = activeCell.Column;

                    if (listOfDevices != null)
            {
                activeSheet.Cells[165, i].Value = listOfDevices[4].ToString();
                activeSheet.Cells[164, i].Value = listOfDevices[1].ToString();
                activeSheet.Cells[162, i].Value = listOfDevices[0].ToString();
                activeSheet.Cells[16, i].Value = listOfDevices[2].ToString();
                activeSheet.Cells[19,  i].Value = listOfDevices[3].ToString();
                activeSheet.Cells[275, i].Value = listOfDevices[5].ToString();

                //activeCell.Font.Color = 192;
                activeCell.Font.Color = 128*256;
            }
                    else
            {
                activeSheet.Cells[162, i].Value = " ";
                activeSheet.Cells[164, i].Value = " ";
                activeSheet.Cells[165, i].Value = " ";
                activeSheet.Cells[16, i].Value = " ";
                activeSheet.Cells[275, i].Value = "";

            }
                
            
            excelWB.Save();

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


    }

}
