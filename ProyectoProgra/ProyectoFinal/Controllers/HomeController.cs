using ProyectoFinal.EF;
using ProyectoFinal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoFinal.Controllers
{
    public class HomeController : Controller
    {
        // POST: /Home/Index  -> Procesa login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string correo, string contrasenna)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasenna))
            {
                ViewBag.Error = "Ingrese correo y contraseña.";
                return View();
            }

            using (var db = new BDCitasMedicasEntities1())
            {
                var user = (from u in db.tbUsuario
                            join p in db.tbPerfil on u.ConsecutivoPerfil equals p.ConsecutivoPerfil
                            where u.CorreoElectronico == correo
                               && u.Contrasenna == contrasenna
                               && u.Estado == true
                            select new
                            {
                                u.ConsecutivoUsuario,
                                u.Nombre,
                                Perfil = p.Nombre
                            }).FirstOrDefault();

                if (user == null)
                {
                    ViewBag.Error = "Credenciales inválidas o usuario inactivo.";
                    return View();
                }

                // Sesión al estilo profe
                Session["ConsecutivoUsuario"] = user.ConsecutivoUsuario;
                Session["NombreUsuario"] = user.Nombre;
                Session["PerfilUsuario"] = user.Perfil;  // "Admin" | "Doctor" | "Paciente"

                return RedirectToAction("Landing");
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        // Landing que depende del rol 
        public ActionResult Landing()
        {
            var rol = (Session["PerfilUsuario"] as string ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(rol)) return RedirectToAction("Index");

            switch (rol)
            {
                case "admin":
                    return View("LandingAdmin");

                case "doctor":
                    return View("LandingDoctor");

                case "paciente":
                    return View("LandingPaciente");

                default:
                    return RedirectToAction("Index");
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index");
        }

    }
}