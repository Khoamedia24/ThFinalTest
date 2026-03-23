using DoAnCuoiKy_TH.Utilities;
using NUnit.Framework;

namespace DoAnCuoiKy_TH.Tests
{
    [TestFixture]
    public class ExcelFormatterTests
    {
        [Test]
        [Explicit("Run this manually to format Tc_khoaa sheet")]
        public void FormatTcKhoaaSheet()
        {
            ExcelFormatterUtility.FormatTcKhoaaSheet();
        }
    }
}
