import {WebsocketService} from '../websocket.service';
import {BehaviorSubject, Observable, of, Subject} from 'rxjs';
import {catchError, map, switchMap, take} from 'rxjs/operators';
import {LoginCommand} from './command/login-command';
import {UserData} from './user-data';
import {LoginResponse} from './response/login-response';
import {AuthData} from './auth-data';
import {RenewCommand} from './command/renew-command';
import {RenewResponse} from './response/renew-response';
import {LocalstoragePaths} from '../helper/localstorage-paths';
import {QueryUsersCommand} from './command/query-users-command';
import {QueryUsersResponse} from './response/query-users-response';
import {QueryRolesCommand} from './command/query-roles-command';
import {QueryRolesResponse} from './response/query-roles-response';
import {RoleData} from './role-data';
import {CreateUserResponse} from './response/create-user-response';
import {CreateUserCommand} from './command/create-user-command';
import {UpdateUserCommand} from './command/update-user-command';
import {UpdateUserResponse} from './response/update-user-response';
import {DeleteUserResponse} from './response/delete-user-response';
import {DeleteUserCommand} from './command/delete-user-command';
import {CreateRoleResponse} from './response/create-role-response';
import {CreateRoleCommand} from './command/create-role-command';
import {UpdateRoleResponse} from './response/update-role-response';
import {UpdateRoleCommand} from './command/update-role-command';
import {DeleteRoleResponse} from './response/delete-role-response';
import {DeleteRoleCommand} from './command/delete-role-command';

export class Auth {
  private authData$: BehaviorSubject<AuthData> = new BehaviorSubject(null);

  private renewPending = false;
  private renewSubject$ = new Subject<RenewResponse>();

  constructor(private websocket: WebsocketService) {
    const authDataString = localStorage.getItem(LocalstoragePaths.authPath);
    if (authDataString) {
      this.authData$.next(JSON.parse(authDataString));
      this.websocket.setBearer(this.authData$.value.authToken);
    }
  }

  /**
   * Get the current user data
   */
  public getUserData(): Observable<UserData> {
    return this.isValid().pipe(switchMap((valid: boolean) => {
      if (valid) {
        return this.authData$.pipe(map((authData: AuthData) => {
          if (authData) {
            return authData.userData;
          }

          return null;
        }));
      }

      return of(null);
    }));
  }

  /**
   * Check if the user is logged in
   */
  public isLoggedIn(): Observable<boolean> {
    return this.isValid();
  }

  /**
   * Log the client in
   */
  public login(username: string, password: string): Observable<UserData> {
    return this.websocket.sendCommand(new LoginCommand(username, password))
      .pipe(switchMap((response: LoginResponse) => {
        this.authData$.next({
          authToken: response.authToken,
          expiresAt: response.expiresAt,
          validFor: response.validFor,
          refreshToken: response.refreshToken,
          userData: response.userData
        });

        localStorage.setItem(LocalstoragePaths.authPath, JSON.stringify(this.authData$.value));
        this.websocket.setBearer(this.authData$.value.authToken);

        return this.getUserData();
      }));
  }

  /**
   * Logout the client
   */
  public logout() {
    localStorage.removeItem(LocalstoragePaths.authPath);
    this.authData$.next(null);
    this.websocket.setBearer(null);
  }

  private isValid(): Observable<boolean> {
    return this.authData$.pipe(switchMap((authData: AuthData) => {
      if (authData) {
        const expiresAt = new Date(authData.expiresAt);
        const difference = (expiresAt.getTime() - new Date().getTime()) / 1000;

        if (difference <= (authData.validFor / 2)) {
          if (!this.renewPending) {
            this.renewPending = true;

            this.websocket.sendCommand(new RenewCommand(authData.userData.id, authData.refreshToken))
              .pipe(catchError((err: any) => {
                return of(null);
              }))
              .subscribe((response: RenewResponse) => {
                this.renewPending = false;
                this.renewSubject$.next(response);

                if (response) {
                  this.authData$.next({
                    userData: response.userData,
                    refreshToken: response.refreshToken,
                    validFor: response.validFor,
                    expiresAt: response.expiresAt,
                    authToken: response.authToken
                  });

                  localStorage.setItem(LocalstoragePaths.authPath, JSON.stringify(this.authData$.value));
                  this.websocket.setBearer(response.authToken);
                } else {
                  this.authData$.next(null);
                  localStorage.removeItem(LocalstoragePaths.authPath);
                  this.websocket.setBearer();
                }
              }, (err: any) => {

              });
          }

          return this.renewSubject$.pipe(
            take(1),
            map((response: RenewResponse) => {
              return !!response && response.error === null;
            })
          );
        } else {
          return of(true);
        }
      }

      return of(false);
    }));
  }

  /**
   * Get a list of all users
   */
  public getUsers(): Observable<UserData[]> {
    return this.websocket.sendCommand(new QueryUsersCommand()).pipe(map((response: QueryUsersResponse) => {
      return response.users;
    }));
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
    return this.websocket.sendCommand(new QueryRolesCommand()).pipe(map((response: QueryRolesResponse) => {
      return response.roles;
    }));
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
}
