import {ResponseBase} from './response-base';
import {RoleData} from '../role-data';

export interface CreateRoleResponse extends ResponseBase {
  newRole: RoleData;
  identityErrors: {
    code: string;
    description: string;
  };
}
