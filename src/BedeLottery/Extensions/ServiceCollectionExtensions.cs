using BedeLottery.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BedeLottery.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLotteryGameServices(this IServiceCollection services)
    {
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IPrizeCalculationService, PrizeCalculationService>();
        services.AddScoped<ILotteryGameService, LotteryGameService>();
        services.AddScoped<IGameUIService, GameUIService>();
        services.AddScoped<IGameOrchestrator, GameOrchestrator>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>();
        services.AddSingleton<IConsoleService, ConsoleService>();

        return services;
    }

    public static IServiceCollection AddGameConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var gameConfigSection = configuration.GetSection(GameConfiguration.SectionName);
        services.Configure<GameConfiguration>(gameConfigSection);
        services.AddSingleton<IValidateOptions<GameConfiguration>, GameConfigurationValidator>();

        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<GameConfiguration>>();
            return options.Value;
        });

        return services;
    }

    public static IServiceCollection AddLotteryGame(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddGameConfiguration(configuration)
            .AddInfrastructureServices()
            .AddLotteryGameServices();
    }
}

public class GameConfigurationValidator : IValidateOptions<GameConfiguration>
{
    public ValidateOptionsResult Validate(string? name, GameConfiguration options)
    {
        try
        {
            options.Validate();
            return ValidateOptionsResult.Success;
        }
        catch (Exception ex)
        {
            return ValidateOptionsResult.Fail($"Game configuration validation failed: {ex.Message}");
        }
    }
}
