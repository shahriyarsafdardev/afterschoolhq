namespace AfterSchoolAdHoc;

public static class AdHocUtilities
{
    public static void SaveReport(byte[] byteData)
    {
        var path = $"report-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv";
        File.WriteAllBytes(path, byteData);
        Console.WriteLine(@$"Report successfully saved as {path}");
    }
}