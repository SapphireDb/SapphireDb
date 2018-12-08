import {ResponseBase} from './response-base';
import {UserData} from '../user-data';

export interface LoginResponse extends ResponseBase {
  authToken: string;
  refreshToken: string;
  validFor: number;
  expiresAt: string;

  userData: UserData;
}
