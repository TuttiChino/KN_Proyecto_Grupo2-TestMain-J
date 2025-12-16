using System;

namespace ProyectoFinal.Models
{
    public class ReporteResumenVM
    {
        public int TotalCitas { get; set; }
        public int CitasProgramadas { get; set; }
        public int CitasCompletadas { get; set; }
        public int CitasCanceladas { get; set; }

        public int TotalMedicos { get; set; }
        public int TotalPacientes { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}