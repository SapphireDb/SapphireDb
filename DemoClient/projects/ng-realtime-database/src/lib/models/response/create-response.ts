import {ValidatedResponseBase} from './validated-response-base';

export interface CreateResponse extends ValidatedResponseBase {
  newObject: any;
}
