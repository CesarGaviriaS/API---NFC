using API___NFC.Hubs;
using API_NFC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using API___NFC.Services;
using API___NFC.Services.Import;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------
// FIX: Enable Legacy Timestamp Behavior for Npgsql
// This prevents errors with 'Kind=Unspecified' dates in PostgreSQL
// ------------------------------------------------------
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);


// ------------------------------------------------------
// DATABASE: Render DATABASE_URL or local connection
// ------------------------------------------------------
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var isDev = builder.Environment.IsDevelopment();
var envDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

// If not in development and we have a DATABASE_URL, use it (Render logic)
if (!isDev && !string.IsNullOrEmpty(envDatabaseUrl))
{
    var uri = new Uri(envDatabaseUrl);
    var userInfo = uri.UserInfo.Split(':');

    connectionString =
        $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
        $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=True;";
}


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ------------------------------------------------------
// SERVICES
// ------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Email + Import
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<ProgramaImportService>();
builder.Services.AddScoped<FichaImportService>();
builder.Services.AddScoped<AprendizImportService>();
builder.Services.AddScoped<UsuarioImportService>();
builder.Services.AddScoped<ImportServiceFactory>();

// ------------------------------------------------------
// JWT AUTHENTICATION
// ------------------------------------------------------
var key = builder.Configuration["Jwt:Key"];

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("AuthToken"))
                {
                    context.Token = context.Request.Cookies["AuthToken"];
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// ------------------------------------------------------
// MIDDLEWARE PIPELINE
// ------------------------------------------------------

// 🔥 Swagger SIEMPRE habilitado (REQUIRED FOR RENDER)
app.UseSwagger();
app.UseSwaggerUI();

// 🔧 Developer exception page SOLO en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// ------------------------------------------------------
// ENDPOINTS
// ------------------------------------------------------

// APIs
app.MapControllers();

// Razor pages
app.MapRazorPages();

// SignalR
app.MapHub<NfcHub>("/nfcHub");

// Default redirect
app.MapGet("/", (HttpContext ctx) =>
{
    ctx.Response.Redirect("/Login");
    return Task.CompletedTask;
});

app.Run();