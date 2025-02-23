using KPCOS.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KPCOS.WebFramework.Api;

public class CustomAuthorizeAttribute  : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string[] _requiredRoles;

    public CustomAuthorizeAttribute(params string[] roles)
    {
        _requiredRoles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated ?? false)
        {
            throw new UnauthorizedAccessException();
        }
        
        if (_requiredRoles.Length == 0)
        {
            return;
        }
        
        var hasRequiredRole = _requiredRoles.Any(role => user.IsInRole(role));
        
        if (!hasRequiredRole)
        {
            throw new UnauthorizedAccessException();
        }
    }
}