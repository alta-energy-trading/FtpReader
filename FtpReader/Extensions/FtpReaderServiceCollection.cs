using Reader.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FtpReader.Extensions
{
    public static class FtpReaderServiceCollection
    {
        public static IServiceCollection AddApiReader(this IServiceCollection services)
        {
            services.AddScoped<IConsumer<BroadcastArgs>, Reader<BroadcastArgs>>();
            return services;
        }
    }
}
