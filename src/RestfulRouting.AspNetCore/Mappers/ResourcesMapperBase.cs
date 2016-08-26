using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;
using RestfulRouting.Exceptions;

namespace RestfulRouting.Mappers
{
    public abstract class ResourcesMapperBase<TController> : Mapper, IResourcesMapperBase where TController : Controller
    {
        protected string ControllerName;
        protected bool GenerateFormatRoutes;
        protected string IdParameterName = "id";
        protected Dictionary<string, Func<IRouteBuilder, Route>> IncludedActions;
        protected RouteNames Names = new RouteNames();
        protected string PluralResourceName;
        protected string ResourceName;
        protected string ResourcePath;
        protected string SingularResourceName;

        protected ResourcesMapperBase()
        {
            ControllerName = GetControllerName<TController>();
            PluralResourceName = Inflector.Pluralize(ControllerName);
            SingularResourceName = Inflector.Singularize(PluralResourceName);
            As(PluralResourceName);
        }

        protected abstract RoutePaths Paths { get; }

        public void As(string resourceName)
        {
            ResourceName = resourceName;
            CalculatePath();
        }

        public void IdParameter(string name)
        {
            IdParameterName = name;
        }

        public void Except(params string[] actions)
        {
            foreach (var action in actions)
                IncludedActions.Remove(action);
        }

        public void Only(params string[] actions)
        {
            if (actions.Any(action => !IncludedActions.ContainsKey(action)))
                throw new InvalidRestfulMethodException(GetControllerName<TController>(), IncludedActions.Keys.ToArray());

            IncludedActions = IncludedActions.Where(a => actions.Contains(a.Key, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(k => k.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        }

        public void PathNames(Action<RouteNames> action)
        {
            action(Names);
        }

        public void WithFormatRoutes()
        {
            GenerateFormatRoutes = true;
        }

        public void ReRoute(Action<RoutePaths> action)
        {
            action(Paths);
        }

        private void CalculatePath()
        {
            ResourcePath = Join(BasePath, ResourceName);
        }

        protected override void SetBasePath(string basePath)
        {
            base.SetBasePath(basePath);
            CalculatePath();
        }

        protected Route GenerateRoute(IRouteBuilder builder, string path, string controller, string action,
            string[] httpMethods)
        {
            return GenerateRoute(builder, $"{controller}#{action}".ToLowerInvariant(), path, controller, action,
                httpMethods);
        }

        protected Route GenerateRoute(IRouteBuilder builder, string name, string path, string controller, string action,
            string[] httpMethods)
        {
            var inlineResolver = builder.ServiceProvider.GetRequiredService(typeof(IInlineConstraintResolver));
            return new Route(builder.DefaultHandler,
                name,
                path,
                new RouteValueDictionary(new {controller, action}),
                new RouteValueDictionary(new {httpMethod = new RestfulHttpMethodConstraint(httpMethods)}),
                new RouteValueDictionary(new {Namespaces = new[] {typeof(TController).Namespace}}),
                (IInlineConstraintResolver) inlineResolver);
        }

        protected void AddIncludedActions(IRouteBuilder builder, List<Route> routes)
        {
            routes.AddRange(IncludedActions.Select(x => x.Value.Invoke(builder)).ToArray());
        }

        public void Constrain(string key, object value)
        {
            Constraints[key] = value;
        }

        protected virtual string BuildPathFor(string path)
        {
            var idParameterToken = string.Concat("{", IdParameterName, "}");
            return path.Replace("{resourcePath}", ResourcePath)
                .Replace("{id}", idParameterToken)
                .Replace("{indexName}", ProperCaseUrl(Names.IndexName))
                .Replace("{showName}", ProperCaseUrl(Names.ShowName))
                .Replace("{newName}", ProperCaseUrl(Names.NewName))
                .Replace("{createName}", ProperCaseUrl(Names.CreateName))
                .Replace("{editName}", ProperCaseUrl(Names.EditName))
                .Replace("{updateName}", ProperCaseUrl(Names.UpdateName))
                .Replace("{deleteName}", ProperCaseUrl(Names.DeleteName))
                .Replace("{destroyName}", ProperCaseUrl(Names.DestroyName));
        }

        protected string ProperCaseUrl(string url)
        {
            return RouteSet.LowercaseUrls
                ? url.ToLowerInvariant()
                : url;
        }
    }
}