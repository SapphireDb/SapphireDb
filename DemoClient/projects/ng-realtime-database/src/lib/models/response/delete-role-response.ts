import {ResponseBase} from './response-base';

export interface DeleteRoleResponse extends ResponseBase {
  identityErrors: {
    code: string;
    description: string;
  };
}
