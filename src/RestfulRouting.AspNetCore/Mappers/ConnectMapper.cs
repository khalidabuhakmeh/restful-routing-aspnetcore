using Microsoft.AspNetCore.Routing;

namespace RestfulRouting.Mappers
{
    public class ConnectMapper<TRouteSet> : Mapper where TRouteSet : RouteSet, new()
    {
        public override void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            var routeSet = new TRouteSet();
            routeSet
                .RegisterRoutes(routeBuilder, Namespaces);
        }
    }
}