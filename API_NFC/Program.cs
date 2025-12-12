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
// DB CONNECTION: SOLO POSTGRESQL (Render + Local)
// ------------------------------------------------------

string connectionUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

string connectionString;

if (!string.IsNullOrEmpty(connectionUrl))
{
    var uri = new Uri(connectionUrl);

    var userInfo = uri.UserInfo.Split(':');

    connectionString =
        $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
        $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=True;";
}
else
{
    // Local (usa lo que tengas en appsettings.json)
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ------------------------------------------------------
// SERVICES
// ------------------------------------------------------

builder.Services.AddRazorPages();

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddScoped<ProgramaImportService>();
builder.Services.AddScoped<FichaImportService>();
builder.Services.AddScoped<AprendizImportService>();
builder.Services.AddScoped<UsuarioImportService>();
builder.Services.AddScoped<ImportServiceFactory>();

// ------------------------------------------------------
// JWT CONFIG
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

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapHub<NfcHub>("/nfcHub");

app.MapGet("/", (HttpContext ctx) =>
{
    ctx.Response.Redirect("/Login");
    return Task.CompletedTask;
});

app.Run();