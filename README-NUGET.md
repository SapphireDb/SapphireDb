# SapphireDb - Server for Asp.Net Core [![Build Status](https://travis-ci.org/morrisjdev/RealtimeDatabase.svg?branch=master)](https://travis-ci.org/morrisjdev/RealtimeDatabase)

SapphireDb is a self-hosted, easy to use realtime database for Asp.Net Core and EF Core.

It creates a generic API you can easily use with different clients to effortlessly create applications with realtime data synchronization.
SapphireDb should serve as a self hosted alternative to firebase realtime database and firestore on top of .Net.

Check out the documentation for more details: [Documentation](https://sapphire-db.com/)

## Features

- :wrench: Dead simple configuration
- :satellite: Broad technology support
- :computer: Self hosted
- :iphone: Offline support
- :floppy_disk: Easy to use CRUD operations
- :zap: Model validation
- :heavy_check_mark: Database support
- :open_file_folder: Supports joins/includes
- :loop: Complex server evaluated queries
- :electric_plug: Actions
- :key: Authorization included
- :envelope: Messaging
- :globe_with_meridians: Scalable

[Learn more](https://sapphire-db.com/)


## Installation

### Install package
To install the package execute the following command in your package manager console

````
PM> Install-Package SapphireDb
````

You can also install the extension using Nuget package manager. The project can be found here: [https://www.nuget.org/packages/SapphireDb/](https://www.nuget.org/packages/SapphireDb/)

### Configure DbContext

You now have to change your DbContext to derive from `SapphireDbContext`. Also make sure to adjust the constructor parameters.

````csharp
// Change DbContext to SapphireDbContext
public class MyDbContext : SapphireDbContext
{
  //Add ISapphireDatabaseNotifier for DI
  public MyDbContext(DbContextOptions<MyDbContext> options, ISapphireDatabaseNotifier notifier) : base(options, notifier)
  {

  }

  public DbSet<User> Users { get; set; }

  public DbSet<Test> Tests { get; set; }
}
````

### Register services and update pipeline

To use the SapphireDb you also have to make some changes in your `Startup.cs`-File.

````csharp
public class Startup
{
  public void ConfigureServices(IServiceCollection services)
  {
    //Register services
    services.AddSapphireDb(...)
      .AddContext<MyDbContext>(cfg => ...);
  }

  public void Configure(IApplicationBuilder app)
  {
    //Add Middleware
    app.UseSapphireDb();
  }
}
````

## Examples

### Server

[AspNet Core Example](https://github.com/SapphireDb/Example-AspNetCore)

### Client

[React Example](https://github.com/SapphireDb/Example-React)

[Svelte Example](https://github.com/SapphireDb/Example-Svelte)

[NodeJs Example](https://github.com/SapphireDb/Example-NodeJs)

[Angular Example](https://github.com/SapphireDb/Example-Angular)

## Documentation

Check out the documentation for more details: [Documentation](https://sapphire-db.com/)

## Implementations

### Server

[SapphireDb - Server for Asp.Net Core](https://github.com/morrisjdev/SapphireDb)

### Client

[sapphiredb - JS client (JS, NodeJs, React, Svelte, ...)](https://github.com/SapphireDb/sapphiredb-js/blob/master/projects/sapphiredb/README.md)

[ng-sapphiredb - Angular client](https://github.com/SapphireDb/sapphiredb-js/blob/master/projects/ng-sapphiredb/README.md)

## Author

[Morris Janatzek](http://morrisj.net) ([morrisjdev](https://github.com/morrisjdev))

## Licenses

SapphireDb - [MIT License](https://github.com/SapphireDb/SapphireDb/blob/master/LICENSE)

sapphiredb-js - [MIT License](https://github.com/SapphireDb/sapphiredb-js/blob/master/LICENSE)
