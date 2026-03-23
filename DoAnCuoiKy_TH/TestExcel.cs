using System.Globalization;
using DoAnCuoiKy_TH.DataProviders;
using NUnit.Framework;
using OfficeOpenXml;

namespace DoAnCuoiKy_TH.Tests
{
    [TestFixture]
    public class ExcelDataProviderTests
    {
        private const string ExcelFileName = "TC_Khoa.xlsx";
        private const string SheetName = "TC_khoa";
        private const string TestCasePrefix = "TC_";
        private string excelPath = string.Empty;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            ExcelPackage.License.SetNonCommercialPersonal("DoAnCuoiKy_TH");
            excelPath = FindFileFromTestDirectory(ExcelFileName);
            Environment.SetEnvironmentVariable("BDCLPM_EXCEL_PATH", excelPath);

            EnsureSheetIsRenamed(excelPath, "TestCases_Cleaned", SheetName);
        }

        [Test]
        public void Test01_GetTestCases_ShouldReturnData()
        {
            var testCases = ExcelDataProvider.GetTestCases(SheetName, TestCasePrefix).ToList();

            Assert.That(testCases, Is.Not.Empty, "Expected at least one test case in sheet TC_khoa.");
        }

        [Test]
        public void Test02_TestCaseIds_ShouldFollowTcPattern()
        {
            var testCases = ExcelDataProvider.GetTestCases(SheetName, TestCasePrefix).ToList();

            foreach (var testCase in testCases)
            {
                Assert.That(testCase.TestCaseId, Does.StartWith("TC_"), $"Invalid TestCaseId format: {testCase.TestCaseId}");
            }
        }

        [Test]
        public void Test03_EachTestCase_ShouldContainAtLeastOneStep()
        {
            var testCases = ExcelDataProvider.GetTestCases(SheetName, TestCasePrefix).ToList();

            foreach (var testCase in testCases)
            {
                Assert.That(testCase.Steps, Is.Not.Null);
                Assert.That(testCase.Steps.Count, Is.GreaterThan(0), $"Test case {testCase.TestCaseId} has no steps.");
            }
        }

        [Test]
        public void Test04_StepNumbers_ShouldBePositive()
        {
            var testCases = ExcelDataProvider.GetTestCases(SheetName, TestCasePrefix).ToList();

            foreach (var testCase in testCases)
            {
                foreach (var step in testCase.Steps)
                {
                    Assert.That(step.StepNumber, Is.GreaterThan(0), $"Step number must be > 0 in {testCase.TestCaseId}.");
                }
            }
        }

        [Test]
        public void Test05_GetNamedTestCases_ShouldMatchTestCaseCount()
        {
            var named = ExcelDataProvider.GetNamedTestCases(SheetName, TestCasePrefix).ToList();
            var all = ExcelDataProvider.GetTestCases(SheetName, TestCasePrefix).ToList();

            Assert.That(named.Count, Is.EqualTo(all.Count));
        }

        [Test]
        public void Test06_GetTestCaseById_ShouldReturnExistingCase()
        {
            var firstCase = ExcelDataProvider.GetTestCases(SheetName, TestCasePrefix).First();
            var loaded = ExcelDataProvider.GetTestCaseById(SheetName, TestCasePrefix, firstCase.TestCaseId);

            Assert.That(loaded.TestCaseId, Is.EqualTo(firstCase.TestCaseId));
            Assert.That(loaded.Steps.Count, Is.EqualTo(firstCase.Steps.Count));
        }

        [Test]
        public void Test07_GetTestCaseById_ShouldThrowForUnknownId()
        {
            Assert.Throws<InvalidOperationException>(() =>
                ExcelDataProvider.GetTestCaseById(SheetName, TestCasePrefix, "TC_NOT_EXISTS_999"));
        }

