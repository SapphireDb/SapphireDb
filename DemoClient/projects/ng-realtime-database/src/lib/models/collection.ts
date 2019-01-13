import {Observable, pipe} from 'rxjs';
import {WebsocketService} from '../websocket.service';
import {CreateCommand} from './command/create-command';
import {CreateResponse} from './response/create-response';
import {CommandResult} from './command-result';
import {DeleteResponse} from './response/delete-response';
import {UpdateCommand} from './command/update-command';
import {UpdateResponse} from './response/update-response';
import {DeleteCommand} from './command/delete-command';
import {SubscribeCommand} from './command/subscribe-command';
import {finalize, map, switchMap} from 'rxjs/operators';
import {QueryResponse} from './response/query-response';
import {InfoResponse} from './response/info-response';
import {ChangeResponse} from './response/change-response';
import {UnsubscribeCommand} from './command/unsubscribe-command';
import {IPrefilter} from './prefilter/iprefilter';
import {UnloadResponse} from './response/unload-response';
import {LoadResponse} from './response/load-response';
import {CollectionHelper} from '../helper/collection-helper';
import {QueryCommand} from './command/query-command';
import {AuthCollectionInfo} from './auth-collection-info';
import {CollectionValue} from './collection-value';

export class Collection<T> {
  /**
   * Information about Authentication and Authorization of the collection
   */
  public authInfo: AuthCollectionInfo;
  private collectionValues: CollectionValue<T>[] = [];

  constructor(public collectionName: string,
              private websocket: WebsocketService,
              private collectionInformation: Observable<InfoResponse>) {
    this.authInfo = new AuthCollectionInfo(this.collectionInformation);
  }

  /**
   * Get a snapshot of the values of the collection
   * @param prefilters Additional prefilters to query only specific data
   */
  public snapshot(...prefilters: IPrefilter<T>[]): Observable<T[]> {
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
  public values(...prefilters: IPrefilter<T>[]): Observable<T[]> {
    const index = this.collectionValues.findIndex(c => c.samePrefilters(prefilters));

    let collectionValue: CollectionValue<T>;
    if (index !== -1) {
       collectionValue = this.collectionValues[index];
       collectionValue.subscriberCount++;
    } else {
      collectionValue = this.createWebsocketValuesSubscription(prefilters);
    }

    return this.createCollectionObservable$(collectionValue, prefilters);
  }

  /**
   * Add a value to the collection
   * @param value The object to add to the collection
   */
  public add(value: T): Observable<CommandResult<T>> {
    return this.createCommandResult$(<any>this.websocket.sendCommand(new CreateCommand(this.collectionName, value)));
  }

  /**
   * Update a value of the collection
   * @param value The object to update in the collection
   */
  public update(value: T): Observable<CommandResult<T>> {
    return this.createCommandResult$(<any>this.websocket.sendCommand(new UpdateCommand(this.collectionName, value)));
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

  private createWebsocketValuesSubscription(prefilters: IPrefilter<T>[]): CollectionValue<T> {
    const subscribeCommand = new SubscribeCommand(this.collectionName, prefilters);
    const collectionValue = new CollectionValue<T>(subscribeCommand.referenceId, prefilters);

    const wsSubscription = this.websocket.sendCommand(subscribeCommand, true)
      .subscribe((response: (QueryResponse | ChangeResponse | UnloadResponse | LoadResponse)) => {
        if (response.responseType === 'QueryResponse') {
          collectionValue.subject.next((<QueryResponse>response).collection);
        } else if (response.responseType === 'ChangeResponse') {
          CollectionHelper.updateCollection<T>(collectionValue.subject, this.collectionInformation, <ChangeResponse>response);
        } else if (response.responseType === 'UnloadResponse') {
          CollectionHelper.unloadItem<T>(collectionValue.subject, this.collectionInformation, <UnloadResponse>response);
        } else if (response.responseType === 'LoadResponse') {
          CollectionHelper.loadItem<T>(collectionValue.subject, <LoadResponse>response);
        }
      });

    collectionValue.setSubscription(wsSubscription);
    this.collectionValues.push(collectionValue);

    return collectionValue;
  }

  private createCollectionObservable$(collectionValue: CollectionValue<T>, prefilters: IPrefilter<T>[]): Observable<T[]> {
    return collectionValue.subject.pipe(finalize(() => {
      collectionValue.subscriberCount--;

      if (collectionValue.subscriberCount === 0) {
        this.websocket.sendCommand(new UnsubscribeCommand(this.collectionName, collectionValue.referenceId), false, true);
        collectionValue.socketSubscription.unsubscribe();
        const indexToRemove = this.collectionValues.findIndex(c => c.referenceId === collectionValue.referenceId);
        this.collectionValues.splice(indexToRemove, 1);
      }
    }), pipe(map((array: T[]) => {
      for (const prefilter of prefilters) {
        array = prefilter.execute(array);
      }

      return array;
    })));
  }

  private createCommandResult$(observable$: Observable<CreateResponse|UpdateResponse>): Observable<CommandResult<T>> {
    return observable$.pipe(map((response: CreateResponse|UpdateResponse) => {
      return new CommandResult<T>(response.error, response.validationResults,
        response.responseType === 'CreateResponse' ?
          (<CreateResponse>response).newObject : (<UpdateResponse>response).updatedObject);
    }));
  }
}
