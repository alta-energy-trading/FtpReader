using Reader.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FtpReader.Console.ToFile
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IConsumer<FileBroadcastArgs>, Reader<FileBroadcastArgs>>();
            services.AddScoped<IStreamBroadcaster<FileBroadcastArgs>, FileBroadcaster>();
        }
    }
}
