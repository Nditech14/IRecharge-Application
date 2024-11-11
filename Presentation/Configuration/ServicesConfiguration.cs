using Application.PayStack;
using Application.Service.Abstraction;
using Application.Service.Implementation;
using Infrastructure.Mapping;
using Infrastructure.Untilities.CachManager;
using Infrastructure.Untilities.Common;
using Infrastructure.Untilities.Communication.Abstraction;
using Infrastructure.Untilities.Communication.Implemenatation;

namespace Presentation.Configuration
{
    public static class ServicesConfiguration
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IPayStackService, PayStackService>()
                    .AddScoped(typeof(ILoggerService<>), typeof(LoggerService<>))
                    .AddScoped<IEmailService, EmailService>()
                    .AddAutoMapper(typeof(AutoMapperProfile))
                    .AddScoped<IRedisCacheManager, RedisCacheManager>()
                    .AddScoped<IWalletService, WalletService>()
                    .AddScoped<IBillService, BillService>();
                    





        }
    }
}
