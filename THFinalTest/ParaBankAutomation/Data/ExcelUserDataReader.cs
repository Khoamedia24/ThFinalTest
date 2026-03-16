using ClosedXML.Excel;
using ParaBankAutomation.Models;

namespace ParaBankAutomation.Data;

public static class ExcelUserDataReader
{
    public static List<UserData> ReadUsers(string filePath)
    {
        if (!File.Exists(filePath))
        {
            CreateTemplateWorkbook(filePath);
        }

        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheet("Users");

        var users = new List<UserData>();
        var row = 2;

        while (!worksheet.Cell(row, 1).IsEmpty())
        {
            users.Add(new UserData
            {
                FirstName = worksheet.Cell(row, 1).GetString(),
                LastName = worksheet.Cell(row, 2).GetString(),
                Address = worksheet.Cell(row, 3).GetString(),
                City = worksheet.Cell(row, 4).GetString(),
                State = worksheet.Cell(row, 5).GetString(),
                ZipCode = worksheet.Cell(row, 6).GetString(),
                Phone = worksheet.Cell(row, 7).GetString(),
                Ssn = worksheet.Cell(row, 8).GetString(),
                Username = worksheet.Cell(row, 9).GetString(),
                Password = worksheet.Cell(row, 10).GetString()
            });

            row++;
        }

        if (users.Count == 0)
        {
            throw new InvalidOperationException("No user data found in Excel file.");
        }

        return users;
    }

    private static void CreateTemplateWorkbook(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Users");

        worksheet.Cell(1, 1).Value = "FirstName";
        worksheet.Cell(1, 2).Value = "LastName";
        worksheet.Cell(1, 3).Value = "Address";
        worksheet.Cell(1, 4).Value = "City";
        worksheet.Cell(1, 5).Value = "State";
        worksheet.Cell(1, 6).Value = "ZipCode";
        worksheet.Cell(1, 7).Value = "Phone";
        worksheet.Cell(1, 8).Value = "Ssn";
        worksheet.Cell(1, 9).Value = "Username";
        worksheet.Cell(1, 10).Value = "Password";

        worksheet.Cell(2, 1).Value = "Auto";
        worksheet.Cell(2, 2).Value = "Tester";
        worksheet.Cell(2, 3).Value = "123 Main St";
        worksheet.Cell(2, 4).Value = "HCM";
        worksheet.Cell(2, 5).Value = "HCM";
        worksheet.Cell(2, 6).Value = "700000";
        worksheet.Cell(2, 7).Value = "0900000000";
        worksheet.Cell(2, 8).Value = "123456789";
        worksheet.Cell(2, 9).Value = "auto_user_001";
        worksheet.Cell(2, 10).Value = "P@ssw0rd123";

        workbook.SaveAs(filePath);
    }
}
