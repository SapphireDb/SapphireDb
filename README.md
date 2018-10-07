# Realtime Database
Realtime Database is an extension for Asp.Net Core and Entity Framework.
It enables realtime transport of data using websockets.
By subscribing to specific collections the clients get notified on changes of the database.
This enables realtime applications and easy data synchronization.

## Advantages

- Simple configuration
- Communication using Websockets
- Comes with all you need: Create, Update, Delete functionality on collections
- Server side validation of the models using annotations on the entity
- Server side prefilters to only query the data your client needs
- Authentication and Authorization integrated
- Extends Entity Framework functionality 
(normal operations on DbContext are also possible and changes are published to subscribing clients)
- Client uses RxJS

## Install

[Installation on Server](Server.md)

[Installation on Client](Client.md)