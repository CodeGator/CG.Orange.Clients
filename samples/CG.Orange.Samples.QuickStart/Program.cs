
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

BootstrapLogger.LogLevelToDebug();

while(true)
{
    var builder = new HostBuilder().ConfigureAppConfiguration(x =>
        x.AddOrangeConfiguration(options =>
        {
            options.Application = "test1";
            options.Environment = "development";
            options.Url = "https://localhost:7145";
            options.ClientId = "cg.orange.samples.quickstart";
            options.ReloadOnChange = true;
        },
        BootstrapLogger.Instance()
        ));

    builder.Build().RunDelegate((host, token) =>
    {
        Console.WriteLine();

        var cfg = host.Services.GetRequiredService<IConfiguration>();
        if (cfg.GetChildren().Any())
        {
            foreach (var kvp in cfg.GetChildren())
            {
                Console.WriteLine($"key: {kvp.Key}, value: {kvp.Value}");
            }
        }
        else
        {
            Console.WriteLine("no configuration found! check the settings / make sure the server is running.");
        }

        ChangeToken.OnChange(
            () => cfg.GetReloadToken(),
            () => 
            {
                Console.WriteLine("change detected from the microservice!");
            });

        Console.WriteLine();
        Console.WriteLine("press Q to stop, or any other to continue");

        var key = Console.ReadKey();
        if (key.Key == ConsoleKey.Q)
        {
            Console.WriteLine("done.");
            return;
        }
        Console.WriteLine();
    });    
}
