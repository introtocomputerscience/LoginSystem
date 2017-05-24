using CustomLoginSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomLoginSystem.Interfaces
{
    public interface IUserManagementService
    {
        OperationResponse CreateUser(string email, string password);
        OperationResponse RemoveUser(string email);
        OperationResponse UpdateUser(string email, string password);
        OperationResponse InitiateForgotPassword(string email);
        OperationResponse ForgotPasswordReset(Guid key, string password);
    }
}
