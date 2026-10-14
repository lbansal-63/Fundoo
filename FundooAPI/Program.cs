using BusinessLayer.Interfaces;
using BusinessLayer.Services;
using BusinessLayer.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RepositoryLayer.Context;
using RepositoryLayer.Interfaces;
using RepositoryLayer.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json.Serialization;
using System.Text;
using System.Threading.RateLimiting;
using FundooAPI.ExceptionHandling;
using System.Security.Claims;

namespace FundooAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. CORS Configuration
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? new[] { "http://localhost:5173" };

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // 2. Controllers and Swagger
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            builder.Services.AddControllers()
                .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Sirf token paste karo"
                    });

                c.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
            });

            // 3. PostgreSQL Database
            builder.Services.AddDbContext<FundooContext>(options =>
                options.UseNpgsql(
                    builder.Configuration
                        .GetConnectionString("Default")));

            // 4. Dependency Injection
            builder.Services.AddScoped<INoteRepository, NoteRepository>();
            builder.Services.AddScoped<INoteBusiness, NoteBusiness>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserBusiness, UserBusiness>();
            builder.Services.AddScoped<EmailHelper>();

            // 5. JWT Authentication
            var jwtKey = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
                throw new InvalidOperationException("Jwt:Key missing ya 32 chars se chhoti hai.");

            var jwtIssuer = builder.Configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer missing.");
            var jwtAudience = builder.Configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience missing.");

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtIssuer,
                        ValidateAudience = true,
                        ValidAudience = jwtAudience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                        ClockSkew = TimeSpan.Zero,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = ctx =>
                        {
                            var repo = ctx.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                            var idClaim = ctx.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                            var tvClaim = ctx.Principal?.FindFirst("tv")?.Value;

                            if (!int.TryParse(idClaim, out var userId)
                                || !int.TryParse(tvClaim, out var tokenVersion)
                                || repo.GetTokenVersion(userId) != tokenVersion)
                            {
                                ctx.Fail("Token revoked or invalid");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            // 5b. Rate limiting
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("auth", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));
            });

            var app = builder.Build();
            app.UseExceptionHandler();

            // 6. Swagger
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 7. Reverse Proxy Forwarded Headers
            app.UseForwardedHeaders(
                new ForwardedHeadersOptions
                {
                    ForwardedHeaders =
                        ForwardedHeaders.XForwardedFor |
                        ForwardedHeaders.XForwardedProto
                });

            // 8. HTTPS Redirection (+ HSTS outside development)
            if (!app.Environment.IsDevelopment())
                app.UseHsts();
            app.UseHttpsRedirection();

            // 9. CORS Middleware
            app.UseCors("AllowReactApp");

            // 10. Rate limiter, then Authentication and Authorization
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            // 11. Map API Controllers
            app.MapControllers();

            app.Run();
        }
    }
}