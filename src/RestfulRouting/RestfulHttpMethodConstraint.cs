using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Primitives;

namespace RestfulRouting
{
    public class RestfulHttpMethodConstraint : HttpMethodRouteConstraint
    {
        public RestfulHttpMethodConstraint(params string[] allowedMethods)
            : base(allowedMethods)
        {
        }

        public override bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            switch (routeDirection)
            {
                case RouteDirection.IncomingRequest:
                    foreach (var method in AllowedMethods)
                    {
                        if (string.Equals(method, httpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
                            return true;

                        var form = httpContext.Request.Form;

                        if (form == null)
                            continue;

                        StringValues intendedMethod;
                        if ((form.TryGetValue("_method", out intendedMethod) || form.TryGetValue("X-HTTP-Method-Override", out intendedMethod)) &&
                            string.Equals(method, intendedMethod, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    break;
            }

            return base.Match(httpContext, route, routeKey, values, routeDirection);
        }
    }
}