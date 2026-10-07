using hoslog.customer.api.Middleware;
using hoslog.customer.api.Security.Options;

namespace hoslog.customer.api.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddDomainValidation(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DomainValidationOptions>(configuration.GetSection("DomainValidation"));
            return services;
        }

        public static IApplicationBuilder UseDomainValidation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<DomainValidationMiddleware>();
        }
    }
}