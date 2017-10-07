using System;
using System.Fabric;

static class ConnectionStringHelper
{
    const string RabbitMqTransportEnvironmentVariable = "RabbitMQTransport.ConnectionString";

    public static string GetTransportConnectionString(this ServiceContext context)
    {
        var connectionString = Environment.GetEnvironmentVariable(RabbitMqTransportEnvironmentVariable, EnvironmentVariableTarget.Machine);

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        connectionString = GetConnectionStringFromConfig(context, "NServiceBus");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new Exception("Could not read the 'NServiceBus.ConnectionString' configuration variable. Check the sample prerequisites.");
        }

        return connectionString;
    }

    public static string GetDbConnectionString(this ServiceContext context)
    {
        var connectionString = GetConnectionStringFromConfig(context, "SqlServer");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new Exception("Could not read the 'SqlServer.ConnectionString' configuration variable. Check the sample prerequisites.");
        }

        return connectionString;
    }


    static string GetConnectionStringFromConfig(ServiceContext context, string section)
    {
        var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
        return configurationPackage.Settings.Sections[section].Parameters["ConnectionString"].Value;
    }
}
