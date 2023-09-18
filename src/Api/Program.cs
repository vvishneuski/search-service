namespace SearchService.Api;

using Destructurama;
using Serilog;
using Serilog.Core;

public class Program
{
    public static Task<int> Main(string[] args) => LogAndRunAsync(CreateHostBuilder(args).Build());

    public static async Task<int> LogAndRunAsync(IHost host)
    {
        if (host is null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();
        Log.Logger = CreateLogger(host);

        try
        {
            Log.Information(
                "Started {Application} in {Environment} mode.",
                hostEnvironment.ApplicationName,
                hostEnvironment.EnvironmentName);
            await host.RunAsync().ConfigureAwait(false);
            Log.Information(
                "Stopped {Application} in {Environment} mode.",
                hostEnvironment.ApplicationName,
                hostEnvironment.EnvironmentName);
            return 0;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            Log.Fatal(
                exception,
                "{Application} terminated unexpectedly in {Environment} mode.",
                hostEnvironment.ApplicationName,
                hostEnvironment.EnvironmentName);
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .UseDefaultServiceProvider((context, options) =>
        {
            var isDevelopment = context.HostingEnvironment.IsDevelopment();
            options.ValidateScopes = isDevelopment;
            options.ValidateOnBuild = isDevelopment;
        })
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            var builtConfig = config.Build();
            var section = builtConfig.GetSection("Vault");
            if (!string.IsNullOrWhiteSpace(section["Address"]))
            {
                config.AddVault(options => section.Bind(options));
            }
        })
        .ConfigureWebHostDefaults(ConfigureWebHostBuilder);

    private static void ConfigureWebHostBuilder(IWebHostBuilder webBuilder) => webBuilder
        .UseStartup<Startup>();

    private static Logger CreateLogger(IHost host)
    {
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();
        return new LoggerConfiguration()
            .Destructure.UsingAttributes()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application",
                configuration["DOTNET_APPLICATIONNAME"] ?? hostEnvironment.ApplicationName)
            .Enrich.WithProperty("Environment", hostEnvironment.EnvironmentName)
            .CreateLogger();
    }
}
