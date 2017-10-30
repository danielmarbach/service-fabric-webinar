using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.ServiceFabric.Services.Runtime;
using NServiceBus.Persistence.Sql;

[assembly: SqlPersistenceSettings(MsSqlServerScripts = true, ScriptPromotionPath = "$(SolutionDir)PromotedSqlScripts")]

namespace Back_Stateless
{
    internal static class Program
    {
        private static void Main()
        {
            try
            {
                ServiceRuntime.RegisterServiceAsync("Back_StatelessType",
                    context => new Back_Stateless(context)).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(Back_Stateless).Name);

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
