using System;

namespace RestfulRouting.Mappers
{
    public interface IResourcesMapperBase : IMapper
    {
        void As(string resourceName);
        void IdParameter(string name);
        void Except(params string[] actions);
        void Only(params string[] actions);
        void PathNames(Action<RouteNames> action);
    }
}