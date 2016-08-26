using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;

namespace RestfulRouting.Mappers
{
    public class StandardMapper : Mapper
    {
        protected string Name;
        protected string Url;
        protected string DefaultName { get; set; }

        protected RouteValueDictionary RouteConstraints { get; set; }
        = new RouteValueDictionary {{"httpMethod", new HttpMethodRouteConstraint("GET")}};

        protected RouteValueDictionary RouteDataTokens { get; set; } = new RouteValueDictionary();
        protected RouteValueDictionary RouteDefaults { get; set; } = new RouteValueDictionary();

        public override StandardMapper Path(string url)
        {
            Url = url;
            return this;
        }

        private static string GetActionName<TController>(Expression<Func<TController, object>> actionExpression)
        {
            var body = (MethodCallExpression) actionExpression.Body;
            var actionName = body.Method.Name;
            return RouteSet.LowercaseDefaults ? actionName.ToLowerInvariant() : actionName;
        }

        public StandardMapper To<T>(Expression<Func<T, object>> func)
        {
            var controller = GetControllerName<T>();
            var action = GetActionName(func);

            RouteDefaults["controller"] = controller;
            RouteDefaults["action"] = action;
            DefaultName = $"{controller}#{action}".ToLowerInvariant();
            return this;
        }

        public StandardMapper Constrain(string name, IRouteConstraint constraint)
        {
            RouteConstraints[name] = constraint;
            return this;
        }

        public StandardMapper Default(string name, IRouteConstraint constraint)
        {
            RouteDefaults[name] = constraint;
            return this;
        }

        public StandardMapper GetOnly()
        {
            RouteConstraints["httpMethod"] = new HttpMethodRouteConstraint("GET");

            return this;
        }

        public StandardMapper Allow(params HttpVerbs[] methods)
        {
            RouteConstraints["httpMethod"] =
                new RestfulHttpMethodConstraint(methods.Select(x => x.ToString().ToUpperInvariant()).ToArray());
            return this;
        }

        public StandardMapper Named(string name)
        {
            Name = name;
            return this;
        }

        public StandardMapper WithNamespace<T>()
        {
            Namespaces = new[] {typeof(T).Namespace};
            return this;
        }

        public override void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            Url = Join(BasePath, Url);
            RouteDataTokens["Namespaces"] = Namespaces;

            var inlineResolver = routeBuilder.ServiceProvider.GetRequiredService(typeof(IInlineConstraintResolver));
            var route = new Route(routeBuilder.DefaultHandler,
                Name ?? DefaultName,
                Url,
                RouteDefaults,
                RouteConstraints,
                RouteDataTokens,
                (IInlineConstraintResolver) inlineResolver);

            routeBuilder.Routes.Add(route);
        }
    }
}