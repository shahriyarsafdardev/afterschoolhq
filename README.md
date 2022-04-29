# AfterSchool HQ

## Local Requirements

- Windows 10
- .NET Framework 4.7.2
- SQL Server Express
- Visual Studio
- .NET Standard SDK
- .NET Core 2.0

## SQL Server Installation and Setup

### Manual setup

- Install SQL Server Database
- Do NOT do a `named instance` instead select `default instance`
- Turn on Mixed Mode Authentication - required for the local app to access the database
- Install SQL Server Management Studio (SSMS)
- Login to database using SSMS
- Create new `AfterSchool` and `AfterSchoolTest` database
- Create `AfterSchoolUser` in the global area for db users `Security > Logins` and give it Owner access to `AfterSchool` and `AfterSchoolTest` database
- Give it the password found in `Database.cs` for the local and test environments
- Turn off the password policy
- Select `User Mapping` in left menu, click on `AfterSchool` and `AfterSchoolTest`, check the `db owner` checkbox, save the user

### Docker setup

- Open a bash terminal in the root project directory and run `docker-compose up -d`.
- Move bash terminal to `/scripts` directory and run `./initialize-sqlserver.sh`.

## Getting up and running

- Open up the project using - `AfterSchoolHQ - Darye.sln`
- Set `after-school-hq` the start up project
- Run the Code - `after-school-hq` (Hit IIS Express button at the top middle of the screen)
- Go to `/db/update` in your browser to create tables and seed the database.
- Go to `/login` and use default admin credentials found in `DBController.cs`

## Resources

- Will add them at some point