        [Test]
        public void Test08_WriteTestResults_WhenPass_ShouldWriteStatusAndActual()
        {
            var testCase = ExcelDataProvider.GetTestCases(SheetName, TestCasePrefix).First();
            var actual = "Auto test PASS " + DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

            var updatedRange = ExcelDataProvider.WriteTestResults(testCase.Steps, actual, "PASS", SheetName);
            Assert.That(updatedRange, Is.Not.Empty);

            using var package = new ExcelPackage(new FileInfo(excelPath));
            var worksheet = package.Workbook.Worksheets[SheetName];
            Assert.That(worksheet, Is.Not.Null);

            var lastRow = testCase.Steps.Max(s => s.ExcelRow);
            var actualCell = ResolveCell(worksheet!, lastRow, 12);
            var statusCell = ResolveCell(worksheet!, lastRow, 13);

            Assert.That(actualCell.Text, Is.EqualTo(actual));
            Assert.That(statusCell.Text, Is.EqualTo("PASS"));
        }

        [Test]
        public void Test09_WriteTestResults_WhenFail_ShouldWriteScreenshotPath()
        {
            var testCase = ExcelDataProvider.GetTestCases(SheetName, TestCasePrefix).First();
            var actual = "Auto test FAIL";
            var screenshotPath = Path.Combine("Screenshots", "tc_fail_test09.png");
            var note = "Screenshot: " + screenshotPath;

            ExcelDataProvider.WriteTestResults(testCase.Steps, actual, "FAIL", SheetName, note);

            using var package = new ExcelPackage(new FileInfo(excelPath));
            var worksheet = package.Workbook.Worksheets[SheetName];
            Assert.That(worksheet, Is.Not.Null);

            var lastRow = testCase.Steps.Max(s => s.ExcelRow);
            var noteCell = ResolveCell(worksheet!, lastRow, 14);
            Assert.That(noteCell.Text, Does.Contain("Screenshot:"));
            Assert.That(noteCell.Text, Does.Contain("tc_fail_test09.png"));
        }

        [Test]
        public void Test10_WriteTestResults_WhenFail_ShouldColorFailCellsRed()
        {
            var testCase = ExcelDataProvider.GetTestCases(SheetName, TestCasePrefix).First();
            var note = "Screenshot: Screenshots/tc_fail_test10.png";

            ExcelDataProvider.WriteTestResults(testCase.Steps, "Auto test FAIL color", "FAIL", SheetName, note);

            using var package = new ExcelPackage(new FileInfo(excelPath));
            var worksheet = package.Workbook.Worksheets[SheetName];
            Assert.That(worksheet, Is.Not.Null);

            var lastRow = testCase.Steps.Max(s => s.ExcelRow);
            var statusCell = ResolveCell(worksheet!, lastRow, 13);
            var noteCell = ResolveCell(worksheet!, lastRow, 14);

            Assert.That(statusCell.Style.Fill.PatternType.ToString(), Is.EqualTo("Solid"));
            Assert.That(noteCell.Style.Fill.PatternType.ToString(), Is.EqualTo("Solid"));
        }

        private static void EnsureSheetIsRenamed(string path, string sourceSheetName, string targetSheetName)
        {
            using var package = new ExcelPackage(new FileInfo(path));
            var targetSheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(targetSheetName, StringComparison.OrdinalIgnoreCase));
            if (targetSheet != null)
            {
                return;
            }

            var sourceSheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(sourceSheetName, StringComparison.OrdinalIgnoreCase));
            if (sourceSheet == null)
            {
                return;
            }

            sourceSheet.Name = targetSheetName;
            package.Save();
        }

        private static string FindFileFromTestDirectory(string fileName)
        {
            var current = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);

            while (current != null)
            {
                var candidate = Path.Combine(current.FullName, fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                current = current.Parent;
            }

            throw new FileNotFoundException($"Cannot find {fileName} from test directory.");
        }

        private static ExcelRangeBase ResolveCell(ExcelWorksheet worksheet, int row, int column)
        {
            var mergedAddress = worksheet.MergedCells[row, column];
            if (!string.IsNullOrWhiteSpace(mergedAddress))
            {
                var range = new ExcelAddress(mergedAddress);
                return worksheet.Cells[range.Start.Row, range.Start.Column];
            }

            return worksheet.Cells[row, column];
        }
    }
}