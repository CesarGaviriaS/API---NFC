using Microsoft.EntityFrameworkCore;
using API___NFC.Data;
using API___NFC.Hubs;
using Microsoft.AspNetCore.Identity; // <-- A�ADIDO: Necesario para Identity
using Microsoft.AspNetCore.Authentication.Cookies; // <-- A�ADIDO: Necesario para la autenticaci�n por Cookies

var builder = WebApplication.CreateBuilder(args);
//Comentario x de persona x
// --- SECCI�N DE SERVICIOS ---

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Controlador con manejo de ciclos de referencia
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// --- INICIO DE LA SECCI�N A�ADIDA ---



// 2. Configuraci�n del esquema de autenticaci�n
// Le dice a la aplicaci�n C�MO verificar si un usuario ha iniciado sesi�n.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Ruta a la que se redirigir� si un usuario no autenticado intenta acceder a un recurso protegido.
        // Aseg�rate que esta ruta sea correcta para tu proyecto (usualmente est� en /Areas/Identity/Pages/Account/Login)
        options.LoginPath = "/Identity/Account/Login";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

// --- FIN DE LA SECCI�N A�ADIDA ---

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

// --- SECCI�N DE CONFIGURACI�N DE LA APP (MIDDLEWARE) ---
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

// --- L�NEAS CORREGIDAS Y ORDENADAS ---
// Es crucial que el orden sea este:
app.UseAuthentication(); // <-- PRIMERO, identifica qui�n es el usuario a trav�s de la cookie.
app.UseAuthorization();  // <-- SEGUNDO, una vez identificado, verifica si tiene permisos.


app.MapControllers();
app.MapHub<NfcHub>("/nfcHub");
app.MapRazorPages();

app.Run();

