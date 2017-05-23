using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomLoginSystem.Interfaces
{
    public interface IMailService
    {
        void SendMail(string email, string subject, string message);
    }
}
