import {Injectable} from '@angular/core';
import {Collection} from './models/collection';
import {WebsocketService} from './websocket.service';
import {CollectionInformationService} from './collection-information.service';

@Injectable()
export class CollectionManagerService {

  private collections: Collection<any>[] = [];

  constructor(private websocket: WebsocketService, private collectionInformation: CollectionInformationService) { }

  public getCollection<T>(collectionName: string) {
    const foundCollections = this.collections.filter(c => c.collectionName === collectionName);

    if (foundCollections.length > 0) {
      return foundCollections[0];
    } else {
      const newCollection = new Collection<any>(
        collectionName,
        this.websocket,
        this.collectionInformation.getCollectionInformation(collectionName));

      this.collections.push(newCollection);
      return newCollection;
    }
  }
}
