using System.Net;
using System.Net.Mail;
using System.Configuration;

namespace ProyectoFinal.Services
{
    public class NotificacionService
    {
        public void EnviarCorreo(string destinatario, string asunto, string mensaje)
        {
            var smtp = new SmtpClient
            {
                Host = ConfigurationManager.AppSettings["SmtpHost"],
                Port = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    ConfigurationManager.AppSettings["SmtpUser"],
                    ConfigurationManager.AppSettings["SmtpPass"]
                )
            };

            var mail = new MailMessage(ConfigurationManager.AppSettings["SmtpUser"], destinatario)
            {
                Subject = asunto,
                Body = mensaje,
                IsBodyHtml = true
            };

            smtp.Send(mail);
        }
    }
}
