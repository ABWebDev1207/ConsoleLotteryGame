using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BedeLottery.Services;
using BedeLottery.Extensions;

namespace BedeLottery;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var gameOrchestrator = host.Services.GetRequiredService<IGameOrchestrator>();
        await gameOrchestrator.RunGameAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                var basePath = AppContext.BaseDirectory;
                config.SetBasePath(basePath);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddLotteryGame(context.Configuration);
                services.AddSingleton<IRandomNumberGeneratorFactory, RandomNumberGeneratorFactory>();
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            });
}
