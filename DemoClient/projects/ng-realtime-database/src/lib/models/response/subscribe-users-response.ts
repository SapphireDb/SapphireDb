import {ResponseBase} from './response-base';
import {UserData} from '../user-data';

export interface SubscribeUsersResponse extends ResponseBase {
  users: UserData[];
}
