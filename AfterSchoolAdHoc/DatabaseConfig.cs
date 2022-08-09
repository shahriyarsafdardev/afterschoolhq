namespace AfterSchoolAdHoc;

public class DatabaseConfig
{
    public SqlServerConnectionParameters SqlServer { get; init; }
}

public class SqlServerConnectionParameters
{
    public string ServerName { get; init; }
    public string DatabaseName { get; init; }
    public string UserName { get; init; }
    public string Password { get; init; }
}