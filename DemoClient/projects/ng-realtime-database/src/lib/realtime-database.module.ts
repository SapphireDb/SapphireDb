import {NgModule} from '@angular/core';
import {RealtimeDatabase} from './realtime-database.service';
import {RealtimeDatabaseOptions} from './models/realtime-database-options';
import {WebsocketService} from './websocket.service';
import {CollectionInformationService} from './collection-information.service';
import {CollectionManagerService} from './collection-manager.service';

const defaultOptions: RealtimeDatabaseOptions = {
  serverBaseUrl: `${location.hostname}:${location.port}`,
  useSecuredSocket: location.protocol === 'https'
};

@NgModule({
  imports: [],
  declarations: [],
  providers: [
    RealtimeDatabase,
    WebsocketService,
    CollectionInformationService,
    CollectionManagerService,
    { provide: 'realtimedatabase.options', useValue: defaultOptions}
  ]
})
export class RealtimeDatabaseModule {
  static config(options: RealtimeDatabaseOptions) {
    return {
      ngModule: RealtimeDatabaseModule,
      providers: [
        { provide: 'realtimedatabase.options', useValue: options }
      ]
    };
  }
}
