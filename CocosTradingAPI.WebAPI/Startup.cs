using CocosTradingAPI.Application.Interfaces;
using CocosTradingAPI.Application.Services;
using CocosTradingAPI.Infrastructure.Data;
using CocosTradingAPI.Infrastructure.Repositories;
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

            services.AddScoped<IInstrumentRepository, InstrumentRepository>();
            services.AddScoped<IPortfolioService, PortfolioService>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IMarketDataRepository, MarketDataRepository>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
