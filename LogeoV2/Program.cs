using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using LogeoV2.Data;
using LogeoV2.Models;
using LogeoV2.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración de logging
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSQL2"));
});

// Agregar password hasher
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Registrar servicio de roles
builder.Services.AddScoped<IRolService, RolService>();

// Registrar servicio de permisos
builder.Services.AddScoped<IPermisoService, PermisoService>();

//registrar servicio de usuarios
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// export service 
builder.Services.AddScoped<IExportService, ExportService>();

// registrar servicio de reclamos
builder.Services.AddScoped<IReclamoService, ReclamoService>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();

// Agregar servicios de autenticación por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Acceso/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

var app = builder.Build();

// Inicializar roles y permisos al arrancar la aplicación
using (var scope = app.Services.CreateScope())
{
    var rolService = scope.ServiceProvider.GetRequiredService<IRolService>();
    var permisoService = scope.ServiceProvider.GetRequiredService<IPermisoService>();
    
    await rolService.InicializarRolesPredeterminados();
    await permisoService.InicializarPermisosPredeterminados();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run();
