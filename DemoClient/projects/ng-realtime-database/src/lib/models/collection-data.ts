import {IPrefilter} from './prefilter/iprefilter';
import {Observable} from 'rxjs';

export interface CollectionData<T> {
  prefilters: IPrefilter[];
  data$: Observable<T[]>;
  referenceId?: string;
}
