import {ResponseBase} from './response-base';
import {UserData} from '../user-data';

export interface QueryUsersResponse extends ResponseBase {
  users: UserData[];
}
