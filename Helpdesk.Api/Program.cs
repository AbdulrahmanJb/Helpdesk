using System.Text;
using Helpdesk.Api.Extensions;
using Helpdesk.Application.Interfaces;
using Helpdesk.Application.Services;
using Helpdesk.Infrastructure.Data;
using Helpdesk.Infrastructure.Repositories;
using Helpdesk.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace Helpdesk.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Enter: Bearer {your token}"
                });

                options.AddSecurityRequirement(document =>
                {
                    OpenApiSecuritySchemeReference schemeRef = new("Bearer", document);

                    return new OpenApiSecurityRequirement
                    {
                        [schemeRef] = []
                    };
                });
            });

            builder.Services.AddDbContext<HelpdeskDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<ITicketRepository, TicketRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<IAuditTrailRepository, AuditTrailRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IAuditTrailService, AuditTrailService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IAttachmentService, AttachmentService>();
            builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();

            var jwtKey = builder.Configuration["Jwt:Key"]!;

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseGlobalExceptionHandling();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
