import {Injectable} from '@angular/core';
import {Collection} from './models/collection';
import {CollectionManagerService} from './collection-manager.service';
import {WebsocketService} from './websocket.service';

@Injectable()
export class RealtimeDatabase {
  constructor(private collectionManager: CollectionManagerService, private websocket: WebsocketService) {}

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
}
