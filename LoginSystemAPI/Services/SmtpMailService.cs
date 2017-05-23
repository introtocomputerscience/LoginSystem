using CustomLoginSystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Net;

namespace CustomLoginSystem.Services
{
    public class SmtpMailService : IMailService
    {
        private string fromEmail;
        private int port;
        private string host;
        private NetworkCredential credentials;

        public SmtpMailService(string fromEmail, string host, int port, string username, string password)
        {
            this.fromEmail = fromEmail;
            this.host = host;
            this.port = port;
            this.credentials = new System.Net.NetworkCredential(username, password);
        }
        public void SendMail(string email, string subject, string message)
        {
            SmtpClient client = new SmtpClient()
            {
                Host = host,
                Port = port,
                Credentials = credentials,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            MailMessage mail = new MailMessage(fromEmail, email, subject, message);
            mail.IsBodyHtml = true;
            client.Send(mail);
        }
    }
}