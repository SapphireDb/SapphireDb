import {Inject, Injectable, Type} from '@angular/core';
import {CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router} from '@angular/router';
import {Observable, of} from 'rxjs';
import {map, switchMap} from 'rxjs/operators';
import {UserData} from './models/user-data';
import {RealtimeDatabase} from './realtime-database.service';
import {RealtimeDatabaseOptions} from './models/realtime-database-options';

@Injectable()
export class RealtimeAuthGuard implements CanActivate {
  constructor(private db: RealtimeDatabase, private router: Router,
              @Inject('realtimedatabase.options') private options: RealtimeDatabaseOptions) {}

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    const roles: string[] = next.data['roles'];

    return this.db.auth.isLoggedIn().pipe(switchMap((loggedIn: boolean) => {
      if (!loggedIn) {
        if (this.options.loginRedirect) {
          this.router.navigate([this.options.loginRedirect], {
            queryParams: {
              return: state.url
            }
          });
        }

        return of(false);
      }

      return this.db.auth.getUserData().pipe(map((userData: UserData) => {
        if (roles && roles.length > 0) {
          let hasRole = false;

          for (const role of roles) {
            if (userData.roles.indexOf(role) !== -1) {
              hasRole = true;
              break;
            }
          }

          if (!hasRole) {
            if (this.options.unauthorizedRedirect) {
              this.router.navigate([this.options.unauthorizedRedirect], {
                queryParams: {
                  return: state.url
                }
              });
            }

            return false;
          }
        }

        return true;
      }));
    }));
  }
}
