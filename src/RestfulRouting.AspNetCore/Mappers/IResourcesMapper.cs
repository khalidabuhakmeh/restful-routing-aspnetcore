using System;
using Microsoft.AspNetCore.Mvc;

namespace RestfulRouting.Mappers
{
    public interface IResourcesMapper<TController> : IResourcesMapperBase where TController : Controller
    {
        void Collection(Action<AdditionalAction> action);
        void Member(Action<AdditionalAction> action);
        void Constrain(string key, object value);
    }
}