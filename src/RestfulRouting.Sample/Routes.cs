using RestfulRouting.Mappers;
using RestfulRouting.Sample.Controllers;
using RestfulRouting.Sample.Controllers.DangerZone;

namespace RestfulRouting.Sample
{
    public class Routes : RouteSet
    {
        public override void Map(IMapper map)
        {
            map.Root<HomeController>(c => c.Index());
            map.Resources<BlogsController>(blogs =>
            {
                blogs.As("weblogs");
                blogs.Only("index", "show");
                blogs.Collection(x => x.Get("latest"));
                blogs.Resources<PostsController>(posts =>
                {
                    posts.Except("create", "update", "destroy");
                    posts.Resources<CommentsController>(c => c.Except("destroy"));
                });
            });
            map.Connect<AnotherRouteSet>();
        }
    }

    public class AnotherRouteSet : RouteSet
    {
        public override void Map(IMapper map)
        {
            map.Resource<AnotherController>();
            map.Area<SpiesController>("danger-zone", a =>
            {
                a.Resources<SpiesController>();
            });
        }
    }
}