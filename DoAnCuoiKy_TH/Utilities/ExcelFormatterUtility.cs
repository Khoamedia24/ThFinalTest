using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DoAnCuoiKy_TH.Utilities
{
    /// <summary>
    /// Utility class to format Excel sheets, specifically for merging cells and expanding test steps
    /// </summary>
    public class ExcelFormatterUtility
    {
        public static void FormatTcKhoaaSheet()
        {
            const string excelFilePath = "Nhom4.xlsx";
            
            if (!File.Exists(excelFilePath))
            {
                Console.WriteLine($"File not found: {excelFilePath}");
                return;
            }

            using (var package = new ExcelPackage(new FileInfo(excelFilePath)))
            {
                // In sheet names
                Console.WriteLine("=== AVAILABLE SHEETS ===");
                foreach (var sheet in package.Workbook.Worksheets)
                {
                    Console.WriteLine($"- {sheet.Name}");
                }

                // Xem No 1 sheet
                var no1Sheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name == "No 1");
                if (no1Sheet != null)
                {
                    Console.WriteLine("\n=== No 1 SHEET (10 first rows) ===");
                    PrintSheetStructure(no1Sheet, 10);
                }

                // Xem Tc_khoaa sheet
                var tcKhoaaSheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name == "Tc_khoaa");
                if (tcKhoaaSheet != null)
                {
                    Console.WriteLine("\n=== Tc_khoaa SHEET (10 first rows) ===");
                    PrintSheetStructure(tcKhoaaSheet, 10);

                    // Format Tc_khoaa sheet
                    Console.WriteLine("\n=== FORMATTING Tc_khoaa ===");
                    ExpandTestStepsInSheet(tcKhoaaSheet);
                    
                    // Save file
                    package.Save();
                    Console.WriteLine("✓ File saved successfully!");
                }
            }
        }

        private static void PrintSheetStructure(ExcelWorksheet sheet, int maxRows)
        {
            for (int row = 1; row <= Math.Min(maxRows, sheet.Dimension?.Rows ?? 0); row++)
            {
                var rowData = new List<string>();
                for (int col = 1; col <= 14; col++)
                {
                    var cell = sheet.Cells[row, col];
                    var isMerged = sheet.MergedCells.Any(m => m.Contains(cell.Address));
                    var value = cell.Value?.ToString() ?? "";
                    if (value.Length > 15)
                        value = value.Substring(0, 12) + "...";
                    
                    rowData.Add(isMerged ? $"[M:{value}]" : $"[{value}]");
                }
                Console.WriteLine($"Row {row:D2}: {string.Join(" ", rowData)}");
            }
        }

        private static void ExpandTestStepsInSheet(ExcelWorksheet sheet)
        {
            // Assume: Row 3 is header, data starts from row 4
            const int headerRow = 3;
            const int dataStartRow = 4;
            
            // Find column indices
            int testCaseIdCol = FindColumnIndex(sheet, headerRow, "Test Case ID");
            int functionCol = FindColumnIndex(sheet, headerRow, "Function");
            int bigItemCol = FindColumnIndex(sheet, headerRow, "Big Item");
            int mediumItemCol = FindColumnIndex(sheet, headerRow, "Medium Item");
            int smallItemCol = FindColumnIndex(sheet, headerRow, "Small Item");
            int preConditionCol = FindColumnIndex(sheet, headerRow, "Pre-Condition");
            int stepNumberCol = FindColumnIndex(sheet, headerRow, "Step Number");
            int stepActionCol = FindColumnIndex(sheet, headerRow, "Step Action");
            int testDataCol = FindColumnIndex(sheet, headerRow, "Test Data");
            int expectedResultCol = FindColumnIndex(sheet, headerRow, "Expected Result");
            int actualResultCol = FindColumnIndex(sheet, headerRow, "Actual Result");
            int resultStatusCol = FindColumnIndex(sheet, headerRow, "Result");
            int notesCol = FindColumnIndex(sheet, headerRow, "Notes");

            Console.WriteLine($"Found columns: TC={testCaseIdCol}, Func={functionCol}, Step={stepNumberCol}");

            var newRows = new List<Dictionary<int, object>>();
            int currentRow = dataStartRow;
            int maxRow = sheet.Dimension?.Rows ?? 0;

            while (currentRow <= maxRow)
            {
                var tcId = sheet.Cells[currentRow, testCaseIdCol].Value?.ToString()?.Trim();
                if (string.IsNullOrEmpty(tcId))
                {
                    currentRow++;
                    continue;
                }

                // Get values that should be merged
                var function = sheet.Cells[currentRow, functionCol].Value?.ToString()?.Trim();
                var bigItem = sheet.Cells[currentRow, bigItemCol].Value?.ToString()?.Trim();
                var mediumItem = sheet.Cells[currentRow, mediumItemCol].Value?.ToString()?.Trim();
                var smallItem = sheet.Cells[currentRow, smallItemCol].Value?.ToString()?.Trim();
                var preCondition = sheet.Cells[currentRow, preConditionCol].Value?.ToString()?.Trim();

                // Get test steps (may be multiple steps separated by line break or already expanded)
                var stepNumbers = sheet.Cells[currentRow, stepNumberCol].Value?.ToString()?.Trim() ?? "";
                var stepActions = sheet.Cells[currentRow, stepActionCol].Value?.ToString()?.Trim() ?? "";
                var testDataValues = sheet.Cells[currentRow, testDataCol].Value?.ToString()?.Trim() ?? "";
                var expectedResults = sheet.Cells[currentRow, expectedResultCol].Value?.ToString()?.Trim() ?? "";

                // Split by line breaks to get individual steps
                var stepNumList = SplitByLineBreak(stepNumbers);
                var stepActionList = SplitByLineBreak(stepActions);
                var testDataList = SplitByLineBreak(testDataValues);
                var expectedResultList = SplitByLineBreak(expectedResults);

                // Ensure all lists have same length
                int maxSteps = Math.Max(stepNumList.Count, Math.Max(stepActionList.Count, Math.Max(testDataList.Count, expectedResultList.Count)));
                PadLists(new[] { stepNumList, stepActionList, testDataList, expectedResultList }, maxSteps);

                Console.WriteLine($"  Test Case {tcId}: {maxSteps} steps found");

                // Create rows for each step
                for (int i = 0; i < maxSteps; i++)
                {
                    var newRow = new Dictionary<int, object>
                    {
                        { testCaseIdCol, (object)(i == 0 ? tcId : "") },  // Only first row
                        { functionCol, (object)(i == 0 ? function : "") },
                        { bigItemCol, (object)(i == 0 ? bigItem : "") },
                        { mediumItemCol, (object)(i == 0 ? mediumItem : "") },
                        { smallItemCol, (object)(i == 0 ? smallItem : "") },
                        { preConditionCol, (object)(i == 0 ? preCondition : "") },
                        { stepNumberCol, (object)(i < stepNumList.Count ? stepNumList[i] : "") },
                        { stepActionCol, (object)(i < stepActionList.Count ? stepActionList[i] : "") },
                        { testDataCol, (object)(i < testDataList.Count ? testDataList[i] : "") },
                        { expectedResultCol, (object)(i < expectedResultList.Count ? expectedResultList[i] : "") },
                        { actualResultCol, (object)"" },  // Keep empty for now
                        { resultStatusCol, (object)"" },  // Keep empty for now
                        { notesCol, (object)"" }  // Keep empty for now
                    };
                    newRows.Add(newRow);
                }

                currentRow++;
            }

            // If we found new rows to write, clear old data and write new data
            if (newRows.Count > 0)
            {
                Console.WriteLine($"Total rows after expansion: {newRows.Count}");
                
                // Clear old data rows (keep header)
                if (sheet.Dimension != null)
                {
                    sheet.DeleteRow(dataStartRow, sheet.Dimension.Rows - dataStartRow + 1);
                }

                // Write new rows
                for (int i = 0; i < newRows.Count; i++)
                {
                    int rowNum = dataStartRow + i;
                    foreach (var kvp in newRows[i])
                    {
                        sheet.Cells[rowNum, kvp.Key].Value = kvp.Value;
                    }
                }

                // Apply merge cells (merge vertically for fields that are same for multiple steps)
                MergeCellsForTestCases(sheet, dataStartRow, dataStartRow + newRows.Count - 1, testCaseIdCol, functionCol, bigItemCol, mediumItemCol, smallItemCol, preConditionCol);
            }
        }

        private static int FindColumnIndex(ExcelWorksheet sheet, int headerRow, string columnName)
        {
            for (int col = 1; col <= 14; col++)
            {
                var cellValue = sheet.Cells[headerRow, col].Value?.ToString()?.Trim();
                if (cellValue?.Equals(columnName, StringComparison.OrdinalIgnoreCase) ?? false)
                    return col;
            }
            return -1;
        }

        private static List<string> SplitByLineBreak(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>();

            return text.Split(new[] { "\n", "\r\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => s.Trim())
                      .Where(s => !string.IsNullOrEmpty(s))
                      .ToList();
        }

        private static void PadLists(List<string>[] lists, int targetLength)
        {
            foreach (var list in lists)
            {
                while (list.Count < targetLength)
                    list.Add("");
            }
        }

        private static void MergeCellsForTestCases(ExcelWorksheet sheet, int startRow, int endRow, params int[] colsToMerge)
        {
            int currentRow = startRow;
            
            while (currentRow <= endRow)
            {
                var testCaseId = sheet.Cells[currentRow, colsToMerge[0]].Value?.ToString()?.Trim();
                if (string.IsNullOrEmpty(testCaseId))
                {
                    currentRow++;
                    continue;
                }

                // Find how many rows have the same test case ID
                int mergeEndRow = currentRow;
                while (mergeEndRow < endRow && 
                       sheet.Cells[mergeEndRow + 1, colsToMerge[0]].Value?.ToString()?.Trim() == "")
                {
                    mergeEndRow++;
                }

                // Merge cells for columns that should be merged
                if (mergeEndRow > currentRow)
                {
                    foreach (int col in colsToMerge)
                    {
                        try
                        {
                            sheet.Cells[currentRow, col, mergeEndRow, col].Merge = true;
                        }
                        catch { /* ignore merge errors */ }
                    }
                }

                currentRow = mergeEndRow + 1;
            }
        }
    }
}
