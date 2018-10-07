import {Injectable} from '@angular/core';
import {AuthData} from '../../model/auth-data';
import {HttpClient} from '@angular/common/http';
import * as moment from 'moment';
import {ActivatedRoute, Router} from '@angular/router';
import {AppUser} from '../../model/app-user';
import {RealtimeDatabase} from 'ng-realtime-database';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private authData: AuthData;
  private renewPending = false;

  constructor(private http: HttpClient, private route: ActivatedRoute, private router: Router, private db: RealtimeDatabase) {
    this.authData = JSON.parse(localStorage.getItem('auth_data'));
  }

  isValid(): boolean {
    if (this.authData && this.authData.refreshToken) {
      const expiresAt = moment(this.authData.expiresAt);
      const difference = moment.duration(expiresAt.diff(moment())).asSeconds();

      if (difference <= (this.authData.validFor / 2)) {
        if (!this.renewPending) {
          this.renewPending = true;
          const renewData = {userid: this.authData.userData.id, refreshtoken: this.authData.refreshToken};
          this.http.post('api/auth/renew', renewData).subscribe((result: any) => {
            this.renewPending = false;

            this.authData.authToken = result.auth_token;
            this.authData.refreshToken = result.refresh_token;
            this.authData.expiresAt = result.expires_at;
            this.authData.validFor = result.valid_for;
            this.authData.userData.roles = result.roles;

            this.db.setBearer(result.auth_token);

            localStorage.setItem('auth_data', JSON.stringify(this.authData));
          }, err => {
            this.authData = null;
            localStorage.removeItem('auth_data');
          });
        }
      }

      return true;
    }

    return false;
  }

  userData(): AppUser {
    return this.authData.userData;
  }

  loggedIn() {
    return this.isValid();
  }

  logout() {
    localStorage.removeItem('auth_data');
    this.authData = null;
  }

  login(username: string, password: string) {
    this.http.post('api/auth/login', { username: username, password: password })
      .subscribe((result: any) => {
        this.authData = new AuthData(result.auth_token, result.refresh_token, result.expires_at, result.valid_for);

        this.authData.userData.id = result.id;
        this.authData.userData.username = result.username;
        this.authData.userData.email = result.email;
        this.authData.userData.firstname = result.firstname;
        this.authData.userData.lastname = result.lastname;
        this.authData.userData.roles = result.roles;

        localStorage.setItem('auth_data', JSON.stringify(this.authData));

        this.db.setBearer(result.auth_token);

        this.route.queryParams.subscribe(params => {
          this.router.navigateByUrl(params['return'] || '');
        });
      });
  }
}
