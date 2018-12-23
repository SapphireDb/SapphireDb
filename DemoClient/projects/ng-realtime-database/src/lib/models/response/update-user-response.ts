import {ResponseBase} from './response-base';
import {UserData} from '../user-data';

export interface UpdateUserResponse extends ResponseBase {
  newUser: UserData;
  identityErrors: {
    code: string;
    description: string;
  };
}
