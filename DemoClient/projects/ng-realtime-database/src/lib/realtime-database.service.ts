import {Injectable} from '@angular/core';
import {Collection} from './models/collection';
import {CollectionManagerService} from './collection-manager.service';
import {WebsocketService} from './websocket.service';
import {ExecuteCommand} from './models/command/execute-command';
import {concatMap, finalize, map, takeWhile} from 'rxjs/operators';
import {ExecuteResponse, ExecuteResponseType} from './models/response/execute-response';
import {Observable, of} from 'rxjs';
import {ActionResult} from './models/action-result';
import {Messaging} from './models/messaging';
import {Auth} from './models/auth';

@Injectable()
export class RealtimeDatabase {
  /**
   * Realtime messaging API
   */
  public messaging: Messaging;

  /**
   * Realtime Auth
   */
  public auth: Auth;

  constructor(private collectionManager: CollectionManagerService, private websocket: WebsocketService) {
    this.messaging = new Messaging(this.websocket);
    this.auth = new Auth(this.websocket);
  }

  /**
   * Get the named collection
   * @param collectionName The name of the collection
   */
  public collection<T>(collectionName: string): Collection<T> {
    return this.collectionManager.getCollection(collectionName);
  }

  /**
   * Reconnect to the websocket with authentication
   * @param bearer The JWT Token to use for authentication/authorization, if empty removes the JWT Token
   */
  public setBearer(bearer?: string) {
    this.websocket.setBearer(bearer);
  }

  /**
   * Execute an action on the server
   * @param handlerName The name of the handler
   * @param actionName The name of the action
   * @param parameters Parameters for the action
   */
  public execute<X, Y>(handlerName: string, actionName: string, ...parameters: any[]): Observable<ActionResult<X, Y>> {
    return this.websocket.sendCommand(new ExecuteCommand(handlerName, actionName, parameters), true).pipe(
      concatMap((result: ExecuteResponse) => {
        if (result.type === ExecuteResponseType.End) {
          return of(result, null);
        }

        return of(result);
      }),
      takeWhile(v => !!v),
      map((result: ExecuteResponse) => {
        return new ActionResult<X, Y>(result);
      })
    );
  }
}
