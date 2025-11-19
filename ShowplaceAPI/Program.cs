using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using ShowplaceAPI.Models;
using System.Reflection;
using System.Text;
using Npgsql;
using ShowplaceAPI.Repositories.Interfaces;
using ShowplaceAPI.Repositories;
using ShowplaceAPI.Sevices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
namespace ShowplaceAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddAutoMapper(typeof(Program));

            // Configure Swagger
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Landmark Review System API",
                    Description = "API для системы отзывов о достопримечательностях",
                    Contact = new OpenApiContact
                    {
                        Name = "Кантурлеев Арман",
                        Email = "armankantuleev6551@gmail.com",
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });

                // Add JWT Bearer token authentication
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter your token in the text input below.
                      Example: 'Bearer {token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

                // Enable XML comments for Swagger
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            // Database Configuration
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreConnection")));

            // JWT Authentication Configuration
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
            {
                // Fallback for development - in production this should be set in environment variables
                secretKey = "armanarmanarmanarmanarmanarmanarmanarmanarmanarmanarmanarmanarmanarmanarmanarmaarmann";
            }

            var key = Encoding.UTF8.GetBytes(secretKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"] ?? "ShowplaceAPI",
                    ValidAudience = jwtSettings["Audience"] ?? "ShowplaceAPIClient",
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // Remove clock skew for exact expiration validation
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated successfully");
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy =>
                    policy.RequireRole("Admin"));
                options.AddPolicy("RequireUserRole", policy =>
                    policy.RequireRole("User", "Admin"));
            });

            // Add repositories
            builder.Services.AddScoped<ILandmarkRepository, LandmarkRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            // Add services
            builder.Services.AddScoped<IAuthService, AuthService>();

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Initialize database
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Автоматическое применение миграций
                context.Database.Migrate();
            }

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Landmark Review System v1");
                    options.RoutePrefix = "swagger"; // Set Swagger UI at the app's root
                    options.DocumentTitle = "Landmark Review System API";
                    options.EnablePersistAuthorization();

                    // Enable OAuth2 in Swagger UI
                    options.OAuthClientId("swagger-ui");
                    options.OAuthScopes("api");
                    options.OAuthUsePkce();
                });
            }

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();

            // Authentication & Authorization middleware (ВАЖНО: в правильном порядке)
            app.UseAuthentication(); // Должно быть перед UseAuthorization
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}