using Reader.Core;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace FtpReader.Console.ToFile
{
    class Program
    {
        static void Main(string[] args)
        {
            //Read Configuration from appSettings
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env}.json", false)
                .AddCommandLine(args)
                .Build();

            //Initialize Logger
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            IServiceCollection services = new ServiceCollection();
            Startup startup = new Startup();
            startup.ConfigureServices(services);

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            try
            {
                Log.Information("FtpReader Starting");

                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(o =>
                    {
                        Run(serviceProvider, o).Wait();
                    });

                Log.Information("FtpReader Complete");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "FtpReader encountered an error");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task Run(IServiceProvider serviceProvider, Options options)
        {
            var reader = serviceProvider.GetRequiredService<IConsumer<FileBroadcastArgs>>();
            FtpConsumerArgs args = new FtpConsumerArgs
            {
                Host = options.Host,
                Filenames = options.Filenames,
                PrivateKeyFilePath = options.PrivateKeyPath,
                RemotePath = options.RemotePath,
                LocalPath = options.OutputPath,
                Password = options.Password,
                Username = options.Username,
                AuthType = options.AuthType,
                Protocol = options.Protocol
            };

            FileBroadcastArgs broadcastArgs = new FileBroadcastArgs
            {
                OutputPath = options.OutputPath
            };

            await reader.Read(args, broadcastArgs);
        }
    }
}
