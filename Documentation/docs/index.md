# Realtime database

## Overview

Realtime database is a provider for Asp.Net Core and Entity Framework.
It enables realtime transport of data using websockets.
By subscribing to specific collections the clients can get notified about changes in the database.
This enables easy data synchronization.

??? success "Easy configuration"

    Realtime database comes with all you need preconfigured.
    Only a few steps to make your app realtime capable.
    Just install and play around.

??? success "No need to write REST-interfaces"
    
    Realtime database comes with all necessary methods for a collection.
    You can query, create, update and delete things without writing a single line
    of code on your server.
    
    Realtime database also comes with server side prefilters to only query
    the data your client needs


??? success "Communication over websockets"

    Realtime database communicates over websockets.
    Not only is websocket a modern web technology.
    It also performs very well and reduces data because things like headers
    cookies etc. only have to get transfered once.

??? success "Validation"

    Realtime database is also able to validate your models send to the server.
    You can check the validity using Annotations.

??? success "Authentication and authorization integrated"

    Realtime database comes with a JWT provider and comes with functionality for 
    authenticating and authorizing the clients.

??? success "Extends existing code"
    
    The library is build on top of entity framework. If you already have a database context
    with models etc. you do not have change anything. All actions on the context anywhere else
    in your project will also synchronized with the clients.

## Quick start

- Server
    1. Install RealtimeDatabase package
        ```
        PM > Install-Package RealtimeDatabase
        ```
        
    2. Register the services
        ``` java
        services.AddRealtimeDatabase<MyDbContext>();
        services.AddDbContext<MyDbContext>(cfg => ...);
        ```
    
    3. Configure Request pipeline
        ``` java 
        app.UseRealtimeDatabase();
        ```
    
- Client
    1. Install package
        ``` 
        npm install ng-realtime-database -S
        ```
    
    2. Import realtime database module
        ``` typescript 
        imports: [
            BrowserModule,
            ...,
            RealtimeDatabaseModule, 
        ]
        ```
    
    3. Query a collection
        ``` 
        constructor(private db: RealtimeDatabase) { }
                
        ngOnInit() {
            this.db.collection('example').values().subscribe(console.log);
        }
        ```

## Author

[Morris Janatzek](http://morrisj.net) ([morrisjdev](https://github.com/morrisjdev))

