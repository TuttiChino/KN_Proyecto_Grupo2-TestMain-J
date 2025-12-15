namespace ProyectoFinal.Models
{
    public class PerfilPacienteDto
    {
        public int ConsecutivoUsuario { get; set; }

        // Solo lectura
        public string Identificacion { get; set; }
        public string Nombre { get; set; }
        public string CorreoElectronico { get; set; }

        // Para cambio de contraseña
        public string ContrasenaActual { get; set; }
        public string ContrasenaNueva { get; set; }
        public string ConfirmarContrasena { get; set; }
    }
}
