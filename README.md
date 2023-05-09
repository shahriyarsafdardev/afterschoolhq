# AfterSchool HQ

## Local Requirements

- Windows 10
- .NET Framework 4.7.2
- SQL Server Express
- Visual Studio
- .NET Standard SDK
- .NET Core 2.0
- Node 16

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

### React setup

- Get inside the `AfterSchool-HQ\afterschoolhq\AfterSchool\React` directory with `cmd/terminal`.
- Run following commands inside the React directory:
```
npm install
```
- Webpacker will compile react app code when you run the following command
```
npm run start
```
##### ONLY WHEN UPDATING REACT CODE (In Development)
Above command will provide a script like below:
```
<script id="__bs_script__">//<![CDATA[
    document.write("<script async src='http://HOST:3000/browser-sync/browser-sync-client.js?v=2.27.11'><\/script>".replace("HOST", location.hostname));
//]]></script>
```

Add that script in the `reactapp/index.html` file, just before closing of the `</body>` tag. It will watch the React app code for any change and if any change occurs webpacker will recompile the code to reflect those changes. So, you don't have to re-run webpacker manually every time you make any change to React app code.

## Configure Local Connection Details

- Create appsettings.json in `Users\{username}\afterschoolhq-overrides\`
  - Default values:
```
{
  "SqlServer": {
    "ServerName": "localhost",
    "DatabaseName": "AfterSchool",
    "UserName": "AfterSchoolUser",
    "Password": "Nj8Ed$wpLfJ"
  },
  "InfluxDB": {
    "Hostname": "http://localhost:8086",
    "OrgName":  "afterschoolhq",
    "BucketName": "local",
    "ApiToken": "See instructions below to setup api token"
  }
}
```
- [How to Setup Local Influx Api Token](https://docs.influxdata.com/influxdb/cloud/security/tokens/create-token/)


## Getting up and running

- Open up the project using - `AfterSchoolHQ - Darye.sln`
- Set `after-school-hq` the start up project
- Run the Code - `after-school-hq` (Hit IIS Express button at the top middle of the screen)
- Go to `/db/update` in your browser to create tables and seed the database.
- Go to `/login` and use default admin credentials found in `DBController.cs`

## Resources

- Will add them at some point

