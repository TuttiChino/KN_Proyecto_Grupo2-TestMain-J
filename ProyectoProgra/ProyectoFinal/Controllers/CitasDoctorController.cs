using ProyectoFinal.EF;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoFinal.Controllers
{
    public class CitasDoctorController : Controller
    {
        // GET: CitasDoctor
        public ActionResult Index()
        {
            var usuarioId = Session["ConsecutivoUsuario"] as int?;
            if (usuarioId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new BDCitasMedicasEntities())
            {
                // Obtener el médico asociado al usuario
                var medico = db.tbMedicoes
                    .Where(m => m.ConsecutivoUsuario == usuarioId.Value)
                    .FirstOrDefault();

                if (medico == null)
                {
                    ViewBag.Error = "No se encontró información del médico.";
                    return View();
                }

                var citas = (from c in db.tbCitas
                            where c.ConsecutivoMedico == medico.ConsecutivoMedico
                               && c.Estado != "Cancelada"
                            orderby c.Fecha, c.HoraInicio
                            select c).ToList();

                ViewBag.Medico = medico;
                return View(citas);
            }
        }
    }
}

