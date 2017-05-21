using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using CustomLoginSystem.Models;

namespace CustomLoginSystem.Controllers
{
    public class UserController : ApiController
    {

        [SwaggerOperation("CreateUser")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public HttpResponseMessage CreateUser(CreateUserRequest request)
        {
            HttpResponseMessage hrm = Request.CreateResponse(HttpStatusCode.OK);
            return hrm;
        }
    }
}
