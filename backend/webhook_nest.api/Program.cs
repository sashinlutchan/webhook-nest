using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using webhook_nest.api.Interfaces;
using webhook_nest.api.Repository;
using webhook_nest.api.Services;
using Amazon.Runtime;

namespace webhook_nest.api;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;
            });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();


        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });


        services.AddSingleton<IAmazonDynamoDB>(provider =>
        {
            var region = Environment.GetEnvironmentVariable("REGION") ?? "af-south-1";
            var regionEndpoint = RegionEndpoint.GetBySystemName(region);

            var logger = provider.GetService<ILogger<Startup>>();
            logger?.LogInformation("Creating DynamoDB client for region: {Region}", region);

            var client = new AmazonDynamoDBClient(regionEndpoint);

            logger?.LogInformation("DynamoDB client created successfully");

            return client;
        });

        services.AddSingleton<IDynamoDBContext>(provider =>
        {
            var dynamoDbClient = provider.GetRequiredService<IAmazonDynamoDB>();
            var context = new DynamoDBContext(dynamoDbClient);

            var logger = provider.GetService<ILogger<Startup>>();
            logger?.LogInformation("DynamoDB context created successfully");

            return context;
        });


        services.AddScoped<IHook, HookRepositoryUsingDynamodb>();
        services.AddScoped<IWebHook, WebHookService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetService<ILogger<Startup>>();
                logger?.LogError(ex, "Unhandled exception in request pipeline");

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    error = "Internal server error",
                    message = ex.Message,
                    details = env.IsDevelopment() ? ex.ToString() : null
                };

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
            }
        });


        app.UseCors();

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
