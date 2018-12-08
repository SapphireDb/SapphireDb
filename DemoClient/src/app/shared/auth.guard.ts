import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot} from '@angular/router';
import {AccountService} from './services/account.service';
import {map} from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private account: AccountService, private router: Router) {

  }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    return this.account.loggedIn().pipe(map((loggedIn: boolean) => {
      if (!loggedIn) {
        this.router.navigate(['/account/login'], {
          queryParams: {
            return: state.url
          }
        });
      }

      return loggedIn;
    }));

  }
}
