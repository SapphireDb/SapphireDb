import {ResponseBase} from './response-base';
import {UserData} from '../user-data';

export interface CreateUserResponse extends ResponseBase {
  newUser: UserData;
  identityErrors: {
    code: string;
    description: string;
  };
}
