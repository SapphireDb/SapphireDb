import {ValidatedResponseBase} from './validated-response-base';

export interface UpdateResponse extends ValidatedResponseBase {
  updatedObject: any;
}
