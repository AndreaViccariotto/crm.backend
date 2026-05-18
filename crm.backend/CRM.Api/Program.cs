using crm.backend.CRM.Application.Services;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Serilog;
using Microsoft.AspNetCore.Diagnostics;


// Serilog initialization
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(AppContext.BaseDirectory, "logs/log-.txt"),
        rollingInterval: RollingInterval.Day)
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

// LOGS
builder.Host.UseSerilog();

// SERVICES
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ContactService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<TaskStatusService>();
builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<RoleService>();

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default"))
    ));

var jwtKey = builder.Configuration["Jwt:Key"];

// AUTH
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ClockSkew = TimeSpan.Zero,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CRM API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Inserisci il token JWT: Bearer {token}"
    });
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Authorization", "Content-Type");
    });
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();

var app = builder.Build();

// ERROR HANDLING
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        if (exception != null)
        {
            logger.LogError(exception, "Errore non gestito");
        }

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            System.Text.Json.JsonSerializer.Serialize(new
            {
                message = "Errore interno"
            }));
    });
});

app.UseSerilogRequestLogging();

app.UseCors("CorsPolicy");

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
