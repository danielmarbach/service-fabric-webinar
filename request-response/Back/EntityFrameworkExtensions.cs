using System.Fabric;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class EntityFrameworkExtensions
{
    public static void AddEntityFramework(this IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();
        var context = provider.GetService<StatelessServiceContext>();
        var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
        var connectionString = configurationPackage.Settings.Sections["SqlServer"].Parameters["ConnectionString"];

        services.AddDbContextPool<OrderContext>(o => o.UseSqlServer(connectionString.Value));
    }
}