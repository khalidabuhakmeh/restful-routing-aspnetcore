using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Routing;

namespace RestfulRouting.Mappers
{
    public class RootMapper<TController> : StandardMapper
    {
        private readonly Expression<Func<TController, object>> _action;

        public RootMapper(Expression<Func<TController, object>> action)
        {
            _action = action;
        }

        public override void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            Path("").To(_action).Named(JoinResources("root")).WithNamespace<TController>();
            base.RegisterRoutes(routeBuilder);
        }
    }
}