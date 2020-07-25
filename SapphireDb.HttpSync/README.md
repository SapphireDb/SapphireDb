# SapphireDb.HttpSync - Http sync module for SapphireDb

SapphireDb supports running in multiple instances. Each instance will synchronize with the other instances using different mechanisms.

If you only have a predefined (and small) number of instances and don't need dynamic scaling you can use this option. SapphireDb will sync changes through a http-interface and send them to all other known instances.

## Installation

### Install package
To install the package execute the following command in your package manager console

````
PM> Install-Package SapphireDb.HttpSync
````

You can also install the extension using Nuget package manager. The project can be found here: [https://www.nuget.org/packages/SapphireDb.HttpSync/](https://www.nuget.org/packages/SapphireDb.HttpSync/)

### Register services and update pipeline

To use the SapphireDb.HttpSync you have to make some changes in your `Startup.cs`-File.

````csharp
public class Startup
{
  public void ConfigureServices(IServiceCollection services)
  {
    HttpSyncConfiguration httpSyncConfiguration = new HttpSyncConfiguration(Configuration.GetSection("HttpSync"));

    services.AddSapphireDb(...)
      .AddContext<MyDbContext>(cfg => ...)
      .AddHttpSync(httpSyncConfiguration);
  }

  public void Configure(IApplicationBuilder app)
  {
    //Add Middleware
    app.UseSapphireDb();
    app.UseSapphireHttpSync();
  }
}
````

## Documentation

Check out the documentation for more details: [Documentation](https://sapphire-db.com/)

## Implementations/Packages

### Server

[SapphireDb - Server for Asp.Net Core](https://github.com/morrisjdev/SapphireDb)

[SapphireDb.RedisSync](https://github.com/SapphireDb/SapphireDb/tree/master/SapphireDb.RedisSync)

[SapphireDb.HttpSync](https://github.com/SapphireDb/SapphireDb/tree/master/SapphireDb.HttpSync)

### Client

[sapphiredb - JS client (JS, NodeJs, React, Svelte, ...)](https://github.com/SapphireDb/sapphiredb-js/blob/master/projects/sapphiredb/README.md)

[ng-sapphiredb - Angular client](https://github.com/SapphireDb/sapphiredb-js/blob/master/projects/ng-sapphiredb/README.md)

## Author

[Morris Janatzek](http://morrisj.net) ([morrisjdev](https://github.com/morrisjdev))

## Licenses

SapphireDb - [MIT License](https://github.com/SapphireDb/SapphireDb/blob/master/LICENSE)

sapphiredb-js - [MIT License](https://github.com/SapphireDb/sapphiredb-js/blob/master/LICENSE)
