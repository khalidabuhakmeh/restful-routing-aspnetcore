using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace RestfulRouting.Mappers
{
    public class ResourceMapper<TController> : ResourcesMapperBase<TController>, IResourceMapper<TController>
        where TController : Controller
    {
        private readonly Dictionary<string, KeyValuePair<string, HttpVerbs[]>> _members =
            new Dictionary<string, KeyValuePair<string, HttpVerbs[]>>(StringComparer.OrdinalIgnoreCase);

        private readonly ResourceRoutePaths _resourcePath = new ResourceRoutePaths();
        private readonly Action<ResourceMapper<TController>> _subMapper;

        public ResourceMapper(Action<ResourceMapper<TController>> subMapper = null)
        {
            As(SingularResourceName);
            IncludedActions = new Dictionary<string, Func<IRouteBuilder, Route>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    Names.ShowName,
                    r =>
                        GenerateRoute(r, JoinResources(ResourceName), BuildPathFor(Paths.Show), ControllerName,
                            Names.ShowName, new[] {"GET"})
                },
                {
                    Names.UpdateName,
                    r => GenerateRoute(r, BuildPathFor(Paths.Update), ControllerName, Names.UpdateName, new[] {"PUT"})
                },
                {
                    Names.NewName,
                    r =>
                        GenerateRoute(r, "new_" + JoinResources(ResourceName), BuildPathFor(Paths.New),
                            ControllerName, Names.NewName, new[] {"GET"})
                },
                {
                    Names.EditName,
                    r =>
                        GenerateRoute(r, "edit_" + JoinResources(ResourceName), BuildPathFor(Paths.Edit),
                            ControllerName, Names.EditName, new[] {"GET"})
                },
                {
                    Names.DestroyName,
                    r =>
                        GenerateRoute(r, BuildPathFor(Paths.Destroy), ControllerName, Names.DestroyName,
                            new[] {"DELETE"})
                },
                {
                    Names.CreateName,
                    r => GenerateRoute(r, BuildPathFor(Paths.Create), ControllerName, Names.CreateName, new[] {"POST"})
                }
            };
            if (RouteSet.MapDelete)
                IncludedActions.Add(Names.DeleteName,
                    r =>
                        GenerateRoute(r, "delete_" + JoinResources(ResourceName), BuildPathFor(Paths.Delete),
                            ControllerName, Names.DeleteName, new[] {"GET"}));
            _subMapper = subMapper;
        }

        protected override RoutePaths Paths => _resourcePath;

        public void Member(Action<AdditionalAction> action)
        {
            var additionalAction = new AdditionalAction(_members);
            action(additionalAction);
        }

        private Route MemberRoute(IRouteBuilder builder, string action, string resource, params HttpVerbs[] methods)
        {
            if (methods.Length == 0)
                methods = new[] {HttpVerbs.Get};

            return GenerateRoute(builder,
                $"{ResourcePath}/{resource}",
                ControllerName,
                action,
                methods.Select(x => x.ToString().ToUpperInvariant()).ToArray());
        }

        public override void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            _subMapper?.Invoke(this);

            var routes = new List<Route>();
            AddIncludedActions(routeBuilder, routes);

            routes.AddRange(
                _members.Select(member => MemberRoute(routeBuilder, member.Key, member.Value.Key, member.Value.Value)));

            foreach (var route in routes)
            {
                ConfigureRoute(route);
                routeBuilder.Routes.Add(route);
            }

            if (Mappers.Any())
            {
                BasePath = ResourcePath;

                AddResourcePath(SingularResourceName);
                RegisterNested(routeBuilder, mapper => mapper.SetParentResources(ResourcePaths));
            }
        }
    }
}