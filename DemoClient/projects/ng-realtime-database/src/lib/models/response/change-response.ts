import {ResponseBase} from './response-base';

export interface ChangeResponse extends ResponseBase {
  state: ChangeState;
  value: any;
}

export enum ChangeState {
  Added, Deleted, Modified
}
