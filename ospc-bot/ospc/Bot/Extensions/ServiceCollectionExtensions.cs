using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OSPC.Domain.Options;

namespace OSPC.Bot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppOption<T>(this IServiceCollection serviceCollection, IConfiguration config) where T : class, IAppOptions
        {
            serviceCollection
                .AddOptions<T>()
                .Bind(config.GetSection(T.Name))
                .ValidateDataAnnotations();

            return serviceCollection;
        }
    }    
}
