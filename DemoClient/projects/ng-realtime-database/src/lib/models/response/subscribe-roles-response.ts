import {ResponseBase} from './response-base';
import {RoleData} from '../role-data';

export interface SubscribeRolesResponse extends ResponseBase {
  roles: RoleData[];
}
