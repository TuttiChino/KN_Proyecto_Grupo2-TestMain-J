using System;

namespace ProyectoFinal.Models
{
    public class ReporteResumenDto
    {
        public int TotalCitas { get; set; }
        public int CitasProgramadas { get; set; }
        public int CitasCompletadas { get; set; }
        public int CitasCanceladas { get; set; }
        public int TotalMedicos { get; set; }
        public int TotalPacientes { get; set; }
    }

}
