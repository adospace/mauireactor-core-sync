using CoreSync;
using CoreSync.Http.Server;
using CoreSync.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Security.Cryptography.Xml;
using TodoApp.Data;

namespace TodoApp.Web;

public static class ServiceCollectionExtentions
{
    public static void AddSyncServices(this IServiceCollection services, string databasePath)
    {
        services.AddCoreSyncHttpServer();
        services.AddSingleton<ISyncProvider>(serviceProvider =>
        {
            var connectionString = $"Data Source={databasePath}";
            var configurationBuilder =
                new SqliteSyncConfigurationBuilder(connectionString)
                    .Table<TodoItem>(syncDirection: SyncDirection.UploadAndDownload)
                    ;

            return new SqliteSyncProvider(configurationBuilder.Build(), ProviderMode.Remote, new SyncLogger(serviceProvider.GetRequiredService<ILogger<SyncLogger>>()));
        });
    }

    class SyncLogger(ILogger<SyncLogger> logger) : ISyncLogger
    {
        private readonly ILogger<SyncLogger> _logger = logger;

        public void Error(string message)
        {
            _logger.LogError("Sync: {message}", message);
        }

        public void Info(string message)
        {
            _logger.LogInformation("Sync: {message}", message);
        }

        public void Trace(string message)
        {
            _logger.LogTrace("Sync: {message}", message);
        }

        public void Warning(string message)
        {
            _logger.LogWarning("Sync: {message}", message);
        }
    }

}
