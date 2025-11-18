using System;

namespace ProyectoFinal.Models
{
    public class CitaDto
    {
        public int IdCita { get; set; }

        public int IdDoctor { get; set; }
        public int IdPaciente { get; set; }

        public string NombreDoctor { get; set; }
        public string NombrePaciente { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public string Motivo { get; set; }
        public string Estado { get; set; }
    }
}
