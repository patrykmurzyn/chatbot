using ChatbotAI.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChatbotAI.API.Extensions
{
    public static class WebApplicationExtensions
    {
        /// <summary>
        /// Applies pending Entity Framework Core migrations.
        /// </summary>
        public static WebApplication UseMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
            return app;
        }

        /// <summary>
        /// Enables Swagger UI when in the Development environment.
        /// </summary>
        public static WebApplication UseSwaggerIfDevelopment(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            return app;
        }

        /// <summary>
        /// Applies the configured CORS policy.
        /// </summary>
        public static WebApplication UseCustomCors(this WebApplication app)
        {
            app.UseCors("CorsPolicy");
            return app;
        }
    }
} 