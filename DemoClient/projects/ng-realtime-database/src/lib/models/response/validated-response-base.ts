import {ResponseBase} from './response-base';

export interface ValidatedResponseBase extends ResponseBase {
  validationResults: { [propertyName: string]: string[] };
}
