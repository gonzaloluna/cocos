using System.Text.Json.Serialization;
using CocosTradingAPI.Application.Interfaces;
using CocosTradingAPI.Application.Services;
using CocosTradingAPI.Infrastructure.Data;
using CocosTradingAPI.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CocosTradingAPI.WebAPI
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_configuration.GetConnectionString("DefaultConnection")));

            services.AddControllers();
            services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            services.AddScoped<IInstrumentRepository, InstrumentRepository>();
            services.AddScoped<IPortfolioService, PortfolioService>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IMarketDataRepository, MarketDataRepository>();
            services.AddScoped<IPortfolioCalculator, PortfolioCalculator>();
            
            services.AddScoped<IOrderExecutionStrategy,BuyLimitOrderStrategy>();
            services.AddScoped<IOrderExecutionStrategy,BuyMarketOrderStrategy>();
            services.AddScoped<IOrderExecutionStrategy,SellLimitOrderStrategy>();
            services.AddScoped<IOrderExecutionStrategy,SellMarketOrderStrategy>();
            services.AddScoped<IOrderService,OrderService>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/problem+json";

                    var problem = new ProblemDetails
                    {
                        Status = 500,
                        Title = "Internal Server Error",
                        Detail = "An unexpected error occurred."
                    };

                    await context.Response.WriteAsJsonAsync(problem);
                });
            });

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
