
using api.painless.events.Core;
using api.painless.events.Hubs;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Text;
using Quartz;
using SmtpServer;
using Microsoft.OpenApi.Models;

namespace api.painless.events
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddDbContext<WriteContext>(options =>
            {
                var connectionString = builder.Configuration.GetSection("ConnectionStrings:WriteConnection").Value;
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), null);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
            builder.Services.AddDbContext<ReadContext>(options =>
            {
                var connectionString = builder.Configuration.GetSection("ConnectionStrings:ReadConnection").Value;
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), null);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            builder.Services.AddScoped<SocketHub>();
            builder.Services.AddScoped<SecurityContext>();
            builder.Services.AddScoped<HelperFunctions>();


            builder.Services.AddQuartz(q =>
            { 
                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 10;
                });
                var jobM = new JobKey("CronMinutely");
                var jobD = new JobKey("CronDaily");
                q.AddJob<CronMinutely>(opts => opts.WithIdentity(jobM));
                q.AddJob<CronDaily>(opts => opts.WithIdentity(jobD));
                q.AddTrigger(opts => opts.ForJob(jobM).WithIdentity("CronMinutelyTrigger").WithCronSchedule("0 0/1 * 1/1 * ? *"));
                q.AddTrigger(opts => opts.ForJob(jobD).WithIdentity("CronDailyTrigger").WithCronSchedule("0 10 6 1/1 * ? *"));
            });
            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = false);


            builder.Services.AddApiVersioning(options => 
            { 
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
            }).AddMvc().AddApiExplorer(options => 
            { 
                options.SubstituteApiVersionInUrl = true;
            });


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSignalR(config => 
            { 
                config.MaximumParallelInvocationsPerClient = 10;
            }).AddMessagePackProtocol();


            var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
            builder.Services.AddSwaggerGen(options => 
            {
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \" bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                options.OperationFilter<SecurityRequirementsOperationFilter>();
                options.AddSignalRSwaggerGen();
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, new Microsoft.OpenApi.Models.OpenApiInfo
                    {
                        Title = $"Painless Events API {description.ApiVersion}",
                        Version = description.ApiVersion.ToString(),
                        Description = "This is the API for Painless Events. It will allow you to manage events and attendees.",
                    });
                }
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });



            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = builder.Configuration.GetSection("Appsettings:Issuer").Value,
                            ValidateAudience = true,
                            ValidAudience = builder.Configuration.GetSection("Appsettings:Audience").Value,
                            ValidateIssuerSigningKey = true,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Appsettings:Token").Value)),
                        };
                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var accessToken = context.Request.Query["access_token"];
                                var path = context.HttpContext.Request.Path;
                                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/socket"))
                                {
                                    context.Token = accessToken;
                                }
                                return Task.CompletedTask;
                            }
                        };
                    });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyCorsPolicy", builder =>
                {
                    builder.WithOrigins(
                        "http://localhost:5173",
                        "https://localhost:5173",
                        "http://painless.events",
                        "https://painless.events",
                        "http://*.painless.events",
                        "https://*.painless.events"
                        )
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed((host) => true)
                    .AllowCredentials();
                });
            });


            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("TenInTwenty", opt =>
                {
                    opt.Window = TimeSpan.FromSeconds(20);
                    opt.PermitLimit = 10;
                    opt.QueueLimit = 0;
                    //opt.QueueLimit = 10;
                    //opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                }).RejectionStatusCode = 429;
            });


            builder.Services.AddSingleton<SmtpServer.SmtpServer>(
                provider =>
                {
                    var options = new SmtpServerOptionsBuilder()
                        .ServerName("painlessevents.com")
                        .Port(25, 587)
                        .Build();
                    var serviceProvider = provider.GetRequiredService<IServiceProvider>();
                    return new SmtpServer.SmtpServer(options, serviceProvider);
                }
            );
            builder.Services.AddHostedService<SmtpWorker>();









            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
            }

            app.UseSwagger(options => 
            { 
                options.RouteTemplate = "{documentName}/swagger.json";
            });
            app.UseSwaggerUI(options => 
            {
                var descriptions = app.DescribeApiVersions();
                descriptions.Reverse();
                foreach (var description in descriptions)
                {
                    options.SwaggerEndpoint($"/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
                options.DefaultModelsExpandDepth(-1);
                options.RoutePrefix = string.Empty;
                options.DocumentTitle = "Painless Events API";
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRateLimiter();
            app.UseCors("MyCorsPolicy");


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<SocketHub>("/socket", options => 
            { 
                options.ApplicationMaxBufferSize = 1024 * 1024;
                options.TransportMaxBufferSize = 1024 * 1024;
            });

            app.MapControllers();

            app.Run();
        }
    }
}
