import {ResponseBase} from './response-base';

export interface QueryResponse extends ResponseBase {
  collection: any[];
}
