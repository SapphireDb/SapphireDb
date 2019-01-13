import {IPrefilter} from './iprefilter';
import {ArrayHelper} from '../../helper/array-helper';
import {OrderByPrefilter} from './order-by-prefilter';

export class ThenOrderByPrefilter<T> extends OrderByPrefilter<T> {
  prefilterType = 'ThenOrderByPrefilter';

  constructor(selectFunction: (x: T) => any, descending: boolean = false, contextData?: { [key: string]: any }) {
    super(selectFunction, descending, contextData);
  }

  public execute(values: T[]) {
    return ArrayHelper.thenOrderBy(values, this.selectFunction, this.descending);
  }
}
