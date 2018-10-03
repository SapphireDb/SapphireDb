import {Subject} from 'rxjs';
import {ResponseBase} from './response/response-base';

export interface CommandReferences {
  [reference: string]: {
    subject$: Subject<ResponseBase>;
    keep?: boolean;
  };
}
