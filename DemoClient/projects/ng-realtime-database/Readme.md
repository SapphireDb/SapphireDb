# ng-realtime-database

Realtime Database Client - Angular Configuration

## Installation

### Install Package
To use realtime database in client you have to install the package using node.js

In your Angular App-Folder execute

```
npm install ng-realtime-database -S
```

### Import realtime database module in your app.module

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
        serverBaseUrl: `${location.hostname}:${location.port}`,
        useSecuredSocket: false
    }) 
]
```

### Use it where you need it

To access the realtime database you need to inject `RealtimeDatabase` where you need it.

Example:
```
@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.less']
})
export class MainComponent implements OnInit {

  constructor(private db: RealtimeDatabase) { }

  ngOnInit() {
    this.db.collection('example').values().subscribe(console.log);
  }
}
```

## Query data

To query data you can just execute `.snapshot()` on your collection.

```js
// returns Observable<IUser[]>
const collection = this.db.collection<IUser>('users').snapshot();
```

The `.snapshot()` function only queries a snapshot of your collection.
The client will not get updates, after the query was made.
The observable completes after the client received the data.

## Subscribe data

If you want your client to also get notified when the collection changes
you have to use `.values()`.

```js
// returns Observable<IUser[]>
const collection = this.db.collection<IUser>('users').values();
```

The `.values()` function returns an observable
with the values of the collection. If the collection changes the
observable emits the new array. The observable does not complete,
so make sure to unsubscribe it.

## Filter data

If you don't want to query the whole collection in your client you
can use prefilters in both methods `.snapshot()` and `.values()`.
The prefilter filters the data at server side and only sends relevant data
to the client.

Example: You only want the last 10 values of your collection,
ordered by username.

```js
const collection = this.db.collection<IUser>('user').snapshot(
  new OrderByPrefilter('id', true),
  new TakePrefilter(10),
  new OrderByPrefilter('username')
);

