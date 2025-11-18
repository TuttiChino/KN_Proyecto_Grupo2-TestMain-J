using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoFinal.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin/Medicos
        public ActionResult Medicos()
        {
            return View();
        }

        // GET: Admin/Especialidades
        public ActionResult Especialidades()
        {
            return View();
        }

        // GET: Admin/Horarios
        public ActionResult Horarios()
        {
            return View();
        }

        // GET: Admin/Usuarios
        public ActionResult Usuarios()
        {
            return View();
        }

        // GET: Admin/Reportes
        public ActionResult Reportes()
        {
            return View();
        }
    }
}

