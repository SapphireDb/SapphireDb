# Realtime Database [![Build Status](https://travis-ci.org/morrisjdev/RealtimeDatabase.svg?branch=master)](https://travis-ci.org/morrisjdev/RealtimeDatabase) [![Maintainability](https://api.codeclimate.com/v1/badges/a80b67f61f2c952d3b49/maintainability)](https://codeclimate.com/github/morrisjdev/RealtimeDatabase/maintainability)

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

[Installation on Client](https://github.com/morrisjdev/ng-realtime-database/blob/master/README.md)

## Author

[Morris Janatzek](http://morrisj.net) ([morrisjdev](https://github.com/morrisjdev))
