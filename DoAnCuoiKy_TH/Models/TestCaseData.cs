namespace DoAnCuoiKy.Models
{
    public class TestCaseData
    {
        public string TestCaseId { get; set; } = string.Empty;
        public string Function { get; set; } = string.Empty;
        public string BigItem { get; set; } = string.Empty;
        public string MediumItem { get; set; } = string.Empty;
        public string SmallItem { get; set; } = string.Empty;
        public string PreCondition { get; set; } = string.Empty;
        public List<TestStep> Steps { get; set; } = [];
        public string Expected { get; set; } = string.Empty;
        public int StartRow { get; set; }
        public TestCaseData(
            string testCaseId,
            string function,
            string bigItem,
            string mediumItem,
            string smallItem,
            string preCondition,
            List<TestStep> steps,
            string expected,
            int startRow)
        {
            TestCaseId = testCaseId;
            Function = function;
            BigItem = bigItem;
            MediumItem = mediumItem;
            SmallItem = smallItem;
            PreCondition = preCondition;
            Steps = steps;
            Expected = expected;
            StartRow = startRow;
        }
        public override string ToString()
        {
            return TestCaseId;
        }
    }
}