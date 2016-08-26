using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using RestfulRouting.Exceptions;

namespace RestfulRouting.Mappers
{
    public class Mapper : IMapper
    {
        protected string BasePath;
        protected RouteValueDictionary Constraints = new RouteValueDictionary();
        protected List<Mapper> Mappers = new List<Mapper>();
        protected string[] Namespaces;
        protected List<string> ResourcePaths = new List<string>();

        public Mapper(string[] namespaces = null)
        {
            Namespaces = namespaces;
        }

        public void Root<TController>(Expression<Func<TController, object>> action)
        {
            AddMapper(new RootMapper<TController>(action));
        }

        public void Route(RouteBase routeBase)
        {
            AddMapper(new RouteMapper(routeBase));
        }

        public void Resources<TController>(Action<IResourcesMapper<TController>> mapper = null)
            where TController : Controller
        {
            AddMapper(new ResourcesMapper<TController>(mapper));
        }

        public void Resource<TController>(Action<IResourceMapper<TController>> mapper = null)
            where TController : Controller
        {
            AddMapper(new ResourceMapper<TController>(mapper));
        }

        public void Area<TController>(string name, Action<IAreaMapper> mapper = null) where TController : Controller
        {
            AddMapper(new AreaMapper(name, null, typeof(TController).Namespace, mapper));
        }

        public void Area<TController>(string name, string pathPrefix, Action<IAreaMapper> mapper)
            where TController : Controller
        {
            AddMapper(new AreaMapper(name, pathPrefix, typeof(TController).Namespace, mapper));
        }

        public void Area(string name, Action<IAreaMapper> mapper)
        {
            AddMapper(new AreaMapper(name, null, null, mapper));
        }

        public void Area(string name, string pathPrefix, Action<IAreaMapper> mapper)
        {
            AddMapper(new AreaMapper(name, pathPrefix, null, mapper));
        }

        public virtual StandardMapper Path(string path)
        {
            var mapper = new StandardMapper().Path(path);
            AddMapper(mapper);
            return mapper;
        }

        public virtual void Connect<TRouteSet>(string[] namespaces = null) where TRouteSet : RouteSet, new()
        {
            AddMapper(new ConnectMapper<TRouteSet> {Namespaces = namespaces});
        }

        private void AddMapper(Mapper mapper)
        {
            if (mapper.Namespaces == null)
                mapper.Namespaces = Namespaces;
            Mappers.Add(mapper);
        }

        protected void AddResourcePath(string path)
        {
            if (!string.IsNullOrEmpty(path))
                ResourcePaths.Add(path);
        }

        protected virtual void SetBasePath(string basePath)
        {
            BasePath = basePath;
        }

        public virtual void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            EnumerateMappers(mapper => { mapper.RegisterRoutes(routeBuilder); });
        }

        protected string Join(params string[] parts)
        {
            var validParts = parts.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            return string.Join("/", validParts);
        }

        protected string GetControllerName<T>()
        {
            var controllerName = typeof(T).Name;

            var name = controllerName.Substring(0, controllerName.Length - "Controller".Length);
            return RouteSet.LowercaseDefaults ? name.ToLowerInvariant() : name;
        }

        protected void RegisterNested(IRouteBuilder routeBuilder, Action<Mapper> action = null)
        {
            EnumerateMappers(mapper =>
            {
                ConfigureNestedMapper(mapper);
                action?.Invoke(mapper);
                mapper.RegisterRoutes(routeBuilder);
            });
        }

        protected void EnumerateMappers(Action<Mapper> action)
        {
            // Use the explicit IEnumerator<Mapper> otherwise the exception
            // is not thrown when calling MoveNext().
            using (IEnumerator<Mapper> enumerator = Mappers.GetEnumerator())
            {
                while (MoveNextMapper(enumerator))
                    action(enumerator.Current);
            }
        }

        private bool MoveNextMapper(IEnumerator enumerator)
        {
            try
            {
                return enumerator.MoveNext();
            }
            catch (InvalidOperationException e)
            {
                var message =
                    @"The mappers were modified during enumeration. Did you accidentally use a parent mapper inside of a scoped mapping block?
For example:

map.Area<PostsController>(""posts"", posts =>
{
    // Wrong - should be using 'posts' instead of 'map'.
    map.Resources<CommentsController>();

    // Right
    posts.Resources<CommentsController>();
});";
                throw new InvalidMapperConfigurationException(message, e);
            }
        }

        private void ConfigureNestedMapper(Mapper mapper)
        {
            mapper.SetBasePath(BasePath);
            mapper.InheritConstraints(Constraints);
            mapper.InheritNamespaces(Namespaces);
        }

        private void InheritConstraints(RouteValueDictionary constraints)
        {
            foreach (var constraint in constraints)
                if (!Constraints.ContainsKey(constraint.Key))
                    Constraints[constraint.Key] = constraint.Value;
        }

        private void InheritNamespaces(string[] namespaces)
        {
            if (namespaces == null)
                namespaces = new string[0];

            Namespaces = Namespaces == null
                ? namespaces
                : Namespaces.Concat(namespaces).ToArray();
        }

        protected void ConfigureRoute(Route route)
        {
            foreach (var constraint in Constraints)
                route.Constraints[constraint.Key] = (IRouteConstraint) constraint.Value;

            if ((Namespaces != null) && Namespaces.Any())
                route.DataTokens["Namespaces"] = Namespaces;
        }

        public void SetParentResources(List<string> resources)
        {
            ResourcePaths = resources.ToList();
        }

        public string JoinResources(string with)
        {
            var resources = new List<string>();
            resources.AddRange(ResourcePaths);
            resources.Add(with);
            return string.Join("_", resources.Distinct().Select(r => r.ToLowerInvariant()));
        }
    }
}