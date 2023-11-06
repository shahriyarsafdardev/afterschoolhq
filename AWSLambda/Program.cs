using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Integrations;
using AfterSchoolLibrary.Models;
using Emmersion.InfluxDB;
using Rollbar;
using Stripe;
using System.Web.Mvc;
using RollbarSharp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var localConfigOverridesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "afterschoolhq-overrides", "appsettings.json");

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile(localConfigOverridesPath, optional: true, reloadOnChange: true);

builder.Services.AddTransient(provider => new DatabaseCredentials(
                builder.Configuration.GetValue<string>("SqlServer:ServerName"),
                builder.Configuration.GetValue<string>("SqlServer:DatabaseName"),
                builder.Configuration.GetValue<string>("SqlServer:UserName"),
                builder.Configuration.GetValue<string>("SqlServer:Password")));
builder.Services.AddTransient(provider => new SterlingCredentials(
                builder.Configuration.GetValue<string>("Sterling:Endpoint"),
                builder.Configuration.GetValue<string>("Sterling:ClientId"),
                builder.Configuration.GetValue<string>("Sterling:ClientSecret"),
                builder.Configuration.GetValue<string>("Sterling:PackageId"),
                builder.Configuration.GetValue<string>("Sterling:AccountId")));
builder.Services.AddSingleton(new SMTPMailTrapHelper(
                builder.Configuration.GetValue<string>("Mailtrap:Username"),
                builder.Configuration.GetValue<string>("Mailtrap:Password"),
                builder.Configuration.GetValue<string>("Mailtrap:Host"),
                builder.Configuration.GetValue<string>("Mailtrap:Port")));
builder.Services.AddSingleton(new RollbarConfig(
                builder.Configuration.GetValue<string>("Rollbar:AccessToken")));

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

var app = builder.Build();

var configuration = app.Configuration;

var serviceProvider = builder.Services.BuildServiceProvider();

var connectionString = serviceProvider.GetService<DatabaseCredentials>().ConnectionString;
BasicLibrary.Utilities.ConnectionString = connectionString;
Database.ConnectionString = connectionString;

RollbarSharp.Configuration.DefaultEnvironment = AfterSchoolLibrary.Helpers.Utilities.GetRollbarEnvironment();
AfterSchoolLibrary.Helpers.Utilities.InitializeApp();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseRouting();

app.Run();
