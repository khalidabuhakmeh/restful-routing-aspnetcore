using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace RestfulRouting.Mappers
{
    public class ResourcesMapper<TController> : ResourcesMapperBase<TController>, IResourcesMapper<TController>
        where TController : Controller
    {
        private readonly ResourcesRoutePaths _resourcesRoutePaths = new ResourcesRoutePaths();

        private readonly Dictionary<string, KeyValuePair<string, HttpVerbs[]>> _collections =
            new Dictionary<string, KeyValuePair<string, HttpVerbs[]>>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, KeyValuePair<string, HttpVerbs[]>> _members =
            new Dictionary<string, KeyValuePair<string, HttpVerbs[]>>(StringComparer.OrdinalIgnoreCase);

        private readonly Action<ResourcesMapper<TController>> _subMapper;

        public ResourcesMapper(Action<ResourcesMapper<TController>> subMapper = null)
        {
            IncludedActions = new Dictionary<string, Func<IRouteBuilder, Route>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    Names.IndexName,
                    r =>
                        GenerateRoute(r, JoinResources(ResourceName), BuildPathFor(Paths.Index), ControllerName,
                            Names.IndexName, new[] {"GET"})
                },
                {
                    Names.CreateName,
                    r => GenerateRoute(r, BuildPathFor(Paths.Create), ControllerName, Names.CreateName, new[] {"POST"})
                },
                {
                    Names.NewName,
                    r =>
                        GenerateRoute(r, "new_" + JoinResources(SingularResourceName), BuildPathFor(Paths.New),
                            ControllerName, Names.NewName, new[] {"GET"})
                },
                {
                    Names.EditName,
                    r =>
                        GenerateRoute(r, "edit_" + JoinResources(SingularResourceName), BuildPathFor(Paths.Edit),
                            ControllerName, Names.EditName, new[] {"GET"})
                },
                {
                    Names.ShowName,
                    r =>
                        GenerateRoute(r, JoinResources(SingularResourceName), BuildPathFor(Paths.Show),
                            ControllerName, Names.ShowName, new[] {"GET"})
                },
                {
                    Names.UpdateName,
                    r => GenerateRoute(r, BuildPathFor(Paths.Update), ControllerName, Names.UpdateName, new[] {"PUT"})
                },
                {
                    Names.DestroyName,
                    r =>
                        GenerateRoute(r, BuildPathFor(Paths.Destroy), ControllerName, Names.DestroyName,
                            new[] {"DELETE"})
                }
            };

            if (RouteSet.MapDelete)
                IncludedActions.Add(Names.DeleteName,
                    r =>
                        GenerateRoute(r, "delete_" + JoinResources(ResourceName), BuildPathFor(Paths.Delete),
                            ControllerName, Names.DeleteName, new[] {"GET"}));
            this._subMapper = subMapper;
        }

        protected override RoutePaths Paths => _resourcesRoutePaths;

        public void Collection(Action<AdditionalAction> action)
        {
            var additionalAction = new AdditionalAction(_collections);
            action(additionalAction);
        }

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
                $"{ResourcePath}/{{{IdParameterName}}}/{resource}",
                ControllerName,
                action,
                methods.Select(x => x.ToString().ToUpperInvariant()).ToArray());
        }

        private Route CollectionRoute(IRouteBuilder builder, string action, string resource, params HttpVerbs[] methods)
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

            var routes =
                _collections.Select(
                        collection =>
                                CollectionRoute(routeBuilder, collection.Key, collection.Value.Key, collection.Value.Value))
                    .ToList();
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
                var parentIdParameterName = SingularResourceName + Inflector.Capitalize(IdParameterName);
                BasePath = Join(ResourcePath, $"{{{parentIdParameterName}}}");
                var idConstraint = Constraints[IdParameterName];
                if (idConstraint != null)
                {
                    Constraints.Remove(IdParameterName);
                    Constraints.Add(parentIdParameterName, idConstraint);
                }

                AddResourcePath(SingularResourceName);
                RegisterNested(routeBuilder, mapper => mapper.SetParentResources(ResourcePaths));
            }
        }
    }
}