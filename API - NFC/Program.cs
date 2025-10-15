using Microsoft.EntityFrameworkCore;
using API___NFC.Data;
using API___NFC.Hubs;
using Microsoft.AspNetCore.Identity; // <-- AÑADIDO: Necesario para Identity
using Microsoft.AspNetCore.Authentication.Cookies; // <-- AÑADIDO: Necesario para la autenticación por Cookies

var builder = WebApplication.CreateBuilder(args);
//Comentario x de persona x
// --- SECCIÓN DE SERVICIOS ---

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Controlador con manejo de ciclos de referencia
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// --- INICIO DE LA SECCIÓN AÑADIDA ---



// 2. Configuración del esquema de autenticación
// Le dice a la aplicación CÓMO verificar si un usuario ha iniciado sesión.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Ruta a la que se redirigirá si un usuario no autenticado intenta acceder a un recurso protegido.
        // Asegúrate que esta ruta sea correcta para tu proyecto (usualmente está en /Areas/Identity/Pages/Account/Login)
        options.LoginPath = "/Identity/Account/Login";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

// --- FIN DE LA SECCIÓN AÑADIDA ---

builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(origin => true)
              .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- SECCIÓN DE CONFIGURACIÓN DE LA APP (MIDDLEWARE) ---
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRouting();
app.UseStaticFiles();

// --- LÍNEAS CORREGIDAS Y ORDENADAS ---
// Es crucial que el orden sea este:
app.UseAuthentication(); // <-- PRIMERO, identifica quién es el usuario a través de la cookie.
app.UseAuthorization();  // <-- SEGUNDO, una vez identificado, verifica si tiene permisos.


app.MapControllers();
app.MapHub<NfcHub>("/nfcHub");
app.MapRazorPages();

app.Run();

