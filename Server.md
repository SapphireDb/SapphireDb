# Realtime Database - Server Configuration

## Installtion

### Install Package
To use the realtime database on server side your first need to install the nuget package.

In an Asp.Net Core project execute

```
PM > Install-Package RealtimeDatabase
```

https://www.nuget.org/packages/RealtimeDatabase/

### Configure DbContext

Create a new context or use an existing and derive from RealtimeDbContext

```csharp
// Change DbContext to RealtimeDbContext
public class MyDbContext : RealtimeDbContext
{
    //Add RealtimeDatabaseNotifier for DI
    public RealtimeContext(DbContextOptions<RealtimeContext> options, RealtimeDatabaseNotifier notifier) : base(options, notifier)
    {

    }

    public DbSet<User> Users { get; set; }

    public DbSet<Test> Tests { get; set; }
    
    ...
}
```

### Register the realtime database service

In the service configuration (normally in Startup.cs) add your DbContext and RealtimeDatabase

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    // Register services
    services.AddRealtimeDatabase<MyDbContext>();
    services.AddDbContext<MyDbContext>(cfg => ...));
    ...
}
```

### Configure Request Pipeline

Add RealtimeDatabase in your pipeline configuration

```csharp

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    ...
    //Add Middleware
    app.UseRealtimeDatabase();
}
```

When using Authentication make sure to call it before RealtimeDatabase

## Configuration

### Make Entity Properties Updatable

To make properties of an Entity updatable using the update method of the realtime collection
you have to add the `UpdatableAttribute` to the class or properties of it

Make all properties of the class updatable:
```csharp
[Updatable]
public class User : Base
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; }

    [Required]
    [MinLength(3)]
    public string FirstName { get; set; }

    [Required]
    [MinLength(3)]
    public string LastName { get; set; }
}
```

Only make Username updatable:
```csharp
public class User : Base
{
    [Updatable]
    [Required]
    [MinLength(3)]
    public string Username { get; set; }

    [Required]
    [MinLength(3)]
    public string FirstName { get; set; }

    [Required]
    [MinLength(3)]
    public string LastName { get; set; }
}
```

### Authentication

If you only want to allow authenticated users access to specific data
you can simply add the `RealtimeAuthorizeAttribute` to your Entity Class

```csharp
[RealtimeAuthorize()]
public class User : Base
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; }

    [Required]
    [MinLength(3)]
    public string FirstName { get; set; }

    [Required]
    [MinLength(3)]
    public string LastName { get; set; }
}
```

#### Use JWT for authentication

If you want to use JWT as an authentication method you have to use a little trick,
because the browser WebSocket is not able to send custom headers.
You have to enable the usage of the bearer parameter as JWT.

Use this snippet to enable that:
```csharp
services.AddAuthentication(cfg => {
    cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(cfg => {
    cfg.Events = new JwtBearerEvents()
    {
        OnMessageReceived = ctx =>
        {
            // When using JWT as authentication for WebSocket add this
            // to enable authentication
            string bearer = ctx.Request.Query["bearer"];
            if (!String.IsNullOrEmpty(bearer))
            {
                ctx.Token = bearer;
            }

            return Task.CompletedTask;
        }
    };
});
```

### Authorization

If you want to restrict the access to specific operation by roles
you can also use the `RealtimeAuthorizeAttribute` in your Entity class.

To allow all operations for the roles admin and user use:
```csharp
[RealtimeAuthorize("admin, user")]
public class User : Base
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; }

    [Required]
    [MinLength(3)]
    public string FirstName { get; set; }

    [Required]
    [MinLength(3)]
    public string LastName { get; set; }
}
```

Or an examle where roles read and read2 are allowed to read, 
role write to write and role delete to delete
```csharp
[RealtimeAuthorize("read, read2", "write", "delete")]
public class User : Base
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; }

    [Required]
    [MinLength(3)]
    public string FirstName { get; set; }

    [Required]
    [MinLength(3)]
    public string LastName { get; set; }
}
```