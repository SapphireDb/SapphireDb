import { Injectable } from '@angular/core';
import {WebsocketService} from './websocket.service';
import {BehaviorSubject, Observable} from 'rxjs';
import {InfoResponse} from './models/response/info-response';
import {InfoCommand} from './models/command/info-command';
import {filter, take} from 'rxjs/operators';

@Injectable()
export class CollectionInformationService {
  private collectionInformation: { [collectionName: string]: Observable<InfoResponse> } = {};

  constructor(private websocket: WebsocketService) { }

  public getCollectionInformation(collectionName: string) {
    if (this.collectionInformation[collectionName]) {
      return this.collectionInformation[collectionName];
    } else {
      const subject$ = new BehaviorSubject<InfoResponse>(null);
      this.collectionInformation[collectionName] = subject$;
      this.websocket.sendCommand(new InfoCommand(collectionName)).subscribe((info: InfoResponse) => {
        subject$.next(info);
      });

      return this.collectionInformation[collectionName].pipe(
        filter(v => !!v),
        take(1)
      );
    }
  }
}
