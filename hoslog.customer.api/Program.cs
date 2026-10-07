using AmazonCognito.Middleware.Domain.Abstract;
using AmazonCognito.Middleware.Domain.Services;
using Asp.Versioning;
using AspNetCoreRateLimit;
using hoslog.customer.api.Filters;
using hoslog.customer.api.Library;
using hoslog.customer.api.Middleware;
using hoslog.customer.api.Services;
using hoslog.customer.common;
using hoslog.customer.common.Cache;
using hoslog.customer.common.Middleware.AmazonCognito;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.shared;
using hoslog.customer.shared.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Reflection;
using hoslog.customer.api.Extensions;
using Microsoft.OpenApi;
using hoslog.customer.common.Models.Auth;
using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using Amazon;
using hoslog.customer.common.RealTime;
using hoslog.customer.common.Service.SSE;
using hoslog.customer.common.Models.IPService;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using hoslog.customer.common.Service.IP;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Configuration["Environment"];

builder.Services.Configure<CognitoOptions>(
    builder.Configuration.GetSection("AWS:Cognito"));

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.AddHealthChecks();
builder.Services.RegisterBusiness();
builder.Services.RegisterDomain();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ValidationHelper>();
// builder.Services.AddScoped<AuthMiddleware>();// Register AuthMiddleware

var headerKeys = new List<string> { "X-Custom-Header1", "X-Custom-Header2", "X-Custom-Header3" };

var StaticConfig = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new CustomHeaderFilter(
        builder.Services.BuildServiceProvider().GetRequiredService<ILogger<CustomHeaderFilter>>(),
        headerKeys,
        builder.Configuration
    ));
});
builder.Services.AddDomainValidation(builder.Configuration);
// builder.Services.AddScoped<SubDomainNameValidatorFilter>();
builder.Services.AddScoped<SubDomainNameValidatorFilterV2>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo { Title = "Hoslog", Version = "v1" });
    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath))
    {
        x.IncludeXmlComments(xmlPath);
    }
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    };

    x.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });

    x.CustomSchemaIds(type => type.FullName ?? type.Name);
});
AppSettingData objAppSettingData = new AppSettingData();
//builder.Services.Configure<AppSettingsModel>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<SecurityKeyCommon>(builder.Configuration.GetSection("SecurityKey"));
builder.Services.AddTransient<StringCipher>();
builder.Services.AddTransient<CommonAttributeStringCipher>();
builder.Services.AddTransient<URLHelper>();
// builder.Services.AddSingleton<SqlTransactionHandler>();
// builder.Services.AddSingleton<SqlHandler>();
builder.Services.AddScoped<SqlTransactionHandler>();
builder.Services.AddScoped<SqlHandler>();
if (!string.IsNullOrEmpty(environment) &&
    new[] { "development" }.Contains(environment.ToLower()))
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
    });
}
else
{
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    var allowedDomains = builder.Configuration.GetSection("AllowedDomains").Get<string[]>() ?? Array.Empty<string>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            // Allow exact matches
            policy.WithOrigins(allowedOrigins);

            // Allow wildcard domains (subdomains)
            policy.SetIsOriginAllowed(origin =>
            {
                if (origin == null) return false;

                var uri = new Uri(origin);
                // Check if matches configured domain(s)
                return allowedDomains.Any(domain => uri.Host.EndsWith(domain, StringComparison.OrdinalIgnoreCase))
                       || allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
            });

            policy.AllowCredentials()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
}

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0); //same as ApiVersion.Default
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("X-Version"));
})
.AddMvc(options => { })
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(objAppSettingData.GetIpRateLimit);
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddTransient<CognitoService>();
builder.Services.AddTransient<CognitoMiddlewareAbstract>();
builder.Services.AddTransient<AmazonCognitoMiddleware>();
//builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddScoped<ApiResponseMessageService>();
builder.Services.AddTransient<LineAPIService>();
builder.Services.AddTransient<CommonResponseHandler>();
builder.Services.AddTransient<ImageHelper>();
builder.Services.AddInMemoryRateLimiting();

builder.Services.AddAuthentication("BearerAuthentication")
            .AddScheme<AuthenticationSchemeOptions, BearerAuthenticationHandler>("BearerAuthentication", null);
builder.Services.AddAuthorization();

