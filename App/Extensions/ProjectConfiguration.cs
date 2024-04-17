using App.Data.Database;
using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace App.Extensions
{
    public static class ProjectConfiguration
    {
        public static void RegisterService(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Services.AddDbContext<ApplicationDbContext>(
                options => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

            var assembly = typeof(Program).Assembly;

            builder.Services.AddMediatR(config => 
                config.RegisterServicesFromAssembly(assembly));

            builder.Services.AddCarter();
            builder.Services.AddValidatorsFromAssembly(assembly);
        }
        public static void RegisterMiddlewares(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API"));
                app.ApplyMigrations();
            }
            app.MapCarter();
        }
    }
}
