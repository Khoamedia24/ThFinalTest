namespace DoAnCuoiKy.Models
{

    public class TestStep
    {
        public int StepNumber { get; set; }
        public string? StepAction { get; set; }
        public string? TestData { get; set; }
        public string? Actual { get; set; }
        public string? Result { get; set; }
        public string? Notes { get; set; }
        public int ExcelRow { get; set; }
    }
}