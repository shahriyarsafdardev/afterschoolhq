# AfterSchool HQ

## Local Requirements

- Windows 10
- .NET Framework 4.6.2+
- SQL Server Express
- Visual Studio
- .NET Standard SDK

## SQL Server Installation and Setup

- Install SQL Server Database
- Don't do a `named instance` instead select `default instance`
- Turn on Mixed Mode Authentication - required for the local app to access the database
- Install SQL Server Management Studio (SSMS)
- Login to database using SSMS
- Create new `AfterSchool` database
- Create `AfterSchoolUser` and give it Owner access to `AfterSchool` database
- Give it the password found in `Database.cs`
- Turn off the password policy
- Select `User Mapping` in left menu, click on AfterSchool, check the `db owner` checkbox, save the user

## Getting up and running

- Turn on the ASP.NET State Service
	- Go to start menu
	- Type "Services"
	- Open Services (should be top choice)
	- Go to ASP.NET State Service
	- Open Properties
	- Setup Startup Type to Automatic
	- "Start" the service
- Open up the code - `AfterSchoolHQ - Darye.sln`
- Set `after-school-hq` the start up project
- Run the Code - `after-school-hq` (Hit IIS Express button at the top middle of the screen)
- Run `/db/update` to create tables and create default admin account for logging in
- Go to `/login` and use default admin credentials found in `DBController.cs`

## Resources

- Will add them at some point

