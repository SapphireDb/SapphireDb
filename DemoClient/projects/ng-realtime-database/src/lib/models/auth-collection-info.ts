import {Observable} from 'rxjs';
import {InfoResponse} from './response/info-response';
import {map} from 'rxjs/operators';
import {ArrayHelper} from '../helper/array-helper';

export class AuthCollectionInfo {
  constructor (private collectionInformation: Observable<InfoResponse>) {}

  /**
   * Check if the collection needs authentication
   */
  public queryAuth(): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        return info.queryAuth.authentication;
      })
    );
  }

  /**
   * Check if the collection needs authentication
   */
  public updateAuth(): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        return info.updateAuth.authentication;
      })
    );
  }

  /**
   * Check if the collection needs authentication
   */
  public createAuth(): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        return info.createAuth.authentication;
      })
    );
  }

  /**
   * Check if the collection needs authentication
   */
  public removeAuth(): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        return info.removeAuth.authentication;
      })
    );
  }

  /**
   * Check if any of the roles can read the collection
   * @param roles The roles to check
   */
  public canQuery(roles: string[]): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        if (info.queryAuth.authorization) {
          return info.queryAuth.roles == null || ArrayHelper.isAnyRolePresent(info.queryAuth.roles, roles);
        }

        return true;
      })
    );
  }

  /**
   * Check if any of the roles can write in the collection
   * @param roles The roles to check
   */
  public canCreate(roles: string[]): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        return info.createAuth.roles == null || ArrayHelper.isAnyRolePresent(info.createAuth.roles, roles);
      })
    );
  }

  /**
   * Check if any of the roles can delete in the collection
   * @param roles The roles to check
   */
  public canRemove(roles: string[]): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        return info.removeAuth.roles == null || ArrayHelper.isAnyRolePresent(info.removeAuth.roles, roles);
      })
    );
  }

  /**
   * Check if any of the roles can delete in the collection
   * @param roles The roles to check
   */
  public canUpdate(roles: string[]): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        return info.updateAuth.roles == null || ArrayHelper.isAnyRolePresent(info.updateAuth.roles, roles);
      })
    );
  }
}