var redisCacheSetting = new RedisCacheSetting();
configuration.GetSection(nameof(RedisCacheSetting)).Bind(redisCacheSetting);
builder.Services.AddSingleton(redisCacheSetting);
if (redisCacheSetting.isEnabled)
{
    // var redisConfiguration = $"{redisCacheSetting.connectionString},password={redisCacheSetting.password}";
    // try
    // {
    //     var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfiguration);

    //     builder.Services.AddStackExchangeRedisCache(options =>
    //     {
    //         options.Configuration = redisConfiguration;
    //         options.InstanceName = redisCacheSetting.instanceName;
    //     });

    //     builder.Services.AddTransient<IResponseCacheService, ResponseCacheService>();
    //     builder.Services.AddTransient<IRedisCacheService, RedisCacheService>();

    //     builder.Services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

    //     builder.Services.AddHostedService<RedisSubscriptionService>();
    // }
    // catch (RedisConnectionException ex)
    // {
    //     Console.WriteLine($"Failed to connect to Redis: {ex.Message}");
    // }

    try
    {
        var redisOptions = new ConfigurationOptions
        {
            AbortOnConnectFail = false,
            Ssl = redisCacheSetting.useSsl,
            SslHost = redisCacheSetting.sslHost,
            ConnectTimeout = redisCacheSetting.connectTimeout,
            SyncTimeout = redisCacheSetting.syncTimeout,
            AllowAdmin = true
        };

        redisOptions.EndPoints.Add(redisCacheSetting.host, redisCacheSetting.port);

        if (!string.IsNullOrWhiteSpace(redisCacheSetting.password))
            redisOptions.Password = redisCacheSetting.password;

        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisOptions);

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = redisOptions;
            options.InstanceName = redisCacheSetting.instanceName;
        });

        builder.Services.AddTransient<IResponseCacheService, ResponseCacheService>();
        builder.Services.AddTransient<IRedisCacheService, RedisCacheService>();

        builder.Services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

        // builder.Services.AddHostedService<RedisSubscriptionService>(); //Uncomment for redis pub/sub service.
    }
    catch (RedisConnectionException ex)
    {
        Console.WriteLine($"Failed to connect to Redis: {ex.Message}");
    }
}
builder.Services.AddScoped<ICognitoAuthService, CognitoAuthService>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
            .ToList();

        var response = new CommonApiResponse
        {
            code = ResponseCodes.Code400_BadRequest,
            message = "Validation failed.",
            errors = errors.Select(errorMessage => new Errors
            {
                errorMessage = errorMessage
            }).ToList()
        };

        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddSingleton<IAmazonCognitoIdentityProvider>(_ =>
{
    var region = builder.Configuration["AWS:Cognito:Region"];
    var accessKey = builder.Configuration["AWS:Credentials:AccessKey"];
    var secretKey = builder.Configuration["AWS:Credentials:SecretKey"];

    if (string.IsNullOrWhiteSpace(region))
        throw new InvalidOperationException("AWS:Cognito:Region is missing.");

    if (
        string.IsNullOrWhiteSpace(accessKey) ||
        string.IsNullOrWhiteSpace(secretKey)
    )
        throw new InvalidOperationException("AWS credentials are missing");

    var credentials = new BasicAWSCredentials(accessKey, secretKey);

    return new AmazonCognitoIdentityProviderClient(
        credentials,
        RegionEndpoint.GetBySystemName(region)
    );
});

builder.Services.AddSingleton<SseHub>();
builder.Services.AddHostedService<VisitNowStatusWatcherWorker>();
builder.Services.AddHostedService<NotificationSseWatcherWorker>();

builder.Services
    .AddOptions<MaxMindOptions>()
    .Bind(
        builder.Configuration.GetSection(MaxMindOptions.sectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(
            options.databasePath
        ), "MaxMind database path is required."
    ).ValidateOnStart();

builder.Services.Configure<DefaultCustomerLocationOptions>(
    builder.Configuration.GetSection(
        DefaultCustomerLocationOptions.SectionName));

builder.Services.Configure<ForwardedHeadersOptions>(
    options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
    });

builder.Services.AddSingleton<IClientIpAddressAccessor,
    ClientIpAddressAccessor>();

builder.Services.AddSingleton<IIpGeolocationService,
    MaxMindIpGeolocationService>();


builder.Services.AddHostedService<RecommendationSseWatcherWorker>();
var app = builder.Build();

app.UseMiddleware<ApiPerformanceMiddleware>();

if (!string.IsNullOrEmpty(environment) &&
    new[] { "development", "uat" }.Contains(environment.ToLower()))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();

// app.UseRouting();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseMiddleware<ExceptionMiddleware>();

app.UseDomainValidation();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.UseStaticFiles();

app.Run();