const collection2 = this.db.collection<IUser>('user').values(
  new OrderByPrefilter('id', true),
  new TakePrefilter(10),
  new OrderByPrefilter('username')
);
```

### Available filter

#### WherePrefilter

This prefilter is used to filter your data.

The syntax is:
```js
// propertyName: needs to be a valid property in your model
// comparison: '==' | '!=' | '<' | '<='| '>' | '>='
// compareValue: any value you want compare with the property
new WherePrefilter(propertyName, comparison, compareValue);
```

Example: Only take values with username test
```js
new WherePrefilter('username', '==', 'test');
```

#### OrderByPrefilter

This prefilter is used for sorting.

The syntax is:
```js
// propertyName: needs to be a valid property in your model
// descending: false | true
new OrderByPrefilter(propertyName, descending);
```

Example: Order the collection by username from z-a (descending)
```js
new OrderByPrefilter('username', true);
```

#### ThenOrderByPrefilter

This prefilter is used for sorting by a second or more properties.
You first have to first use an OrderByPrefilter.

The syntax is:
```js
// propertyName: needs to be a valid property in your model
// descending: false | true
new ThenOrderByPrefilter(propertyName, descending);
```

Example: Order the collection by username from z-a (descending)
and then by name.
```js
new OrderByPrefilter('username', true),
new ThenOrderByPrefilter('name')
```

#### SkipPrefilter

This prefilter is used to skip a specific count of items in your collection.

The syntax is:
```js
// number: The number of items to skip
new SkipPrefilter(number);
```

Example: Skip the first 15 values
```js
new SkipPrefilter(15);
```

#### TakePrefilter

This prefilter is used to limit the number of items queried.

The syntax is:
```js
// number: The number of items to take
new TakePrefilter(number);
```

Example: Only take a maximum of 15 items
```js
new TakePrefilter(15);
```

## Add data

To add data use `.add()` function on collection

Example: Add User to collection user

```js
this.db.collection('user').add({
  username: 'test123',
  name: 'Test User'
});
```

The value to add has to be a valid model. You can use annotations at server side
to check the model. If there are validation errors the returned observable of the
function will emit an object with information about it.

## Update data

To update data use `.update()` function on collection

Example: Add User in collection user

```js
this.db.collection('user').update({
  id: 32,
  username: 'test123',
  name: 'Test User'
});
```

The new model has to be valid. Also you need to send the primary keys of the
model.

## Remove data

To delete data use `.remove()` function on collection

Example: Remove User from collection user

```js
this.db.collection('user').remove({
  id: 32,
  username: 'test123',
  name: 'Test User'
});
```

The model is removed from the collection using the primary key(s). You only need
to pass this information to the server.

## Security

If you want to use a JWT to secure your backend you have to
also send it to realtime database. This can be done by using the function
`.setBearer()`

Example:
```js
db.setBearer('example JWT Token');
```

The websocket will now refresh and use the JWT.

Make sure to configure the backend to load the JWT from the query parameter.
See [Server Configuration](Server.md) to learn more.

### Check role access

If you secured your models at server side you sometimes want to find out if a specific 
user can access specific operations on the model.

You can get this information by using the following methods:

#### Query

Check if the collection user requires authentication for queries by using `queryAuth()`:

````
this.db.collection('user').authInfo.queryAuth();
````

Check if any of the roles can query the collection user:

````
this.db.collection('user').authInfo.canQuery(['user', 'admin']);
````

To check if a property needs authentication to query use `queryPropertyAuth()`:
````
this.db.collection('user').authInfo.queryPropertyAuth('firstName');
````

To check if the property can get queried by specific roles:
````
this.db.collection('user').authInfo.canQueryProperty('firstName', ['user', 'admin']);
````

#### Update

Check if the collection user requires authentication for updates by using `updateAuth()`:

````
this.db.collection('user').authInfo.updateAuth();
````

Check if any of the roles can update the collection user:

````
this.db.collection('user').authInfo.canUpdate(['user', 'admin']);
````

To check if a property needs authentication to update use `updatePropertyAuth()`:
````
this.db.collection('user').authInfo.updatePropertyAuth('firstName');
````

To check if the property can get updated by specific roles:
````
this.db.collection('user').authInfo.canUpdateProperty('firstName', ['user', 'admin']);
````

#### Create

Check if the collection user requires authentication to add data by using `createAuth()`:

````
this.db.collection('user').authInfo.createAuth();
````

Check if any of the roles can create objects in the collection user:

````
this.db.collection('user').authInfo.canCreate(['user', 'admin']);
````

#### Remove

Check if the collection user requires authentication to remove data by using `removeAuth()`:

````
this.db.collection('user').authInfo.removeAuth();
````

Check if any of the roles can remove objects from the collection user:

````
this.db.collection('user').authInfo.canRemove(['user', 'admin']);
````

## Realtime Auth
Realtime auth provides a JWT Auth Provider. You can use it if the server is configured.

### Login

To login the user use:

```
this.db.auth.login(username, password).subscribe(...);
```

### Get User Data
```
this.db.auth.getUserData();
```

### Logout
```
this.db.auth.logout();
```

### Check if logged in
```
this.db.auth.isLoggedIn();
```

## Actions

You can call actions at server using the websocket connection
realtime database already adds. You first have to define those actions
at server side.

Then you can invoke this actions using:
````
this.db.execute('example', 'GenerateRandomNumber', parameters);
````

This will return an observable that returns an action result.
You can simply subscribe and use a custom function to react to the
results. The actions at server side can also send notification during
execution for example as a progress indicator. To handle this the recommended
way is using `ActionHelper`:

````
this.db.execute('example', 'GenerateRandomNumber')
  .subscribe(ActionHelper.result<number, string>(
    v => console.log('Result: ' + v),
    v => console.log('Notification: ' + v)));
````

Now you can react to the results and notifications in separate
functions.
The observable of the `execute`-function completes after the result
was send.

Example with parameters:
````
this.db.execute('example', 'TestWithParams', 'test1234', 'test2345')
  .subscribe(console.log);
````

## Messaging

You can use realtime database for communication.


### Receive Messages
The client can receive general messages using:

```
this.db.messaging.messages().subscribe(console.warn);
```

### Subscribe to topic
You can also subscribe to a topic and only get messages of this category.

```
this.db.messaging.topic('test').subscribe(alert);
```

### Send a message
You can send a message to all client using:
```
this.db.messaging.send({data: this.message});
```

You can also publish a message to a topic using:
```
this.db.messaging.publish('test', this.message);
```
