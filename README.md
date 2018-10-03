# RealtimeDatabase
Realtime Database is an extension for Asp.Net Core and Entity Framework.
It enables transport of data using websockets.
The client can subscribe to specific collection data and gets notified on changes.
This enables realtime functionality and all client get the same instance.

## Advantages

- Simple configuration
- Communication over Websockets
- Comes with all you need: Create, Update, Delete functionality on collections
- Server side prefilters to only query the data your client needs
- Authentication and Authorization integrated
- Extends Entity Framework functionality 
(normal operations on the context are also possible and changes are published to subscribing clients)

## Install

### Server

In an Asp.Net Core project execute

```
PM > Install-Package RealtimeDatabase
```

https://www.nuget.org/packages/RealtimeDatabase/


#### Use RealtimeDbContext instead of DbContext

```csharp
public class MyDbContext : RealtimeDbContext
{
    //Add RealtimeDatabaseNotifier for DI
    public RealtimeContext(DbContextOptions<RealtimeContext> options, RealtimeDatabaseNotifier notifier) : base(options, notifier)
    {

    }

    public DbSet<User> Users { get; set; }

    public DbSet<Test> Tests { get; set; }
}
```

#### Register the realtime database service
```csharp
services.AddRealtimeDatabase<MyDbContext>();
services.AddDbContext<MyDbContext>(cfg => ...));
```

#### Configure the request pipeline
```csharp
app.UseRealtimeDatabase();
```

### Client

Install the Angular client using

```
npm install ng-realtime-database linq4js
```

#### Import realtime database module in your app.module

```
imports: [
    BrowserModule,
    ...,
    RealtimeDatabaseModule, 
]
```

or using custom configuration
```
imports: [
    BrowserModule,
    ...,
    RealtimeDatabaseModule.config({
        serverBaseUrl: `${location.hostname}:${location.port}`
    }) 
]
```

#### Use RealtimeDatabase where you need it

```
constructor(private db: RealtimeDatabase) { }
```