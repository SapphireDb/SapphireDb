# Realtime Database - Angular Configuration

## Installtion

### Install Package
To use realtime database in client you have to install the package using node.js

In your Angular App-Folder execute

```
npm install ng-realtime-database linq4js -S
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

To access the realtime database you need to inject realtime database where you need it.

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

To query data 
