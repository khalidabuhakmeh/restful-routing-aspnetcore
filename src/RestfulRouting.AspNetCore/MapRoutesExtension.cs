using Microsoft.AspNetCore.Routing;

namespace RestfulRouting
{
    public static class MapRoutesExtension
    {
        public static void UseRestfulRouting<TRoutes>(this IRouteBuilder routes, string[] namespaces = null)
            where TRoutes : RouteSet, new()
        {
            new TRoutes().RegisterRoutes(routes, namespaces);
        }
    }
}