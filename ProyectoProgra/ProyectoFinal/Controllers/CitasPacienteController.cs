using ProyectoFinal.EF;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ProyectoFinal.Controllers
{
    public class CitasPacienteController : Controller
    {
        // GET: CitasPaciente
        public ActionResult Index()
        {
            var pacienteId = Session["ConsecutivoUsuario"] as int?;
            if (pacienteId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new BDCitasMedicasEntities())
            {
                var citas = (from c in db.tbCitas
                            where c.ConsecutivoPaciente == pacienteId.Value
                            orderby c.Fecha descending, c.HoraInicio descending
                            select c).ToList();

                return View(citas);
            }
        }

        // GET: CitasPaciente/Crear
        public ActionResult Crear()
        {
            var pacienteId = Session["ConsecutivoUsuario"] as int?;
            if (pacienteId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new BDCitasMedicasEntities())
            {
                ViewBag.Medicos = (from m in db.tbMedicoes
                                  where m.Estado == true
                                  select new
                                  {
                                      m.ConsecutivoMedico,
                                      Nombre = m.Nombre
                                  }).ToList()
                                  .Select(m => new SelectListItem
                                  {
                                      Value = m.ConsecutivoMedico.ToString(),
                                      Text = m.Nombre
                                  }).ToList();

                return View();
            }
        }

        // POST: CitasPaciente/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(int consecutivoMedico, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin, string observaciones)
        {
            var pacienteId = Session["ConsecutivoUsuario"] as int?;
            if (pacienteId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                using (var db = new BDCitasMedicasEntities())
                {
                    // Validar solape usando stored procedure
                    var validacion = db.sp_ValidarSolapeCita(
                        consecutivoMedico,
                        fecha.Date,
                        horaInicio,
                        horaFin
                    ).FirstOrDefault();

                    if (validacion?.HayFueraHorario == true)
                    {
                        ViewBag.Error = "La cita está fuera del horario disponible del médico.";
                        ViewBag.Medicos = (from m in db.tbMedicoes
                                          where m.Estado == true
                                          select new
                                          {
                                              m.ConsecutivoMedico,
                                              Nombre = m.Nombre
                                          }).ToList()
                                          .Select(m => new SelectListItem
                                          {
                                              Value = m.ConsecutivoMedico.ToString(),
                                              Text = m.Nombre
                                          }).ToList();
                        return View();
                    }

                    if (validacion?.HaySolape == true)
                    {
                        ViewBag.Error = "Ya existe una cita en ese horario para este médico.";
                        ViewBag.Medicos = (from m in db.tbMedicoes
                                          where m.Estado == true
                                          select new
                                          {
                                              m.ConsecutivoMedico,
                                              Nombre = m.Nombre
                                          }).ToList()
                                          .Select(m => new SelectListItem
                                          {
                                              Value = m.ConsecutivoMedico.ToString(),
                                              Text = m.Nombre
                                          }).ToList();
                        return View();
                    }

                    var nuevaCita = new tbCita
                    {
                        ConsecutivoPaciente = pacienteId.Value,
                        ConsecutivoMedico = consecutivoMedico,
                        Fecha = fecha.Date,
                        HoraInicio = horaInicio,
                        HoraFin = horaFin,
                        Estado = "Programada",
                        Observaciones = observaciones
                    };

                    db.tbCitas.Add(nuevaCita);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al crear la cita: " + ex.Message;
                using (var db = new BDCitasMedicasEntities())
                {
                    ViewBag.Medicos = (from m in db.tbMedicoes
                                      where m.Estado == true
                                      select new
                                      {
                                          m.ConsecutivoMedico,
                                          Nombre = m.Nombre
                                      }).ToList()
                                      .Select(m => new SelectListItem
                                      {
                                          Value = m.ConsecutivoMedico.ToString(),
                                          Text = m.Nombre
                                      }).ToList();
                }
                return View();
            }
        }
    }
}

