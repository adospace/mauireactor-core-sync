using CoreSync;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography.X509Certificates;
using TodoApp.Data;

namespace TodoApp.Web;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder UseLatestDatabaseVersion(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();

        return app;
    }

    public static IApplicationBuilder SetupServerSynchronization(this IApplicationBuilder app)
    {
        var syncProvider = app.ApplicationServices.GetRequiredService<ISyncProvider>();

        syncProvider.ApplyProvisionAsync().Wait();

        return app;
    }


    public static IApplicationBuilder ResetServerSynchronization(this IApplicationBuilder app)
    {
        var syncProvider = app.ApplicationServices.GetRequiredService<ISyncProvider>();

        syncProvider.RemoveProvisionAsync().Wait();

        return app;
    }
}
