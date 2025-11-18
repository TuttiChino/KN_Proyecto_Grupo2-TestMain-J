using ProyectoFinal.EF;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoFinal.Controllers
{
    public class PacientesController : Controller
    {
        // Verificar que solo Admin tenga acceso
        private bool EsAdmin()
        {
            var perfil = Session["PerfilUsuario"] as string ?? "";
            return perfil.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        // GET: Pacientes
        public ActionResult Index(string buscar = "")
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new BDCitasMedicasEntities())
            {
                var perfilPaciente = db.tbPerfils
                    .Where(p => p.Nombre == "Paciente")
                    .FirstOrDefault();

                if (perfilPaciente == null)
                {
                    ViewBag.Error = "No se encontró el perfil de Paciente.";
                    return View();
                }

                var query = from u in db.tbUsuarios
                           where u.ConsecutivoPerfil == perfilPaciente.ConsecutivoPerfil
                           select u;

                // Búsqueda por nombre, identificación o correo
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    buscar = buscar.Trim();
                    query = query.Where(u => 
                        u.Nombre.Contains(buscar) ||
                        u.Identificacion.Contains(buscar) ||
                        u.CorreoElectronico.Contains(buscar)
                    );
                }

                var pacientes = query
                    .OrderBy(u => u.Nombre)
                    .ToList();

                ViewBag.Buscar = buscar;
                return View(pacientes);
            }
        }

        // GET: Pacientes/Crear
        public ActionResult Crear()
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Pacientes/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(string identificacion, string nombre, string correoElectronico, string contrasenna, bool estado = true)
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            // Validaciones
            if (string.IsNullOrWhiteSpace(identificacion))
            {
                ViewBag.Error = "La identificación es requerida.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                ViewBag.Error = "El nombre es requerido.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(correoElectronico))
            {
                ViewBag.Error = "El correo electrónico es requerido.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(contrasenna))
            {
                ViewBag.Error = "La contraseña es requerida.";
                return View();
            }

            // Validar formato de correo básico
            if (!correoElectronico.Contains("@") || !correoElectronico.Contains("."))
            {
                ViewBag.Error = "El formato del correo electrónico no es válido.";
                return View();
            }

            try
            {
                using (var db = new BDCitasMedicasEntities())
                {
                    // Validar que la identificación no exista
                    if (db.tbUsuarios.Any(u => u.Identificacion == identificacion.Trim()))
                    {
                        ViewBag.Error = "Ya existe un usuario con esa identificación.";
                        return View();
                    }

                    // Validar que el correo no exista
                    if (db.tbUsuarios.Any(u => u.CorreoElectronico == correoElectronico.Trim()))
                    {
                        ViewBag.Error = "Ya existe un usuario con ese correo electrónico.";
                        return View();
                    }

                    // Obtener perfil de Paciente
                    var perfilPaciente = db.tbPerfils
                        .Where(p => p.Nombre == "Paciente")
                        .FirstOrDefault();

                    if (perfilPaciente == null)
                    {
                        ViewBag.Error = "No se encontró el perfil de Paciente.";
                        return View();
                    }

                    var nuevoPaciente = new tbUsuario
                    {
                        Identificacion = identificacion.Trim(),
                        Nombre = nombre.Trim(),
                        CorreoElectronico = correoElectronico.Trim().ToLower(),
                        Contrasenna = contrasenna,
                        Estado = estado,
                        ConsecutivoPerfil = perfilPaciente.ConsecutivoPerfil
                    };

                    db.tbUsuarios.Add(nuevoPaciente);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al crear el paciente: " + ex.Message;
                return View();
            }
        }

        // GET: Pacientes/Editar/5
        public ActionResult Editar(int? id)
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            using (var db = new BDCitasMedicasEntities())
            {
                var paciente = db.tbUsuarios.Find(id);
                if (paciente == null)
                {
                    ViewBag.Error = "Paciente no encontrado.";
                    return RedirectToAction("Index");
                }

                // Verificar que sea un paciente
                var perfilPaciente = db.tbPerfils
                    .Where(p => p.Nombre == "Paciente")
                    .FirstOrDefault();

                if (paciente.ConsecutivoPerfil != perfilPaciente?.ConsecutivoPerfil)
                {
                    ViewBag.Error = "El usuario no es un paciente.";
                    return RedirectToAction("Index");
                }

                return View(paciente);
            }
        }

        // POST: Pacientes/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(int id, string nombre, string correoElectronico, string contrasenna, bool? estado)
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            // Validaciones
            if (string.IsNullOrWhiteSpace(nombre))
            {
                ViewBag.Error = "El nombre es requerido.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(correoElectronico))
            {
                ViewBag.Error = "El correo electrónico es requerido.";
                return View();
            }

            // Validar formato de correo básico
            if (!correoElectronico.Contains("@") || !correoElectronico.Contains("."))
            {
                ViewBag.Error = "El formato del correo electrónico no es válido.";
                return View();
            }

            try
            {
                using (var db = new BDCitasMedicasEntities())
                {
                    var paciente = db.tbUsuarios.Find(id);
                    if (paciente == null)
                    {
                        ViewBag.Error = "Paciente no encontrado.";
                        return RedirectToAction("Index");
                    }

                    // Validar que el correo no esté en uso por otro usuario
                    if (db.tbUsuarios.Any(u => u.CorreoElectronico == correoElectronico.Trim() && u.ConsecutivoUsuario != id))
                    {
                        ViewBag.Error = "Ya existe otro usuario con ese correo electrónico.";
                        return View(paciente);
                    }

                    // Actualizar datos (no permitir cambiar identificación)
                    paciente.Nombre = nombre.Trim();
                    paciente.CorreoElectronico = correoElectronico.Trim().ToLower();
                    paciente.Estado = estado ?? false; // Si es null, asumir false

                    // Solo actualizar contraseña si se proporcionó una nueva
                    if (!string.IsNullOrWhiteSpace(contrasenna))
                    {
                        paciente.Contrasenna = contrasenna;
                    }

                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al actualizar el paciente: " + ex.Message;
                using (var db = new BDCitasMedicasEntities())
                {
                    var paciente = db.tbUsuarios.Find(id);
                    return View(paciente);
                }
            }
        }

        // GET: Pacientes/Detalles/5
        public ActionResult Detalles(int? id)
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            using (var db = new BDCitasMedicasEntities())
            {
                var paciente = db.tbUsuarios.Find(id);
                if (paciente == null)
                {
                    ViewBag.Error = "Paciente no encontrado.";
                    return RedirectToAction("Index");
                }

                // Verificar que sea un paciente
                var perfilPaciente = db.tbPerfils
                    .Where(p => p.Nombre == "Paciente")
                    .FirstOrDefault();

                if (paciente.ConsecutivoPerfil != perfilPaciente?.ConsecutivoPerfil)
                {
                    ViewBag.Error = "El usuario no es un paciente.";
                    return RedirectToAction("Index");
                }

                // Obtener citas del paciente
                var citas = db.tbCitas
                    .Where(c => c.ConsecutivoPaciente == id)
                    .OrderByDescending(c => c.Fecha)
                    .ThenByDescending(c => c.HoraInicio)
                    .ToList();

                ViewBag.Citas = citas;

                // Estadísticas básicas
                ViewBag.TotalCitas = citas.Count;
                ViewBag.CitasProgramadas = citas.Count(c => c.Estado == "Programada" || c.Estado == "Reprogramada");
                ViewBag.CitasCompletadas = citas.Count(c => c.Estado == "Completada");
                ViewBag.CitasCanceladas = citas.Count(c => c.Estado == "Cancelada");

                return View(paciente);
            }
        }
    }
}

