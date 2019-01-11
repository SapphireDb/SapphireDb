import {IPrefilter} from './prefilter/iprefilter';
import {Observable} from 'rxjs';

export interface CollectionData<T> {
  prefilters: IPrefilter<T>[];
  data$: Observable<T[]>;
  referenceId?: string;
}
