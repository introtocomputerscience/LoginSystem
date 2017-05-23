using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using CustomLoginSystem.Models;
using CustomLoginSystem.Interfaces;

namespace CustomLoginSystem.Controllers
{
    public class UserController : ApiController
    {
        private IUserManagementService userManagement;
        public UserController(IUserManagementService userManagement)
        {
            this.userManagement = userManagement;
        }

        [Route("User/CreateUser")]
        [HttpPost]
        public HttpResponseMessage CreateUser(CreateUserRequest request)
        {
            HttpResponseMessage hrm;
            try
            {
                var operationResult = userManagement.CreateUser(request.Email, request.Password);
                if (operationResult.IsSuccess)
                {
                    hrm = Request.CreateResponse(HttpStatusCode.OK, operationResult.Message);
                }
                else
                {
                    hrm = Request.CreateErrorResponse(HttpStatusCode.BadRequest, operationResult.Message);
                }
            }
            catch (Exception e)
            {
                hrm = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
            return hrm;
        }

        [Route("User/ForgotPassword")]
        [HttpPost]
        public HttpResponseMessage InitiatePasswordReset(ForgotPasswordRequest request)
        {
            HttpResponseMessage hrm;
            try
            {
                var operationResult = userManagement.InitiateForgotPassword(request.Email);
                if (operationResult.IsSuccess)
                {
                    hrm = Request.CreateResponse(HttpStatusCode.OK, operationResult.Message);
                }
                else
                {
                    hrm = Request.CreateErrorResponse(HttpStatusCode.BadRequest, operationResult.Message);
                }
            }
            catch (Exception e)
            {
                hrm = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
            return hrm;
        }

        [Route("User/ResetPassword/{key}")]
        [HttpGet]
        public HttpResponseMessage ResetPassword(Guid key)
        {
            HttpResponseMessage hrm;
            try
            {
                var operationResult = userManagement.ForgotPasswordReset(key, "TestPassword");
                if (operationResult.IsSuccess)
                {
                    hrm = Request.CreateResponse(HttpStatusCode.OK, operationResult.Message);
                }
                else
                {
                    hrm = Request.CreateErrorResponse(HttpStatusCode.BadRequest, operationResult.Message);
                }
            }
            catch (Exception e)
            {
                hrm = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
            return hrm;
        }
    }
}
