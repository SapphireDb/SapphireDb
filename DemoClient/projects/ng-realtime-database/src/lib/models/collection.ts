import 'linq4js';
import {BehaviorSubject, Observable, pipe, Subscription} from 'rxjs';
import {WebsocketService} from '../websocket.service';
import {CreateCommand} from './command/create-command';
import {CreateResponse} from './response/create-response';
import {CommandResult} from './command-result';
import {DeleteResponse} from './response/delete-response';
import {UpdateCommand} from './command/update-command';
import {UpdateResponse} from './response/update-response';
import {DeleteCommand} from './command/delete-command';
import {SubscribeCommand} from './command/subscribe-command';
import {filter, finalize, map, switchMap, take} from 'rxjs/operators';
import {QueryResponse} from './response/query-response';
import {InfoResponse} from './response/info-response';
import {ChangeResponse} from './response/change-response';
import {UnsubscribeCommand} from './command/unsubscribe-command';
import {IPrefilter} from './prefilter/iprefilter';
import {UnloadResponse} from './response/unload-response';
import {LoadResponse} from './response/load-response';
import {CollectionHelper} from '../helper/collection-helper';
import {QueryCommand} from './command/query-command';

export class Collection<T> {
  constructor(public collectionName: string,
              private websocket: WebsocketService,
              private collectionInformation: Observable<InfoResponse>) {

  }

  /**
   * Get a snapshot of the values of the collection
   * @param prefilters Additional prefilters to query only specific data
   */
  public snapshot(...prefilters: IPrefilter[]): Observable<T[]> {
    const queryCommand = new QueryCommand(this.collectionName, prefilters);

    return this.websocket.sendCommand(queryCommand).pipe(
      map((response: QueryResponse) => {
        let array = response.collection;

        for (const prefilter of prefilters) {
          array = prefilter.execute(array);
        }

        return array;
      })
    );
  }

  /**
   * Get the values of the collection and also get updates if the collection has changed
   * @param prefilters Additional prefilters to query only specific data
   */
  public values(...prefilters: IPrefilter[]): Observable<T[]> {
    let wsSubscription: Subscription;

    const collectionData = new BehaviorSubject<T[]>([]);
    const subscribeCommand = new SubscribeCommand(this.collectionName, prefilters);

    wsSubscription = this.websocket.sendCommand(subscribeCommand, true)
      .subscribe((response: (QueryResponse | ChangeResponse | UnloadResponse | LoadResponse)) => {
        if (response.responseType === 'QueryResponse') {
          collectionData.next((<QueryResponse>response).collection);
        } else if (response.responseType === 'ChangeResponse') {
          CollectionHelper.updateCollection<T>(collectionData, this.collectionInformation, <ChangeResponse>response);
        } else if (response.responseType === 'UnloadResponse') {
          CollectionHelper.unloadItem<T>(collectionData, this.collectionInformation, <UnloadResponse>response);
        } else if (response.responseType === 'LoadResponse') {
          CollectionHelper.loadItem<T>(collectionData, <LoadResponse>response);
        }
      });

    return collectionData.pipe(finalize(() => {
      this.websocket.sendCommand(new UnsubscribeCommand(this.collectionName, subscribeCommand.referenceId));

      if (wsSubscription) {
        wsSubscription.unsubscribe();
      }
    }), pipe(map((array: T[]) => {
      for (const prefilter of prefilters) {
        array = prefilter.execute(array);
      }

      return array;
    })));
  }

  /**
   * Add a value to the collection
   * @param value The object to add to the collection
   */
  public add(value: T): Observable<CommandResult<T>> {
    const createCommand = new CreateCommand(this.collectionName, value);
    return this.websocket.sendCommand(createCommand).pipe(map((response: CreateResponse) => {
      return new CommandResult<T>(response.error, response.validationResults, response.newObject);
    }));
  }

  /**
   * Update a value of the collection
   * @param value The object to update in the collection
   */
  public update(value: T): Observable<CommandResult<T>> {
    const updateCommand = new UpdateCommand(this.collectionName, value);
    return this.websocket.sendCommand(updateCommand).pipe(map((response: UpdateResponse) => {
      return new CommandResult<T>(response.error, response.validationResults, response.updatedObject);
    }));
  }

  /**
   * Remove a value from the collection
   * @param value The object to remove from the collection
   */
  public remove(value: T): Observable<CommandResult<T>> {
    return this.collectionInformation.pipe(
      switchMap((info: InfoResponse) => {
        const primaryValues = {};
        info.primaryKeys.forEach(pk => {
          primaryValues[pk] = value[pk];
        });

        const deleteCommand = new DeleteCommand(this.collectionName, primaryValues);
        return this.websocket.sendCommand(deleteCommand).pipe(map((response: DeleteResponse) => {
          return new CommandResult<T>(response.error, response.validationResults);
        }));
    }));
  }

  /**
   * Check if the collection needs authentication
   */
  public onlyAuthenticated(): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        return info.onlyAuthorized;
      })
    );
  }

  /**
   * Check if any of the roles can read the collection
   * @param roles The roles to check
   */
  public canRead(roles: string[]): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        return info.rolesRead == null || info.rolesRead.Any(r => roles.Contains(r));
      })
    );
  }

  /**
   * Check if any of the roles can write in the collection
   * @param roles The roles to check
   */
  public canWrite(roles: string[]): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        return info.rolesWrite == null || info.rolesWrite.Any(r => roles.Contains(r));
      })
    );
  }

  /**
   * Check if any of the roles can delete in the collection
   * @param roles The roles to check
   */
  public canDelete(roles: string[]): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        return info.rolesDelete == null || info.rolesDelete.Any(r => roles.Contains(r));
      })
    );
  }
}
