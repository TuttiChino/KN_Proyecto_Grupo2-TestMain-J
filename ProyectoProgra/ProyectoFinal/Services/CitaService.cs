using ProyectoFinal.EF;
using ProyectoFinal.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ProyectoFinal.Services
{
    public class CitaService
    {
        private readonly BDCitasMedicasEntities _context = new BDCitasMedicasEntities();

        public List<CitaDto> ListarCitas()
        {
            var result = _context.Database.SqlQuery<CitaDto>("EXEC sp_GetCitas").ToList();
            return result;
        }

        public ResultadoSP CrearCita(int doctorId, int pacienteId, DateTime fechaInicio, DateTime fechaFin, string motivo)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var result = db.Database.SqlQuery<ResultadoSP>(
                    "EXEC dbo.sp_InsertCita @DoctorId, @PacienteId, @FechaInicio, @FechaFin, @Motivo",
                    new SqlParameter("@DoctorId", doctorId),
                    new SqlParameter("@PacienteId", pacienteId),
                    new SqlParameter("@FechaInicio", fechaInicio),
                    new SqlParameter("@FechaFin", fechaFin),
                    new SqlParameter("@Motivo", motivo)
                ).FirstOrDefault();

                return result ?? new ResultadoSP { Codigo = 0, Mensaje = "No se obtuvo respuesta del procedimiento." };
            }
        }

        public ResultadoSP EditarCita(int idCita, int doctorId, int pacienteId, DateTime fechaInicio, DateTime fechaFin, string motivo)
        {
            var p1 = new SqlParameter("@IdCita", idCita);
            var p2 = new SqlParameter("@DoctorId", doctorId);
            var p3 = new SqlParameter("@PacienteId", pacienteId);
            var p4 = new SqlParameter("@FechaInicio", fechaInicio);
            var p5 = new SqlParameter("@FechaFin", fechaFin);
            var p6 = new SqlParameter("@Motivo", motivo);

            var res = _context.Database.SqlQuery<ResultadoSP>(
                "EXEC sp_UpdateCita @IdCita, @DoctorId, @PacienteId, @FechaInicio, @FechaFin, @Motivo",
                p1, p2, p3, p4, p5, p6
            ).FirstOrDefault();

            return res ?? new ResultadoSP { Codigo = -99, Mensaje = "Respuesta nula del SP" };
        }

        public ResultadoSP CancelarCita(int idCita, string motivo)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var result = db.Database.SqlQuery<ResultadoSP>(
                    "EXEC dbo.sp_CancelarCita @IdCita, @Motivo",
                    new SqlParameter("@IdCita", idCita),
                    new SqlParameter("@Motivo", motivo ?? (object)DBNull.Value)
                ).FirstOrDefault();

                return result ?? new ResultadoSP { Codigo = 0, Mensaje = "No se obtuvo respuesta del procedimiento." };
            }
        }

        public ResultadoSP MarcarAtendida(int idCita)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                try
                {
                    var res = db.Database.SqlQuery<ResultadoSP>(
                        "EXEC dbo.sp_MarcarCitaAtendida @IdCita",
                        new SqlParameter("@IdCita", idCita)
                    ).FirstOrDefault();

                    return res ?? new ResultadoSP { Codigo = 0, Mensaje = "No se obtuvo respuesta del procedimiento." };
                }
                catch (Exception ex)
                {
                    return new ResultadoSP
                    {
                        Codigo = 0,
                        Mensaje = $"Error en CitaService.MarcarAtendida: {ex.Message}"
                    };
                }
            }
        }

        public List<CitaDto> ListarCitasDoctor(int idDoctor)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                try
                {
                    var result = db.Database.SqlQuery<CitaDto>(
                        "EXEC dbo.sp_GetCitasPorDoctor @IdUsuario",
                        new SqlParameter("@IdUsuario", idDoctor)
                    ).ToList();

                    return result;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al obtener las citas del doctor: " + ex.Message);
                }
            }
        }
    }
}
