using BusinessLayer.Interfaces;
using BusinessLayer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Middleware.ExceptionHandling;
using Middleware.Services.Cache;
using Middleware.Services.Email;
using Middleware.Services.Hashing;
using Middleware.Services.Jwt;
using NLog.Web;
using RepositoryLayer.Context;
using RepositoryLayer.Interfaces;
using RepositoryLayer.Services;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using BusinessLayer.User.Queries;
using BusinessLayer.User.Commands;
using BusinessLayer.Notes.Commands;
using BusinessLayer.Notes.Queries;
using Middleware.Services.Google;

namespace FundooAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

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
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "FundooAPI",
                    Version = "v1",
                    Description = "Fundoo Notes backend API: Users, Authentication aur Notes Management.",
                    Contact = new OpenApiContact
                    {
                        Name = "Laksha Bansal",
                        Email = "laksha.bansal7015@email.com",
                        Url = new Uri("https://github.com/lbansal-63")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });

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
            builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
            builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
            builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
            builder.Services.AddSingleton<IGoogleTokenValidator, GoogleTokenValidator>();
            builder.Services.AddScoped<IUserBusiness, UserBusiness>();
            builder.Services.AddScoped<LoginUserHandler>();
            builder.Services.AddScoped<RegisterUserHandler>();
            builder.Services.AddScoped<ForgotPasswordHandler>();
            builder.Services.AddScoped<ResetPasswordHandler>(); 
            builder.Services.AddScoped<AddNoteHandler>();
            builder.Services.AddScoped<EditNoteHandler>();
            builder.Services.AddScoped<DeleteNoteHandler>();
            builder.Services.AddScoped<ArchiveNoteHandler>();
            builder.Services.AddScoped<TrashNoteHandler>();
            builder.Services.AddScoped<GetNotesHandler>();
            builder.Services.AddScoped<GetNoteByIdHandler>();


            // 4b. Caching (Redis ya in-memory, Redis:ConnectionString ke hisaab se)
            builder.Services.AddAppCaching(builder.Configuration);

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
                            var cache = ctx.HttpContext.RequestServices.GetRequiredService<ICacheService>();
                            var idClaim = ctx.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                            var tvClaim = ctx.Principal?.FindFirst("tv")?.Value;

                            if (!int.TryParse(idClaim, out var userId)
                                || !int.TryParse(tvClaim, out var tokenVersion))
                            {
                                ctx.Fail("Token revoked or invalid");
                                return Task.CompletedTask;
                            }

                            // Pehle Redis se token version, na mile to DB se aur cache kar do
                            var key = CacheKeys.TokenVersion(userId);
                            var current = cache.Get<int?>(key);
                            if (current == null)
                            {
                                current = repo.GetTokenVersion(userId);
                                if (current != null)
                                    cache.Set(key, current.Value, TimeSpan.FromMinutes(5));
                            }

                            if (current != tokenVersion)
                                ctx.Fail("Token revoked or invalid");

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