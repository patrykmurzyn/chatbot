using ChatbotAI.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ChatbotAI.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            return services;
        }
    }
} 