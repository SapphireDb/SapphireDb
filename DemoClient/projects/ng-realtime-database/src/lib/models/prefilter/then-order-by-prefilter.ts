import {IPrefilter} from './iprefilter';
import {ArrayHelper} from '../../helper/array-helper';

export class ThenOrderByPrefilter implements IPrefilter {
  prefilterType = 'ThenOrderByPrefilter';
  propertyName: string;
  descending: boolean;

  constructor(propertyName: string, descending: boolean = false) {
    this.propertyName = propertyName;
    this.descending = descending;
  }

  public execute(values: any[]) {
    return ArrayHelper.thenOrderBy(values, v => v[this.propertyName], this.descending);
  }

  public hash() {
    return `${this.prefilterType},${this.propertyName},${this.descending}`;
  }
}
