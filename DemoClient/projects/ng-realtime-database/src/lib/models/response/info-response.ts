import {ResponseBase} from './response-base';

export interface InfoResponse extends ResponseBase {
  primaryKeys: string[];
  onlyAuthorized: boolean;
  rolesRead?: string[];
  rolesWrite?: string[];
  rolesDelete?: string[];
}
