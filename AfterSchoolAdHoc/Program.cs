using AfterSchoolAdHoc;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;
using System.Drawing.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        void doCSVReportGenerator(DatabaseConnection dc)
        {
            const int accountId = 0;
            const RegistrationReportType reportType = RegistrationReportType.DismissalForHostedActivities;
            const string? commaSeparatedActivityGroupIds = null;

            CsvReportGenerator.Run(accountId, reportType, commaSeparatedActivityGroupIds, dc);
        }

        var localConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "afterschoolhq-overrides", "appsettings.json");

        string json;
        try
        {
            using var streamReader = new StreamReader(localConfigFilePath);
            json = streamReader.ReadToEnd();
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine(@"Local overrides config file not found; confirm file exists and is in the correct location and try again");
            return;
        }

        var config = JsonConvert.DeserializeObject<DatabaseConfig>(json)?.AdHocSqlServer;
        if (config is null)
        {
            Console.WriteLine(
                @"Ad hoc config section has not been added to the local overrides file file not found; confirm file exists and is in the correct location and try again");
            return;
        }

        var credentials = new DatabaseCredentials(config.ServerName, config.DatabaseName, config.UserName, config.Password);
        var dc = new DatabaseConnection(credentials.ConnectionString);
        var msgs = new SystemMessages();

        // doCSVReportGenerator(dc);

        // JotFormImporter.Run("Z:\\Downloads\\Get_started_with_AfterSchool_HQ2022-12-02_15_49_51_Clean.csv", msgs, dc);

        FormDataFix.Run(msgs, dc);
    }
}