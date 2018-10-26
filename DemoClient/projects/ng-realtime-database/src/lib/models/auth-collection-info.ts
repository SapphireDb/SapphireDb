import {Observable} from 'rxjs';
import {InfoResponse} from './response/info-response';
import {map} from 'rxjs/operators';
import {ArrayHelper} from '../helper/array-helper';

export class AuthCollectionInfo {
  constructor (private collectionInformation: Observable<InfoResponse>) {}

  /**
   * Check if the collection needs authentication for queries
   */
  public queryAuth(): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        return info.queryAuth.authentication;
      })
    );
  }

  /**
   * Check if the property needs authentication for queries
   */
  public queryPropertyAuth(propertyName: string): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        const propertyInfo = info.queryAuth.properties[propertyName];

        if (propertyInfo) {
          return propertyInfo.authentication;
        }

        return false;
      })
    );
  }

  /**
   * Check if the collection needs authentication for updates
   */
  public updateAuth(): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        return info.updateAuth.authentication;
      })
    );
  }

  /**
   * Check if the property needs authentication for updates
   */
  public updatePropertyAuth(propertyName: string): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        const propertyInfo = info.updateAuth.properties[propertyName];

        if (propertyInfo) {
          return propertyInfo.authentication;
        }

        return false;
      })
    );
  }

  /**
   * Check if the collection needs authentication to create data
   */
  public createAuth(): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        return info.createAuth.authentication;
      })
    );
  }

  /**
   * Check if the collection needs authentication to remove data
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
   * Check if the property is queriable with one of the given roles
   * @param propertyName The name of the property to check
   * @param roles The roles to check
   */
  public canQueryProperty(propertyName: string, roles: string[]): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        const propertyInfo = info.queryAuth.properties[propertyName];

        if (propertyInfo) {
          return propertyInfo.roles == null || ArrayHelper.isAnyRolePresent(propertyInfo.roles, roles);
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

  /**
   * Check if the property is updatable by one of the given roles
   * @param propertyName The name of the property to check
   * @param roles The roles to check
   */
  public canUpdateProperty(propertyName: string, roles: string[]): Observable<boolean> {
    return this.collectionInformation.pipe(
      map((info: InfoResponse) => {
        const propertyInfo = info.updateAuth.properties[propertyName];

        if (propertyInfo) {
          return propertyInfo.roles == null || ArrayHelper.isAnyRolePresent(propertyInfo.roles, roles);
        }

        return true;
      })
    );
  }
}
