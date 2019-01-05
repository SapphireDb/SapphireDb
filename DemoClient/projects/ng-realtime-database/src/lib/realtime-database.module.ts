import {NgModule} from '@angular/core';
import {RealtimeDatabase} from './realtime-database.service';
import {RealtimeDatabaseOptions} from './models/realtime-database-options';
import {WebsocketService} from './websocket.service';
import {CollectionInformationService} from './collection-information.service';
import {CollectionManagerService} from './collection-manager.service';
import {RealtimeAuthGuard} from './realtime-auth.guard';

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
    { provide: 'realtimedatabase.options', useValue: defaultOptions},
    RealtimeAuthGuard
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
