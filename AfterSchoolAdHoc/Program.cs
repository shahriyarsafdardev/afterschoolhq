using AfterSchoolAdHoc;
using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;

var localConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "afterschoolhq-overrides", "appsettings.json");

using var streamReader = new StreamReader(localConfigFilePath);
var json = streamReader.ReadToEnd();
var items = JsonConvert.DeserializeObject<DatabaseConfig>(json);

var credentials = new DatabaseCredentials(items.SqlServer.ServerName, items.SqlServer.DatabaseName, items.SqlServer.UserName, items.SqlServer.Password);
var dc = new DatabaseConnection(credentials.ConnectionString);

GenerateCsvReport.Run(dc);