import {WebsocketService} from '../websocket.service';
import {AuthSubscriptionReference} from './auth-subscription-reference';
import {BehaviorSubject, Observable} from 'rxjs';
import {UserData} from './user-data';
import {SubscribeUsersCommand} from './command/subscribe-users-command';
import {SubscribeUsersResponse} from './response/subscribe-users-response';
import {finalize, map} from 'rxjs/operators';
import {CreateUserResponse} from './response/create-user-response';
import {CreateUserCommand} from './command/create-user-command';
import {UpdateUserResponse} from './response/update-user-response';
import {UpdateUserCommand} from './command/update-user-command';
import {DeleteUserResponse} from './response/delete-user-response';
import {DeleteUserCommand} from './command/delete-user-command';
import {RoleData} from './role-data';
import {SubscribeRolesCommand} from './command/subscribe-roles-command';
import {SubscribeRolesResponse} from './response/subscribe-roles-response';
import {UnsubscribeRolesCommand} from './command/unsubscribe-roles-command';
import {CreateRoleResponse} from './response/create-role-response';
import {CreateRoleCommand} from './command/create-role-command';
import {UpdateRoleResponse} from './response/update-role-response';
import {UpdateRoleCommand} from './command/update-role-command';
import {DeleteRoleResponse} from './response/delete-role-response';
import {DeleteRoleCommand} from './command/delete-role-command';
import {UnsubscribeUsersCommand} from './command/unsubscribe-users-command';

export class AuthInfo {
  private authSubscriptionReferences: { [key: string]: AuthSubscriptionReference } = {};

  constructor(private websocket: WebsocketService) {

  }

  /**
   * Get a list of all users
   */
  public getUsers(): Observable<UserData[]> {
    return <Observable<UserData[]>>this.getSubscription('users');
  }

  /**
   * Create a new user
   */
  public createUser(userName: string, email: string, password: string, roles: string[], addtionalData: { [key: string]: any })
    : Observable<CreateUserResponse> {
    return <Observable<CreateUserResponse>>this.websocket.sendCommand(
      new CreateUserCommand(userName, email, password, roles, addtionalData));
  }

  /**
   * Update an existing user
   */
  public updateUser(id: string, userName?: string, email?: string,
                    password?: string, roles?: string[], addtionalData?: { [key: string]: any })
    : Observable<UpdateUserResponse> {
    return <Observable<UpdateUserResponse>>this.websocket.sendCommand(
      new UpdateUserCommand(id, userName, email, password, roles, addtionalData));
  }

  /**
   * Delete user
   */
  public deleteUser(id: string)
    : Observable<DeleteUserResponse> {
    return <Observable<DeleteUserResponse>>this.websocket.sendCommand(
      new DeleteUserCommand(id));
  }

  /**
   * Get a list of all roles
   */
  public getRoles(): Observable<RoleData[]> {
    return <Observable<RoleData[]>>this.getSubscription('roles');
  }

  /**
   * Create a new role
   */
  public createRole(name: string)
    : Observable<CreateRoleResponse> {
    return <Observable<CreateRoleResponse>>this.websocket.sendCommand(
      new CreateRoleCommand(name));
  }

  /**
   * Update an existing role
   */
  public updateRole(id: string, name: string)
    : Observable<UpdateRoleResponse> {
    return <Observable<UpdateRoleResponse>>this.websocket.sendCommand(
      new UpdateRoleCommand(id, name));
  }

  /**
   * Delete role
   */
  public deleteRole(id: string)
    : Observable<DeleteRoleResponse> {
    return <Observable<DeleteRoleResponse>>this.websocket.sendCommand(
      new DeleteRoleCommand(id));
  }

  private getSubscription(type: 'users'|'roles'): Observable<(UserData|RoleData)[]> {
    let authSubscriptionReference: AuthSubscriptionReference = this.authSubscriptionReferences[type];

    if (!authSubscriptionReference) {
      const subscribeCommand = type === 'users' ? new SubscribeUsersCommand() : new SubscribeRolesCommand();

      authSubscriptionReference = {
        count: 0,
        subject$: new BehaviorSubject<RoleData[]>([]),
        subscription: null,
        referenceId: subscribeCommand.referenceId
      };

      authSubscriptionReference.subscription = this.websocketSubscribeData(subscribeCommand, authSubscriptionReference);
      authSubscriptionReference[type] = authSubscriptionReference;
    }

    authSubscriptionReference.count++;
    return this.createAuthSubscriptionObservable$(authSubscriptionReference, type);
  }

  private websocketSubscribeData(subscribeCommand: SubscribeUsersCommand|SubscribeRolesCommand,
                                 authSubscriptionReference: AuthSubscriptionReference) {
    return this.websocket.sendCommand(subscribeCommand, true)
      .pipe(map((response: (SubscribeRolesResponse|SubscribeUsersResponse)) => {
        if (response.responseType === 'SubscribeRolesResponse') {
          return (<SubscribeRolesResponse>response).roles;
        } else {
          return (<SubscribeUsersResponse>response).users;
        }
      }))
      .subscribe((data: (RoleData|UserData)[]) => {
        authSubscriptionReference.subject$.next(data);
      });
  }

  private createAuthSubscriptionObservable$(authSubscriptionReference: AuthSubscriptionReference, type: string): Observable<(UserData|RoleData)[]> {
    return authSubscriptionReference.subject$.asObservable().pipe(finalize(() => {
      authSubscriptionReference.count--;

      if (authSubscriptionReference.count === 0) {
        this.websocket.sendCommand((type === 'users' ? new UnsubscribeUsersCommand() : new UnsubscribeRolesCommand()), false, true);
        authSubscriptionReference.subject$.complete();
        authSubscriptionReference.subscription.unsubscribe();
        delete this.authSubscriptionReferences[type];
      }
    }));
  }
}
