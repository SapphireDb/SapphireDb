import {UserData} from './user-data';

export interface AuthData {
  userData: UserData;

  authToken: string;
  refreshToken: string;
  expiresAt: string;
  validFor: number;
}
