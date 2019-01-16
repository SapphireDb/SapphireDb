import {ResponseBase} from './response-base';

export interface InfoResponse extends ResponseBase {
  primaryKeys: string[];

  queryAuth: PropertyAuthInfo;
  createAuth: AuthInfo;
  removeAuth: AuthInfo;
  updateAuth: PropertyAuthInfo;
}

export interface AuthInfo {
  authentication: boolean;
  authorization: boolean;
  roles?: string[];
  functionName?: string;
}

export interface PropertyAuthInfo extends AuthInfo {
  properties: { [key: string]: AuthInfo };
}
