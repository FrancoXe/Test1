using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using LogeoV2.Data;
using LogeoV2.Models;
using LogeoV2.ViewModels;  
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using LogeoV2.Services;

namespace LogeoV2.Controllers
{
    public class AccesoController : Controller
    {
        private readonly AppDBContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly ILogger<AccesoController> _logger;
        private readonly IRolService _rolService;

        public AccesoController(
            AppDBContext context, 
            IPasswordHasher<Usuario> passwordHasher,
            ILogger<AccesoController> logger,
            IRolService rolService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _rolService = rolService;
        }

        [HttpGet]
        public IActionResult Registrarse()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrarse(UsuarioVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try 
            {
                var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Correo == model.Correo);
                if (usuarioExiste)
                {
                    ModelState.AddModelError("Correo", "Este correo ya está registrado");
                    return View(model);
                }

                // Usar servicio de roles para obtener rol de Vecino
                var rol = await _rolService.ObtenerRolPorNombre("Vecino");
                if (rol == null)
                {
                    // Si no existe, crearlo
                    rol = await _rolService.CrearRolSiNoExiste("Vecino");
                }

                // Validar que las contraseñas coincidan
                if (model.Clave != model.ConfirmarClave)
                {
                    ModelState.AddModelError("ConfirmarClave", "Las contraseñas no coinciden");
                    return View(model);
                }

                var usuario = new Usuario
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Correo = model.Correo,
                    IdRol = rol.IdRol,
                    Rol = rol
                };

                // Hash the password
                usuario.Clave = _passwordHasher.HashPassword(usuario, model.Clave);

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Usuario registrado: {usuario.Correo} con rol {rol.NombreRol}");

                // Usar TempData para mostrar mensaje de éxito en la vista de Login
                TempData["RegistroExitoso"] = "¡Cuenta creada exitosamente! Inicie sesión para continuar.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Loguear el error detallado con excepción interna
                _logger.LogError(ex, "Error durante el registro de usuario");
                
                // Obtener detalles de la excepción interna
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    _logger.LogError(innerException, "Excepción interna: {Message}", innerException.Message);
                    innerException = innerException.InnerException;
                }

                ModelState.AddModelError("", $"Ocurrió un error durante el registro: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == model.Correo);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Las credenciales son incorrectas");
                return View(model);
            }

            // Verify password
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(usuario, usuario.Clave, model.Clave);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Las credenciales son incorrectas");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Surname, usuario.Apellido),
                new Claim("Correo", usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol.NombreRol)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = model.RecordarMe,
                ExpiresUtc = model.RecordarMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), 
                authProperties);

            usuario.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }


    }
}
