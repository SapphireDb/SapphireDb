# Installation on server

## Install package

To use the realtime database on server side your first need to install the nuget package.

In an Asp.Net Core project execute:

```
PM > Install-Package RealtimeDatabase
```

[https://www.nuget.org/packages/RealtimeDatabase/](https://www.nuget.org/packages/RealtimeDatabase/)

## Create/Configure DbContext

Create a new db context or use an existing and change the base class from `DbContext` to `RealtimeDbContext`

``` csharp
// Change DbContext to RealtimeDbContext
public class MyDbContext : RealtimeDbContext
{
    //Add RealtimeDatabaseNotifier for DI
    public RealtimeContext(DbContextOptions<RealtimeContext> options, RealtimeDatabaseNotifier notifier)
        : base(options, notifier)
    {

    }

    public DbSet<User> Users { get; set; }

    public DbSet<Test> Tests { get; set; }
    
    ...
}
```

## Register the realtime database service

In the service configuration (normally in Startup.cs) add your RealtimeDbContext and also RealtimeDatabase

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    // Register services
    services.AddRealtimeDatabase<MyDbContext>();
    services.AddDbContext<MyDbContext>(cfg => ...));
    ...
}
```

## Configure Request Pipeline

Add RealtimeDatabase in your pipeline configuration

``` csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    ...
    //Add Middleware
    app.UseRealtimeDatabase();
}
```

!!! info "Call authentication before"

    When using Authentication make sure to call it before `UseRealtimeDatabase`