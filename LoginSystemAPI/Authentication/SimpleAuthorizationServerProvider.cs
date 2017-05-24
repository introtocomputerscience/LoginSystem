using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;
using LoginSystemDAL.Helpers;
using LoginSystemDAL;
using LoginSystemDAL.Interfaces;
using System.Linq;
using CustomLoginSystem.Helpers;
using System.Security.Claims;
using System.Configuration;

namespace CustomLoginSystem.Authentication
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            using (var unitOfWork = new UnitOfWork<LoginSystemEntities>())
            {
                IRepository<User> userRepo = unitOfWork.GetRepository<User>();
                var user = userRepo.Get(x => x.Email.Equals(context.UserName, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (user == null)
                {
                    //Login Failure
                    context.SetError("invalid_grant", "The username or password is incorrect.");
                    return;
                }
                else
                {
                    var secretKey = ConfigurationManager.AppSettings["SecretKey"];
                    var submittedPassword = HashHelper.GetEncryptedPassword(user.Salt, context.Password, secretKey);
                    var equal = user.Password.SequenceEqual(submittedPassword);
                    if (!equal)
                    {
                        //Login Failure
                        context.SetError("invalid_grant", "The username or password is incorrect.");
                        return;
                    }
                }
            }
            //Login Success
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Email, context.UserName));
            identity.AddClaim(new Claim(ClaimTypes.Role, "user"));

            context.Validated(identity);

        }
    }
}