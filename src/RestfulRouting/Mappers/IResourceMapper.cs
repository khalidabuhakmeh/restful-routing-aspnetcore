using System;
using Microsoft.AspNetCore.Mvc;

namespace RestfulRouting.Mappers
{
    public interface IResourceMapper<TController> : IResourcesMapperBase where TController : Controller
    {
        void Member(Action<AdditionalAction> action);
    }
}