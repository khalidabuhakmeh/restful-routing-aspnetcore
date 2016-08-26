using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace RestfulRouting.Mappers
{
    public interface IMapper
    {
        void Root<TController>(Expression<Func<TController, object>> action);
        void Route(RouteBase routeBase);
        void Resources<TController>(Action<IResourcesMapper<TController>> mapper = null) where TController : Controller;
        void Resource<TController>(Action<IResourceMapper<TController>> mapper = null) where TController : Controller;
        void Area<TController>(string name, Action<IAreaMapper> mapper = null) where TController : Controller;

        void Area<TController>(string name, string pathPrefix, Action<IAreaMapper> mapper)
            where TController : Controller;

        void Area(string name, Action<IAreaMapper> mapper);
        void Area(string name, string pathPrefix, Action<IAreaMapper> mapper);
        StandardMapper Path(string path);
        void Connect<TRouteSet>(string[] namespaces = null) where TRouteSet : RouteSet, new();
    }
}