import {ResponseBase} from './response-base';
import {RoleData} from '../role-data';

export interface QueryRolesResponse extends ResponseBase {
  roles: RoleData[];
}
