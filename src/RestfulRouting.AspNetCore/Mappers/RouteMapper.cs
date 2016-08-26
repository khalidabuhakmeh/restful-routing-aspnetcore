using Microsoft.AspNetCore.Routing;

namespace RestfulRouting.Mappers
{
    public class RouteMapper : Mapper
    {
        private readonly RouteBase _routeBase;

        public RouteMapper(RouteBase routeBase)
        {
            _routeBase = routeBase;
        }

        public override void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.Routes.Add(_routeBase);
        }
    }
}