namespace ProyectoFinal.Models
{
    public class UsuarioDto
    {
        public int ConsecutivoUsuario { get; set; }
        public string Identificacion { get; set; }
        public string Nombre { get; set; }
        public string CorreoElectronico { get; set; }
        public string Contrasenna { get; set; }
        public bool Estado { get; set; }
        public int ConsecutivoPerfil { get; set; }
        public string NombrePerfil { get; set; }
    }
}