import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot} from '@angular/router';
import {AccountService} from './services/account.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private account: AccountService, private router: Router) {

  }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
      if (this.account.loggedIn()) {
        return true;
      } else {
        this.router.navigate(['/account/login'], {
          queryParams: {
            return: state.url
          }
        });
        return false;
      }
  }
}
