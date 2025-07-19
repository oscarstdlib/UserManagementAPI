using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using UserManagementAPI.Services;

namespace UserManagementAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _recurso;
        private readonly string _accion;

        public AuthorizeAttribute(string recurso, string accion)
        {
            _recurso = recurso;
            _accion = accion;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var hasPermission = user.Claims.Any(c => 
                c.Type == "Permiso" && 
                c.Value == $"{_recurso}:{_accion}");

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
} 