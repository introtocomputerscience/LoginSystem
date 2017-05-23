using Autofac;
using Autofac.Integration.WebApi;
using CustomLoginSystem.Interfaces;
using CustomLoginSystem.Services;
using LoginSystemDAL;
using LoginSystemDAL.Helpers;
using LoginSystemDAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Http;

namespace CustomLoginSystem
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Load AppSettings
            var secretKey = ConfigurationManager.AppSettings["SecretKey"];
            var hostname = ConfigurationManager.AppSettings["HostName"];
            var smtpEmail = ConfigurationManager.AppSettings["SmtpEmail"];
            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            var smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"];
            var smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
            // Autofac
            var builder = new ContainerBuilder();
            builder.RegisterType<UnitOfWork<LoginSystemEntities>>().As<IUnitOfWork>();
            builder.Register<SmtpMailService>(c => new SmtpMailService(smtpEmail, smtpHost, smtpPort, smtpUsername, smtpPassword)).As<IMailService>();
            builder.Register<UserManagementService>(c => new UserManagementService(c.Resolve<IUnitOfWork>(), secretKey, hostname, c.Resolve<IMailService>())).As<IUserManagementService>();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            config.DependencyResolver = new AutofacWebApiDependencyResolver(builder.Build());

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
