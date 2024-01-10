using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using JW_Web_Token.Data; // Add this using statement

namespace JW_Web_Token
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create a new web application using the WebApplicationBuilder
            var builder = WebApplication.CreateBuilder(args);

            // Add Entity Framework DbContext to services, using the specified connection string
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add MVC services to handle controllers
            builder.Services.AddControllers();

            // Add API Explorer for endpoint documentation
            builder.Services.AddEndpointsApiExplorer();

            // Add Swagger for API documentation
            builder.Services.AddSwaggerGen();

            // Build the web application
            var app = builder.Build();

            // If the environment is Development, enable Swagger and SwaggerUI
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Enable HTTPS redirection for added security
            app.UseHttpsRedirection();

            // Enable authorization for the application
            app.UseAuthorization();

            // Map controllers for handling requests
            app.MapControllers();

            // Run the application
            app.Run();
        }
    }
}
