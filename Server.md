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
you have to add the `UpdatableAttribute` to the class or properties of it. All other properties
cannot be changed using the realtime methods at client side.

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

### Authentication/Authorization

You can protect specific actions on entity classes by using the attributes
`QueryAuthAttribute`, `CreateAuthAttribute`, `UpdateAuthAttribute` and `RemoveAuthAttribute`.

If you just use the plain attributes without any configuration they will just enable authentication
for the specific action and model.

For example:
```csharp
[QueryAuth]     // Will require an authenticated request to allow query users
[RemoveAuth]    // Will require an authenticated request to allow remove users
                // All other operations are allowed without authentication
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

You can also define roles that are authorized to perform a specific action:
```csharp
[QueryAuth]             // Will require an authenticated request to allow query
[RemoveAuth("admin")]   // Will require an authenticated request and role 
                        // 'admin' to allow remove
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


The `QueryAuthAttribute` and `UpdateAuthAttribute` can also be used for properties.
You can use it to control query or update of a specific property.
If a property is not queryable beacause the user is not authorized to it is just omitted
and does not get transmitted to the client. The same behavior is used when
an update of a property is not allowed for a user: The property just is omitted and not changed.

```csharp
[QueryAuth]
public class User : Base
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; }

    [Required]
    [MinLength(3)]
    [QueryAuth("admin")]        // Property FirstName can only get queried by 
                                // users with role `admin`
    public string FirstName { get; set; }

    [Required]
    [MinLength(3)]
    [Updatable]
    [UpdateAuth("admin")]       // LastName can only get updated by users with role
                                // `admin`
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