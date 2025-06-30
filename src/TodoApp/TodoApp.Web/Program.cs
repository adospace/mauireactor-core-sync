using CoreSync.Http.Server;
using TodoApp.Data;
using TodoApp.Web;

var dbFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "todoapp", "server");
Directory.CreateDirectory(dbFolder);

string databasePath = Path.Combine(dbFolder, "todoapp.db");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataServices(databasePath);

builder.Services.AddSyncServices(databasePath);


var app = builder.Build();

app.UseCoreSyncHttpServer();

app.UseLatestDatabaseVersion();

app.SetupServerSynchronization();

app.Run();