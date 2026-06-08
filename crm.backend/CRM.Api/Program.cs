using crm.backend.CRM.Application.Services;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Serilog;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "logs/log-.txt"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ContactService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<TaskStatusService>();
builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<ArticleService>();
builder.Services.AddScoped<QuoteService>();
builder.Services.AddScoped<SalesOrderService>();
builder.Services.AddScoped<PurchaseOrderService>();
builder.Services.AddScoped<GeneralSettingsService>();
builder.Services.AddScoped<CustomFieldService>();
builder.Services.AddScoped<CommercialAutomationService>();
builder.Services.AddScoped<CommercialDashboardService>();
builder.Services.AddScoped<CommercialNotificationService>();
builder.Services.AddScoped<AccessControlService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<InterventionService>();
builder.Services.AddHttpContextAccessor();

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:Default non configurata.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key deve essere configurata con almeno 32 caratteri.");

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
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

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .WithMethods("GET", "POST", "PUT", "DELETE")
                  .WithHeaders("Authorization", "Content-Type");
        }
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanDeleteCrm", policy => policy.RequireClaim("permission", "crm.delete"));
});

builder.Services.AddControllers();
var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:Initialize"))
    await DatabaseInitializer.InitializeAsync(app.Services, app.Configuration, app.Logger);

app.UseForwardedHeaders();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        var unauthorized = exception is UnauthorizedAccessException;
        if (unauthorized)
            logger.LogWarning("Tentativo di accesso con credenziali non valide");
        else if (exception != null)
            logger.LogError(exception, "Errore non gestito");
        context.Response.StatusCode = unauthorized
            ? StatusCodes.Status401Unauthorized
            : StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            message = unauthorized ? exception!.Message : "Errore interno"
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
app.MapGet("/health", async (AppDbContext db) =>
    await db.Database.CanConnectAsync()
        ? Results.Ok(new { status = "healthy" })
        : Results.Problem("Database non raggiungibile"));
app.Run();
