using AfterSchoolAdHoc;
using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;

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

var config = JsonConvert.DeserializeObject<DatabaseConfig>(json).AdHocSqlServer;
if (config is null)
{
    Console.WriteLine(
        @"Ad hoc config section has not been added to the local overrides file file not found; confirm file exists and is in the correct location and try again");
    return;
}

var credentials = new DatabaseCredentials(config.ServerName, config.DatabaseName, config.UserName, config.Password);
var dc = new DatabaseConnection(credentials.ConnectionString);

//GenerateCsvReport.Run(dc);
GenerateCsvReportForSession.Run(dc);