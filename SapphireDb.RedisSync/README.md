# SapphireDb.RedisSync - Redis sync module for SapphireDb

SapphireDb supports running in multiple instances. Each instance will synchronize with the other instances using different mechanisms.

If you want to scale dynamically you don't know the instances in advance. You therefor need a kind of message broker that distributes changes to all application instances. 

SapphireDb.RedisSync provides functionality that uses Redis to achieve this goal.

## Installation

### Install package
To install the package execute the following command in your package manager console

````
PM> Install-Package SapphireDb.RedisSync
````

You can also install the extension using Nuget package manager. The project can be found here: [https://www.nuget.org/packages/SapphireDb.RedisSync/](https://www.nuget.org/packages/SapphireDb.RedisSync/)

### Register services

To use the SapphireDb.RedisSync you have to make some changes in your `Startup.cs`-File.

````csharp
public class Startup
{
  public void ConfigureServices(IServiceCollection services)
  {
    RedisSyncConfiguration redisSyncConfiguration = new RedisSyncConfiguration(Configuration.GetSection("RedisSync"));
    
    services.AddSapphireDb(...)
      .AddContext<MyDbContext>(cfg => ...)
      .AddRedisSync(redisSyncConfguraiton);
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
