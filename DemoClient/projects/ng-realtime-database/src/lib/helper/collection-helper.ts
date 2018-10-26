import {UnloadResponse} from '../models/response/unload-response';
import {InfoResponse} from '../models/response/info-response';
import {BehaviorSubject, Observable} from 'rxjs';
import {LoadResponse} from '../models/response/load-response';
import {ChangeResponse, ChangeState} from '../models/response/change-response';
import {FilterFunctions} from './filter-functions';

export class CollectionHelper {
  static unloadItem<T>(collectionData: BehaviorSubject<T[]>, info$: Observable<InfoResponse>, unloadResponse: UnloadResponse) {
    info$.subscribe((info: InfoResponse) => {
      const primaryKeys = info.primaryKeys;

      const index = collectionData.value.findIndex(c => {
        let isCorrectElement = true;

        for (let i = 0; i < primaryKeys.length; i++) {
          if (c[primaryKeys[i]] !== unloadResponse.primaryValues[i]) {
            isCorrectElement = false;
            break;
          }
        }

        return isCorrectElement;
      });

      if (index !== -1) {
        collectionData.value.splice(index, 1);
        collectionData.next(collectionData.value);
      }
    });
  }

  static loadItem<T>(collectionData: BehaviorSubject<T[]>, loadResponse: LoadResponse) {
    collectionData.next(collectionData.value.concat([loadResponse.newObject]));
  }

  static updateCollection<T>(collectionData: BehaviorSubject<T[]>, info$: Observable<InfoResponse>, changeResponse: ChangeResponse) {
    info$.subscribe((info: InfoResponse) => {
      if (changeResponse.state === ChangeState.Added) {
        collectionData.next(collectionData.value.concat([changeResponse.value]));
      } else if (changeResponse.state === ChangeState.Modified) {
        const index = collectionData.value.findIndex(
          FilterFunctions.comparePrimaryKeysFunction(info.primaryKeys, changeResponse.value));

        if (index !== -1) {
          collectionData.value[index] = changeResponse.value;
          collectionData.next(collectionData.value);
        }
      } else if (changeResponse.state === ChangeState.Deleted) {
        const index = collectionData.value.findIndex(
          FilterFunctions.comparePrimaryKeysFunction(info.primaryKeys, changeResponse.value));

        if (index !== -1) {
          collectionData.value.splice(index, 1);
          collectionData.next(collectionData.value);
        }
      }
    });
  }
}
