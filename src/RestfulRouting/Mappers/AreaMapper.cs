using System;
using Microsoft.AspNetCore.Routing;

namespace RestfulRouting.Mappers
{
    public class AreaMapper : Mapper, IAreaMapper
    {
        private readonly string _areaName;
        private readonly string _ns;
        private readonly string _pathPrefix;
        private readonly Action<AreaMapper> _subMapper;

        public AreaMapper(string areaName, string pathPrefix = null, string _namespace = null,
            Action<AreaMapper> subMapper = null)
        {
            _areaName = areaName;
            _pathPrefix = pathPrefix ?? areaName;
            _ns = _namespace;
            _subMapper = subMapper;
        }

        public override void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            _subMapper?.Invoke(this);
            var routes = new RouteBuilder(routeBuilder.ApplicationBuilder, routeBuilder.DefaultHandler);
            BasePath = Join(BasePath, _pathPrefix);
            AddResourcePath(_pathPrefix);
            RegisterNested(routes, mapper => mapper.SetParentResources(ResourcePaths));

            foreach (var route in routes.Routes)
            {
                ConstrainArea(route as RouteBase);
                routeBuilder.Routes.Add(route);
            }
        }

        private void ConstrainArea(RouteBase route)
        {
            if (!string.IsNullOrEmpty(_ns) && !route.DataTokens.ContainsKey("area"))
                route.DataTokens["namespaces"] = new[] {_ns};
            if (!string.IsNullOrEmpty(_areaName) && !route.DataTokens.ContainsKey("area"))
                route.DataTokens["area"] = _areaName;
            route.DataTokens["UseNamespaceFallback"] = false;
        }
    }
}