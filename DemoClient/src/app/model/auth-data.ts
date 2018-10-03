import {AppUser} from './app-user';

export class AuthData {
  userData: AppUser;

  authToken: string;
  refreshToken: string;
  expiresAt: string;
  validFor: number;

  constructor(_authToken: string, _refreshToken: string, _expiresAt: string, _validFor: number) {
    this.authToken = _authToken;
    this.refreshToken = _refreshToken;
    this.expiresAt = _expiresAt;
    this.validFor = _validFor;

    this.userData = new AppUser();
  }
}
