using OfficeOpenXml;
using TestCaseModel = THFinalTest.ParaBankAutomation.Models.TestCaseData;
using TestStepModel = THFinalTest.ParaBankAutomation.Models.TestStep;

namespace DoAnCuoiKy_TH.DataProviders
{
    public class ExcelDataProvider
    {
        private const string ExcelFilePathEnvironmentVariable = "BDCLPM_EXCEL_PATH";
        private const string DefaultExcelFileName = "Testcase.xlsx";
        public const string PlaceholderTestCaseId = "__MISSING_OR_EMPTY_TEST_DATA__";
        private const int DataStartRow = 5;
        private const int TestCaseIdColumn = 3;
        private const int FunctionColumn = 4;
        private const int BigItemColumn = 5;
        private const int MediumItemColumn = 6;
        private const int SmallItemColumn = 7;
        private const int PreConditionColumn = 8;
        private const int StepNumberColumn = 9;
        private const int StepActionColumn = 10;
        private const int TestDataColumn = 11;
        private const int ExpectedResultColumn = 12;
        private const int ActualResultColumn = 13;
        private const int ResultStatusColumn = 14;
        private const int NotesColumn = 15;
        private static string ExcelFilePath => ResolveExcelFilePath();

        static ExcelDataProvider()
        {
            ExcelPackage.License.SetNonCommercialPersonal("BDCLPM_THFinal");
        }

        private static string ResolveExcelFilePath()
        {
            var envPath = Environment.GetEnvironmentVariable(ExcelFilePathEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(envPath))
            {
                return envPath;
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
            var testCases = new List<TestCaseModel>();

            using var package = new ExcelPackage(new FileInfo(ExcelFilePath));
            var worksheet = package.Workbook.Worksheets[sheetName];

            if (worksheet?.Dimension == null)
            {
                return testCases;
            }

            int rowCount = worksheet.Dimension.Rows;

            string currentTestCaseId = "";
            string function = "";
            string bigItem = "";
            string mediumItem = "";
            string smallItem = "";
            string preCondition = "";
            string expected = "";

            var steps = new List<TestStepModel>();
            int startRow = 0;

            for (int row = DataStartRow; row <= rowCount; row++)
            {
                string tcId = worksheet.Cells[row, TestCaseIdColumn].Text.Trim();

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

                if (int.TryParse(worksheet.Cells[row, StepNumberColumn].Text, out int stepNum))
                {
                    steps.Add(new TestStepModel
                    {
                        StepNumber = stepNum,
                        StepAction = worksheet.Cells[row, StepActionColumn].Text,
                        TestData = worksheet.Cells[row, TestDataColumn].Text,
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
            return GetTestCases(sheetName, testCaseFilter)
                .First(tc => string.Equals(tc.TestCaseId, testCaseId, StringComparison.OrdinalIgnoreCase));
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
            if (!string.IsNullOrEmpty(testCaseId) && steps.Count > 0 && testCaseId.StartsWith(filter))
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