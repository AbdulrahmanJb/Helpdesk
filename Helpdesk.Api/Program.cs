using Helpdesk.Application.Interfaces;
using Helpdesk.Application.Services;
using Helpdesk.Infrastructure.Data;
using Helpdesk.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<HelpdeskDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                ));

            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddScoped<ITicketRepository, TicketRepository>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}