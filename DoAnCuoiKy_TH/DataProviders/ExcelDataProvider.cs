using DoAnCuoiKy.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Text.RegularExpressions;
using TestCaseModel = DoAnCuoiKy.Models.TestCaseData;
using TestStepModel = DoAnCuoiKy.Models.TestStep;

namespace DoAnCuoiKy_TH.DataProviders
{
    public class ExcelDataProvider
    {
        private const string ExcelFilePathEnvironmentVariable = "BDCLPM_EXCEL_PATH";
        private const string DefaultExcelFileName = "TC_Khoa.xlsx";
        public const string PlaceholderTestCaseId = "__MISSING_OR_EMPTY_TEST_DATA__";
        private const int TestCaseIdColumn = 2;
        private const int FunctionColumn = 3;
        private const int BigItemColumn = 4;
        private const int MediumItemColumn = 5;
        private const int SmallItemColumn = 6;
        private const int PreConditionColumn = 7;
        private const int StepNumberColumn = 8;
        private const int StepActionColumn = 9;
        private const int TestDataColumn = 10;
        private const int ExpectedResultColumn = 11;
        private const int ActualResultColumn = 12;
        private const int ResultStatusColumn = 13;
        private const int NotesColumn = 14;
        private static string ExcelFilePath => ResolveExcelFilePath();
        static ExcelDataProvider()
        {
            ExcelPackage.License.SetNonCommercialPersonal("DoAnCuoiKy_TH");
        }
        private static string ResolveExcelFilePath()
        {
            var envPath = Environment.GetEnvironmentVariable(ExcelFilePathEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(envPath))
            {
                if (Path.IsPathRooted(envPath))
                {
                    return envPath;
                }

                var fromCurrentDirectory = Path.GetFullPath(envPath, Directory.GetCurrentDirectory());
                if (File.Exists(fromCurrentDirectory))
                {
                    return fromCurrentDirectory;
                }

                var fromAppBaseDirectory = Path.GetFullPath(envPath, AppContext.BaseDirectory);
                if (File.Exists(fromAppBaseDirectory))
                {
                    return fromAppBaseDirectory;
                }
            }

            var current = AppContext.BaseDirectory;

            for (int i = 0; i < 6 && !string.IsNullOrWhiteSpace(current); i++)
            {
                var candidate = Path.Combine(current, DefaultExcelFileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                var candidateInTestData = Path.Combine(current, "TestData", DefaultExcelFileName);
                if (File.Exists(candidateInTestData))
                {
                    return candidateInTestData;
                }

                current = Directory.GetParent(current)?.FullName ?? "";
            }

            return Path.Combine(AppContext.BaseDirectory, DefaultExcelFileName);
        }
        public static IEnumerable<TestCaseModel> GetTestCases(string sheetName, string testCaseFilter)
        {
            using var package = new ExcelPackage(new FileInfo(ExcelFilePath));
            var testCases = new List<TestCaseModel>();

            var worksheet = package.Workbook.Worksheets[sheetName];
            if (worksheet != null)
            {
                testCases.AddRange(ReadTestCasesFromWorksheet(worksheet, testCaseFilter));
            }

            if (testCases.Count > 0)
            {
                return testCases;
            }

            // Fallback: if the requested worksheet is empty or mismatched, scan all sheets.
            foreach (var candidateWorksheet in package.Workbook.Worksheets)
            {
                if (string.Equals(candidateWorksheet.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                testCases.AddRange(ReadTestCasesFromWorksheet(candidateWorksheet, testCaseFilter));
            }

            return testCases;
        }

        private static IEnumerable<TestCaseModel> ReadTestCasesFromWorksheet(ExcelWorksheet worksheet, string testCaseFilter)
        {
            var testCases = new List<TestCaseModel>();

            if (worksheet.Dimension == null)
            {
                return testCases;
            }

            int rowCount = worksheet.Dimension.Rows;
            int dataStartRow = ResolveDataStartRow(worksheet);

            string currentTestCaseId = "";
            string function = "";
            string bigItem = "";
            string mediumItem = "";
            string smallItem = "";
            string preCondition = "";
            string expected = "";

            var steps = new List<TestStepModel>();
            int startRow = 0;

            for (int row = dataStartRow; row <= rowCount; row++)
            {
                string tcId = NormalizeTestCaseId(worksheet.Cells[row, TestCaseIdColumn].Text);

                if (!string.IsNullOrEmpty(tcId))
                {
                    if (steps.Count > 0)
                    {
                        AddTestCase(testCases, currentTestCaseId, function, bigItem, mediumItem, smallItem, preCondition, expected, steps, startRow, testCaseFilter);
                    }

                    currentTestCaseId = tcId;
                    function = worksheet.Cells[row, FunctionColumn].Text;
                    bigItem = worksheet.Cells[row, BigItemColumn].Text;
                    mediumItem = worksheet.Cells[row, MediumItemColumn].Text;
                    smallItem = worksheet.Cells[row, SmallItemColumn].Text;
                    preCondition = worksheet.Cells[row, PreConditionColumn].Text;
                    expected = worksheet.Cells[row, ExpectedResultColumn].Text;

                    steps = new List<TestStepModel>();
                    startRow = row;
                }

                var stepNumbers = ParseStepNumbers(worksheet.Cells[row, StepNumberColumn].Text);
                var stepActions = SplitCellLines(worksheet.Cells[row, StepActionColumn].Text);
                var stepDataItems = SplitCellLines(worksheet.Cells[row, TestDataColumn].Text);

                var stepCount = Math.Max(stepNumbers.Count, stepActions.Count);
                if (stepCount == 0 && !string.IsNullOrWhiteSpace(worksheet.Cells[row, StepActionColumn].Text))
                {
                    stepCount = 1;
                }

                for (var index = 0; index < stepCount; index++)
                {
                    var stepNumber = index < stepNumbers.Count ? stepNumbers[index] : index + 1;
                    var stepAction = index < stepActions.Count ? stepActions[index] : worksheet.Cells[row, StepActionColumn].Text;
                    var stepData = index < stepDataItems.Count
                        ? stepDataItems[index]
                        : (index == 0 ? worksheet.Cells[row, TestDataColumn].Text : string.Empty);

                    steps.Add(new TestStepModel
                    {
                        StepNumber = stepNumber,
                        StepAction = stepAction,
                        TestData = stepData,
                        Actual = worksheet.Cells[row, ActualResultColumn].Text,
                        Result = worksheet.Cells[row, ResultStatusColumn].Text,
                        Notes = worksheet.Cells[row, NotesColumn].Text,
                        ExcelRow = row
                    });
                }
            }

            AddTestCase(testCases, currentTestCaseId, function, bigItem, mediumItem, smallItem, preCondition, expected, steps, startRow, testCaseFilter);

            return testCases;
        }
        public static IEnumerable<object[]> GetNamedTestCases(string sheetName, string testCaseFilter)
        {
            foreach (var testCase in GetTestCases(sheetName, testCaseFilter))
            {
                yield return new object[]
                {
                    testCase.TestCaseId,
                    testCase.Function,
                    testCase.BigItem
                };
            }
        }
        public static TestCaseModel GetTestCaseById(string sheetName, string testCaseFilter, string testCaseId)
        {
            var normalizedRequestedId = NormalizeTestCaseId(testCaseId);
            var testCases = GetTestCases(sheetName, testCaseFilter).ToList();

            var matchedTestCase = testCases
                .FirstOrDefault(tc => string.Equals(NormalizeTestCaseId(tc.TestCaseId), normalizedRequestedId, StringComparison.OrdinalIgnoreCase));

            if (matchedTestCase != null)
            {
                return matchedTestCase;
            }

            var availableIds = string.Join(", ", testCases.Select(tc => tc.TestCaseId).Take(30));
            throw new InvalidOperationException($"Không tìm thấy test case '{testCaseId}' (normalized: '{normalizedRequestedId}') trong sheet '{sheetName}'. Tổng test case load được: {testCases.Count}. IDs: {availableIds}");
        }
        public static string WriteTestResults(List<TestStepModel> steps, string actual, string status, string sheetName, string note = "")
        {
            using var package = new ExcelPackage(new FileInfo(ExcelFilePath));
            var worksheet = package.Workbook.Worksheets[sheetName];

            if (steps == null || steps.Count == 0 || worksheet == null)
            {
                return string.Empty;
            }

            int firstRow = steps.Min(s => s.ExcelRow);
            int lastRow = steps.Max(s => s.ExcelRow);

            SetCellValue(worksheet, lastRow, ActualResultColumn, actual);

            if (!string.IsNullOrEmpty(note))
            {
                SetCellValue(worksheet, lastRow, NotesColumn, note);
            }

            for (int row = firstRow; row <= lastRow; row++)
            {
                SetCellValue(worksheet, row, ResultStatusColumn, status);
                ApplyStatusStyling(worksheet, row, ResultStatusColumn, status);
            }

            if (!string.IsNullOrWhiteSpace(note))
            {
                ApplyStatusStyling(worksheet, lastRow, NotesColumn, status);
            }

            package.Save();

            return $"{GetExcelColumnName(ResultStatusColumn)}{firstRow}:{GetExcelColumnName(ResultStatusColumn)}{lastRow}";
        }
        private static void AddTestCase(
            List<TestCaseModel> testCases,
            string testCaseId,
            string function,
            string bigItem,
            string mediumItem,
            string smallItem,
            string preCondition,
            string expected,
            List<TestStepModel> steps,
            int startRow,
            string filter)
        {
            if (!string.IsNullOrEmpty(testCaseId)
                && steps.Count > 0
                && NormalizeTestCaseId(testCaseId).StartsWith(filter, StringComparison.OrdinalIgnoreCase))
            {
                testCases.Add(new TestCaseModel(
                    testCaseId,
                    function,
                    bigItem,
                    mediumItem,
                    smallItem,
                    preCondition,
                    steps.ToList(),
                    expected,
                    startRow));
            }
        }

        private static string NormalizeTestCaseId(string? rawId)
        {
            if (string.IsNullOrWhiteSpace(rawId))
            {
                return string.Empty;
            }

            var value = rawId.Trim().ToUpperInvariant();
            var match = Regex.Match(value, "TC[\\s_-]*(\\d+)");

            if (!match.Success)
            {
                return value;
            }

            return int.TryParse(match.Groups[1].Value, out var testNumber)
                ? $"TC_{testNumber:00}"
                : value;
        }

        private static void SetCellValue(ExcelWorksheet worksheet, int row, int column, object? value)
        {
            var mergedAddress = worksheet.MergedCells[row, column];

            if (!string.IsNullOrWhiteSpace(mergedAddress))
            {
                var mergedRange = new ExcelAddress(mergedAddress);
                worksheet.Cells[mergedRange.Start.Row, mergedRange.Start.Column].Value = value;
                return;
            }

            worksheet.Cells[row, column].Value = value;
        }

        private static void ApplyStatusStyling(ExcelWorksheet worksheet, int row, int column, string status)
        {
            var mergedAddress = worksheet.MergedCells[row, column];
            ExcelRange targetCell;

            if (!string.IsNullOrWhiteSpace(mergedAddress))
            {
                var mergedRange = new ExcelAddress(mergedAddress);
                targetCell = worksheet.Cells[mergedRange.Start.Row, mergedRange.Start.Column];
            }
            else
            {
                targetCell = worksheet.Cells[row, column];
            }

            var isFail = string.Equals(status?.Trim(), "FAIL", StringComparison.OrdinalIgnoreCase);

            if (isFail)
            {
                targetCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                targetCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(192, 0, 0));
                targetCell.Style.Font.Color.SetColor(Color.White);
                targetCell.Style.Font.Bold = true;
                return;
            }

            targetCell.Style.Fill.PatternType = ExcelFillStyle.None;
            targetCell.Style.Font.Color.SetColor(Color.Black);
            targetCell.Style.Font.Bold = false;
        }

        private static int ResolveDataStartRow(ExcelWorksheet worksheet)
        {
            if (worksheet.Dimension == null)
            {
                return 2;
            }

            var rowLimit = Math.Min(10, worksheet.Dimension.Rows);
            for (var row = 1; row <= rowLimit; row++)
            {
                var header = worksheet.Cells[row, TestCaseIdColumn].Text.Trim();
                if (header.Equals("Test Case ID", StringComparison.OrdinalIgnoreCase))
                {
                    return row + 1;
                }
            }

            return 4;
        }

        private static List<int> ParseStepNumbers(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return [];
            }

            var numbers = Regex.Matches(input, "\\d+")
                .Select(match => int.TryParse(match.Value, out var value) ? value : 0)
                .Where(value => value > 0)
                .ToList();

            return numbers;
        }

        private static List<string> SplitCellLines(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return [];
            }

            return input
                .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();
        }
        private static string GetExcelColumnName(int columnNumber)
        {
            var columnName = string.Empty;

            while (columnNumber > 0)
            {
                var modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }

            return columnName;
        }
    }
}