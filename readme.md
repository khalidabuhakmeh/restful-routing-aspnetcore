# Restful Routing (ASP.NET Core)

This is an initial port of [Restful Routing](http://restfulrouting.com) that works with the new ASP.NET Core MVC.

There are features stripped out that I feel are probably better suited for Middleware. 

There is also a working sample in RestfulRouting.Sample.

This is far from complete, but works decently well. 

## Startup.cs

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    loggerFactory.AddConsole(Configuration.GetSection("Logging"));
    loggerFactory.AddDebug();

    app.UseDeveloperExceptionPage();
    app.UseSession();

    app.UseMvc(routes =>
    {
        routes.UseRestfulRouting<Routes>();
    });
}
```

## Routes.cs

```csharp
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
```