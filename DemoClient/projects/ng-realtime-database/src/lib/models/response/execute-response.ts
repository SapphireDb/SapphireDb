import {ResponseBase} from './response-base';

export interface ExecuteResponse extends ResponseBase {
  result: any;
  type: ExecuteResponseType;
}

export enum ExecuteResponseType {
  End, Notify
}
