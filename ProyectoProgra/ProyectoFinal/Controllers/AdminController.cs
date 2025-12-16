using ProyectoFinal.EF;
using ProyectoFinal.Models;
using ProyectoFinal.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace ProyectoFinal.Controllers
{
    public class AdminController : Controller
    {
        private readonly UsuarioService _usuarioService = new UsuarioService();
        private readonly MedicoService _medicoService = new MedicoService();
        private readonly EspecialidadService _especialidadService = new EspecialidadService();

        private const int PERFIL_MEDICO = 2;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["ConsecutivoUsuario"] == null)
            {
                filterContext.Result = RedirectToAction("Index", "Home");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        // ============================
        // PANEL ADMIN
        // ============================
        public ActionResult Index()
        {
            return View();
        }

        // ============================
        // MÉDICOS
        // ============================
        public ActionResult Medicos()
        {
            try
            {
                return View(_medicoService.ListarMedicos());
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(Enumerable.Empty<MedicoDto>());
            }
        }

        [HttpGet]
        public ActionResult CrearMedico()
        {
            return View(new UsuarioDto
            {
                Estado = true,
                ConsecutivoPerfil = PERFIL_MEDICO
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearMedico(UsuarioDto model)
        {
            try
            {
                model.ConsecutivoPerfil = PERFIL_MEDICO;

                var resUsuario = _usuarioService.CrearUsuario(model);
                if (resUsuario.Codigo != 1)
                {
                    TempData["Error"] = resUsuario.Mensaje;
                    return View(model);
                }

                var medico = new MedicoDto
                {
                    Nombre = model.Nombre,
                    Identificacion = model.Identificacion,
                    CorreoElectronico = model.CorreoElectronico,
                    Estado = model.Estado,
                    ConsecutivoUsuario = resUsuario.ConsecutivoUsuario
                };

                var resMedico = _medicoService.CrearMedico(medico);
                TempData[resMedico.Codigo == 1 ? "Success" : "Error"] = resMedico.Mensaje;

                return RedirectToAction("Medicos");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult EditarMedico(int id)
        {
            var medico = _medicoService.ObtenerMedico(id);
            if (medico == null)
            {
                TempData["Error"] = "Médico no encontrado.";
                return RedirectToAction("Medicos");
            }
            return View(medico);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarMedico(MedicoDto model)
        {
            var res = _medicoService.ActualizarMedico(model);
            TempData[res.Codigo == 1 ? "Success" : "Error"] = res.Mensaje;
            return RedirectToAction("Medicos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarMedico(int id)
        {
            var res = _medicoService.EliminarMedico(id);
            TempData[res.Codigo == 1 ? "Success" : "Error"] = res.Mensaje;
            return RedirectToAction("Medicos");
        }

        // ============================
        // HORARIOS
        // ============================
        public ActionResult HorariosMedicos()
        {
            return View(_medicoService.ListarMedicos());
        }

        [HttpGet]
        public ActionResult HorariosMedico(int id)
        {
            var medico = _medicoService.ObtenerMedico(id);
            if (medico == null)
            {
                TempData["Error"] = "Médico no encontrado.";
                return RedirectToAction("HorariosMedicos");
            }

            ViewBag.MedicoNombre = medico.Nombre;
            ViewBag.ConsecutivoMedico = id;

            return View(_medicoService.ListarHorariosMedico(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarHorarioMedico(HorarioMedicoDto model)
        {
            if (model.HoraFin <= model.HoraInicio)
            {
                TempData["Error"] = "Hora fin inválida.";
                return RedirectToAction("HorariosMedico", new { id = model.ConsecutivoMedico });
            }

            _medicoService.InsertarHorarioMedico(model);
            TempData["Success"] = "Horario agregado.";

            return RedirectToAction("HorariosMedico", new { id = model.ConsecutivoMedico });
        }

        // ============================
        // ESPECIALIDADES
        // ============================
        public ActionResult Especialidades()
        {
            return View(_especialidadService.Listar());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearEspecialidad(string nombre)
        {
            _especialidadService.Crear(nombre);
            TempData["Success"] = "Especialidad creada.";
            return RedirectToAction("Especialidades");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarEspecialidad(EspecialidadDto model)
        {
            _especialidadService.Editar(model);
            TempData["Success"] = "Especialidad actualizada.";
            return RedirectToAction("Especialidades");
        }

        public ActionResult EliminarEspecialidad(int id)
        {
            _especialidadService.Eliminar(id);
            TempData["Success"] = "Especialidad eliminada.";
            return RedirectToAction("Especialidades");
        }

        // ============================
        // ESPECIALIDADES POR MÉDICO
        // ============================
        [HttpGet]
        public ActionResult EspecialidadesMedico(int? consecutivoMedico)
        {
            var vm = new EspecialidadesMedicoVM
            {
                Medicos = _medicoService.ListarMedicos(),
                EspecialidadesDisponibles = _especialidadService.Listar(),
                ConsecutivoMedicoSeleccionado = consecutivoMedico
            };

            if (consecutivoMedico.HasValue)
            {
                vm.EspecialidadesAsignadas =
                    _especialidadService.ListarPorMedico(consecutivoMedico.Value);
            }

            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarEspecialidadMedico(MedicoEspecialidadDto model)
        {
            if (model.ConsecutivoMedico <= 0)
            {
                TempData["Error"] = "Debe seleccionar un médico válido.";
                return RedirectToAction("EspecialidadesMedico");
            }

            _especialidadService.AgregarEspecialidadMedico(model);

            TempData["Success"] = "Especialidad asignada correctamente.";

            return RedirectToAction("EspecialidadesMedico", new { id = model.ConsecutivoMedico });
        }

        public ActionResult Reportes(DateTime? fechaInicio, DateTime? fechaFin)
        {
            DateTime fi = fechaInicio ?? DateTime.Today.AddDays(-30);
            DateTime ff = fechaFin ?? DateTime.Today;

            var vm = new ReporteResumenVM
            {
                FechaInicio = fi,
                FechaFin = ff
            };

            using (var db = new BDCitasMedicasEntities())
            {
                var data = db.Database.SqlQuery<ReporteResumenDto>(
                    "EXEC sp_ReporteResumenGeneral @FechaInicio, @FechaFin",
                    new SqlParameter("@FechaInicio", fi),
                    new SqlParameter("@FechaFin", ff)
                ).FirstOrDefault();

                if (data != null)
                {
                    vm.TotalCitas = data.TotalCitas;
                    vm.CitasProgramadas = data.CitasProgramadas;
                    vm.CitasCompletadas = data.CitasCompletadas;
                    vm.CitasCanceladas = data.CitasCanceladas;
                    vm.TotalMedicos = data.TotalMedicos;
                    vm.TotalPacientes = data.TotalPacientes;
                }
            }

            return View(vm); 
        }

    }
}
