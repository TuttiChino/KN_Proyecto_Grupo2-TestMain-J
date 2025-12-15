using ProyectoFinal.EF;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ProyectoFinal.Controllers
{
    public class DoctorController : Controller
    {
        private BDCitasMedicasEntities db = new BDCitasMedicasEntities();


        // Listado de Citas del doctor

        public ActionResult Index()
        {
            if (Session["ConsecutivoUsuario"] == null)
                return RedirectToAction("Index", "Home");

            int idUsuario = Convert.ToInt32(Session["ConsecutivoUsuario"]);

            // Obtener el medico vinculado a ese usuario
            var medico = db.tbMedico.FirstOrDefault(m => m.ConsecutivoUsuario == idUsuario);
            if (medico == null)
                return RedirectToAction("Landing", "Home");

            // Citas del médico ordenadas por fecha
            var citas = db.tbCita
                .Include(c => c.tbUsuario)
                .Where(c => c.ConsecutivoMedico == medico.ConsecutivoMedico)
                .OrderBy(c => c.Fecha)
                .ThenBy(c => c.HoraInicio)
                .ToList();

            // Contadores para el dashboard
            ViewBag.TotalProgramadas = citas.Count(c => c.Estado == "Programada" || c.Estado == "Reprogramada");
            ViewBag.TotalCompletadas = citas.Count(c => c.Estado == "Completada");
            ViewBag.TotalCanceladas = citas.Count(c => c.Estado == "Cancelada");

            ViewBag.NombreDoctor = medico.Nombre;
            return View(citas);
        }


        // Agendar Nueva Cita
        [HttpGet]
        public ActionResult NuevaCita()
        {
            if (Session["ConsecutivoUsuario"] == null)
                return RedirectToAction("Index", "Home");

            ViewBag.Pacientes = new SelectList(
                db.tbUsuario.Where(u => u.tbPerfil.Nombre == "Paciente" && u.Estado == true),
                "ConsecutivoUsuario", "Nombre"
            );

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NuevaCita(tbCita cita)
        {
            if (Session["ConsecutivoUsuario"] == null)
                return RedirectToAction("Index", "Home");

            int idUsuario = Convert.ToInt32(Session["ConsecutivoUsuario"]);
            var medico = db.tbMedico.FirstOrDefault(m => m.ConsecutivoUsuario == idUsuario);
            if (medico == null)
                return RedirectToAction("Landing", "Home");

            cita.ConsecutivoMedico = medico.ConsecutivoMedico;
            cita.Estado = "Programada";

            // Ejecutar SP para validar
            var resultado = db.Database.SqlQuery<ProyectoFinal.Models.ValidarSolapeCitaResult>(
                "EXEC dbo.sp_ValidarSolapeCita @MedicoId={0}, @Fecha={1}, @HoraInicio={2}, @HoraFin={3}",
                cita.ConsecutivoMedico, cita.Fecha, cita.HoraInicio, cita.HoraFin
            ).FirstOrDefault();

            bool hayFueraHorario = resultado?.HayFueraHorario ?? false;
            bool haySolape = resultado?.HaySolape ?? false;

            if (hayFueraHorario)
            {
                TempData["Error"] = "⚠️ La cita está fuera del horario disponible del médico.";
            }
            else if (haySolape)
            {
                TempData["Error"] = "⚠️ Ya existe una cita en ese rango horario.";
            }
            else if (ModelState.IsValid)
            {
                db.tbCita.Add(cita);
                db.SaveChanges();
                TempData["Exito"] = "Cita agendada con éxito.";
                return RedirectToAction("Index");
            }

            ViewBag.Pacientes = new SelectList(
                db.tbUsuario.Where(u => u.tbPerfil.Nombre == "Paciente" && u.Estado == true),
                "ConsecutivoUsuario", "Nombre"
            );

            return View(cita);
        }



        // Cancelar Cita
        public ActionResult CancelarCita(int id)
        {
            var cita = db.tbCita.Find(id);
            if (cita != null)
            {
                cita.Estado = "Cancelada";
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult CompletarCita(int id)
        {
            var cita = db.tbCita.Find(id);
            if (cita != null && cita.Estado != "Cancelada")
            {
                cita.Estado = "Completada";
                db.SaveChanges();
                TempData["Exito"] = " La cita ha sido marcada como completada.";
            }
            else
            {
                TempData["Error"] = "⚠️ No se puede completar una cita cancelada o inexistente.";
            }

            return RedirectToAction("Index");
        }


        // Edita Cita Get
        [HttpGet]
        public ActionResult EditarCita(int id)
        {
            var cita = db.tbCita.Find(id);
            if (cita == null)
            {
                TempData["Error"] = "⚠️ No se encontró la cita.";
                return RedirectToAction("Index");
            }

            ViewBag.Pacientes = new SelectList(
                db.tbUsuario.Where(u => u.tbPerfil.Nombre == "Paciente" && u.Estado == true),
                "ConsecutivoUsuario", "Nombre", cita.ConsecutivoPaciente
            );

            return View(cita);
        }


        // Editar Cita Post

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarCita(tbCita citaEditada)
        {
            if (Session["ConsecutivoUsuario"] == null)
                return RedirectToAction("Index", "Home");

            int idUsuario = Convert.ToInt32(Session["ConsecutivoUsuario"]);
            var medico = db.tbMedico.FirstOrDefault(m => m.ConsecutivoUsuario == idUsuario);
            if (medico == null)
                return RedirectToAction("Landing", "Home");

            citaEditada.ConsecutivoMedico = medico.ConsecutivoMedico;
            citaEditada.Estado = "Reprogramada";

            // Validar solape y horario
            var resultado = db.Database.SqlQuery<ProyectoFinal.Models.ValidarSolapeCitaResult>(
                 "EXEC dbo.sp_ValidarSolapeCita @MedicoId={0}, @Fecha={1}, @HoraInicio={2}, @HoraFin={3}, @CitaActualId={4}",
                citaEditada.ConsecutivoMedico, citaEditada.Fecha, citaEditada.HoraInicio, citaEditada.HoraFin, citaEditada.ConsecutivoCita
            ).FirstOrDefault();


            bool hayFueraHorario = resultado?.HayFueraHorario ?? false;
            bool haySolape = resultado?.HaySolape ?? false;

            if (hayFueraHorario)
            {
                TempData["Error"] = "⚠️ La cita está fuera del horario disponible del médico.";
            }
            else if (haySolape)
            {
                TempData["Error"] = "⚠️ Ya existe una cita en ese rango horario.";
            }
            else if (ModelState.IsValid)
            {
                db.Entry(citaEditada).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Exito"] = "Cita actualizada con éxito.";
                return RedirectToAction("Index");
            }

            ViewBag.Pacientes = new SelectList(
                db.tbUsuario.Where(u => u.tbPerfil.Nombre == "Paciente" && u.Estado == true),
                "ConsecutivoUsuario", "Nombre", citaEditada.ConsecutivoPaciente
            );

            return View(citaEditada);
        }



        // Limpia todas las citas del Doctor

        [HttpPost]
        public ActionResult LimpiarCitas()
        {
            if (Session["ConsecutivoUsuario"] == null)
                return RedirectToAction("Index", "Home");

            int idUsuario = Convert.ToInt32(Session["ConsecutivoUsuario"]);
            var medico = db.tbMedico.FirstOrDefault(m => m.ConsecutivoUsuario == idUsuario);

            if (medico != null)
            {
                var citas = db.tbCita.Where(c => c.ConsecutivoMedico == medico.ConsecutivoMedico);
                db.tbCita.RemoveRange(citas);
                db.SaveChanges();

                TempData["Exito"] = "Todas las citas del doctor fueron eliminadas correctamente.";
            }
            else
            {
                TempData["Error"] = "⚠️ No se encontró el médico asociado al usuario.";
            }

            return RedirectToAction("Index");
        }



    }
}
