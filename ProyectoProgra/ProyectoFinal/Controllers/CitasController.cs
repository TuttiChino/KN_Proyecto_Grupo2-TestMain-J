using ProyectoFinal.Models;
using ProyectoFinal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ProyectoFinal.Controllers
{
    public class CitasController : Controller
    {
        private readonly CitaService _service = new CitaService();
        private readonly NotificacionService _notifier = new NotificacionService();

        // ============================================================
        // ✅ Validación de sesión: ningún usuario sin login puede acceder
        // ============================================================
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["ConsecutivoUsuario"] == null)
            {
                filterContext.Result = RedirectToAction("Index", "Home");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        // ============================================================
        // 🏠 Página principal del módulo de Citas
        // ============================================================
        public ActionResult MainC()
        {
            try
            {
                var citas = _service.ListarCitas();
                return View(citas);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al cargar las citas: {ex.Message}";
                return View(new System.Collections.Generic.List<dynamic>());
            }
        }

        // ============================================================
        // 📋 Citas activas del usuario actual
        // ============================================================
        [HttpGet]
        public ActionResult CitasActivas()
        {
            try
            {
                var perfil = (Session["PerfilUsuario"] as string ?? "").ToLower();
                int.TryParse((Session["ConsecutivoUsuario"] ?? "0").ToString(), out int userId);

                var todas = _service.ListarCitas();
                var activas = todas.Where(c => !string.Equals(c.Estado, "Cancelada", StringComparison.OrdinalIgnoreCase));

                if (perfil == "paciente")
                    activas = activas.Where(c => c.IdPaciente == userId);
                else if (perfil == "doctor")
                    activas = activas.Where(c => c.IdDoctor == userId);

                return View(activas.OrderByDescending(c => c.FechaHoraInicio).ToList());
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al obtener las citas activas: {ex.Message}";
                return View(new System.Collections.Generic.List<dynamic>());
            }
        }

        // ============================================================
        // ➕ GET - Crear nueva cita
        // ============================================================
        [HttpGet]
        public ActionResult CrearCita()
        {
            try
            {
                if (Session["ConsecutivoUsuario"] == null)
                    return RedirectToAction("Index", "Home");

                ViewBag.PacienteId = Convert.ToInt32(Session["ConsecutivoUsuario"]);
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al cargar formulario: {ex.Message}";
                return View();
            }
        }

        // ============================================================
        // 💾 POST - Crear cita
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearCita(int DoctorId, int PacienteId, DateTime FechaInicio, DateTime FechaFin, string Motivo)
        {
            try
            {
                // Validar sesión del paciente
                int idSesion = Convert.ToInt32(Session["ConsecutivoUsuario"]);
                if (idSesion != PacienteId)
                {
                    ModelState.AddModelError("", "El ID del paciente no coincide con la sesión actual.");
                    return View();
                }

                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("", "Verifique los datos ingresados.");
                    return View();
                }

                var res = _service.CrearCita(DoctorId, PacienteId, FechaInicio, FechaFin, Motivo);

                if (res.Codigo == 1)
                {
                    TempData["Success"] = res.Mensaje;
                    return RedirectToAction("MainC");
                }

                ModelState.AddModelError("", res.Mensaje);
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al registrar la cita: {ex.Message}");
                return View();
            }
        }

        // ============================================================
        // ✏️ GET - Editar cita existente
        // ============================================================
        [HttpGet]
        public ActionResult EditarCita(int id)
        {
            try
            {
                var cita = _service.ListarCitas().FirstOrDefault(c => c.IdCita == id);
                if (cita == null)
                    return HttpNotFound();

                return View(cita);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al cargar la cita: {ex.Message}";
                return RedirectToAction("MainC");
            }
        }

        // ============================================================
        // 💾 POST - Editar cita
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarCita(int IdCita, int DoctorId, int PacienteId, DateTime FechaInicio, DateTime FechaFin, string Motivo)
        {
            try
            {
                var res = _service.EditarCita(IdCita, DoctorId, PacienteId, FechaInicio, FechaFin, Motivo);

                if (res.Codigo == 1)
                {
                    TempData["Success"] = res.Mensaje;
                    return RedirectToAction("MainC");
                }

                ModelState.AddModelError("", res.Mensaje);
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al editar la cita: {ex.Message}");
                return View();
            }
        }

        // ============================================================
        // ❌ POST - Cancelar cita
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancelar(int IdCita, string MotivoCancelacion = null)
        {
            try
            {
                // Si el motivo no se envió desde el formulario, se asigna por defecto:
                if (string.IsNullOrWhiteSpace(MotivoCancelacion))
                    MotivoCancelacion = "Cancelada por el usuario.";

                var res = _service.CancelarCita(IdCita, MotivoCancelacion);

                if (res.Codigo == 1)
                {
                    TempData["Success"] = res.Mensaje;
                    return RedirectToAction("CitasActivas");
                }

                TempData["Error"] = res.Mensaje;
                return RedirectToAction("CitasActivas");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cancelar la cita: {ex.Message}";
                return RedirectToAction("CitasActivas");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarcarAtendida(int IdCita)
        {
            var res = _service.MarcarAtendida(IdCita);

            if (res.Codigo == 1)
            {
                TempData["Success"] = res.Mensaje;
            }
            else
            {
                TempData["Error"] = res.Mensaje;
            }

            return RedirectToAction("CitasDoctor");
        }

        public ActionResult CitasDoctor()
        {
            try
            {
                // Obtener el ID del doctor en sesión
                int idUsuario = Convert.ToInt32(Session["ConsecutivoUsuario"]);

                // Validar sesión
                if (idUsuario == 0)
                {
                    return RedirectToAction("Index", "Home");
                }

                // Obtener las citas del doctor
                var citas = _service.ListarCitasDoctor(idUsuario);

                return View(citas);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar las citas del doctor: " + ex.Message;
                return View(new List<CitaDto>());
            }
        }

        private readonly PacienteService _pacienteService = new PacienteService();


        [HttpGet]
        public ActionResult Perfil()
        {
            int idUsuario = Convert.ToInt32(Session["ConsecutivoUsuario"]);
            var model = _pacienteService.ObtenerPerfil(idUsuario);
            if (model == null)
            {
                ViewBag.Error = "No se encontró el perfil del usuario.";
                model = new PerfilPacienteDto();
            }
            return View(model);  // Usa Views/Citas/Perfil.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Perfil(PerfilPacienteDto model)
        {
            if (string.IsNullOrWhiteSpace(model.ContrasenaActual) ||
                string.IsNullOrWhiteSpace(model.ContrasenaNueva) ||
                string.IsNullOrWhiteSpace(model.ConfirmarContrasena))
            {
                ModelState.AddModelError("", "Complete todos los campos de contraseña.");
                return View(model);
            }

            if (model.ContrasenaNueva != model.ConfirmarContrasena)
            {
                ModelState.AddModelError("", "Las contraseñas nuevas no coinciden.");
                return View(model);
            }

            int idUsuario = Convert.ToInt32(Session["ConsecutivoUsuario"]);
            var res = _pacienteService.CambiarContrasena(idUsuario, model.ContrasenaActual, model.ContrasenaNueva);

            if (res.Codigo == 1)
            {
                TempData["Success"] = res.Mensaje;

                var perfilActualizado = _pacienteService.ObtenerPerfil(idUsuario);
                perfilActualizado.ContrasenaActual =
                    perfilActualizado.ContrasenaNueva =
                    perfilActualizado.ConfirmarContrasena = string.Empty;

                return View(perfilActualizado);
            }

            ModelState.AddModelError("", res.Mensaje);
            return View(model);
        }


    }
}
