using AfterSchoolAdHoc;
using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;

var localConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "afterschoolhq-overrides", "appsettings.json");

using var streamReader = new StreamReader(localConfigFilePath);
var json = streamReader.ReadToEnd();
var items = JsonConvert.DeserializeObject<DatabaseConfig>(json)!.AdHocSqlServer;
var databaseCredentials = new DatabaseCredentials(items.ServerName, items.DatabaseName, items.UserName, items.Password);
var dc = new DatabaseConnection(databaseCredentials.ConnectionString);

const int accountId = 0;

GenerateCsvReportForAllActivities.Run(accountId, dc);
// GenerateCsvReportForSession.Run(accountId, RegistrationReportType.Detailed, "00000000-0000-0000-0000-000000000000", dc);