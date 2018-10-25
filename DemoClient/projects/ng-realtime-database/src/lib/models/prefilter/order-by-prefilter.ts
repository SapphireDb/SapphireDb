import {IPrefilter} from './iprefilter';
import {ArrayHelper} from '../../helper/array-helper';

export class OrderByPrefilter implements IPrefilter {
  prefilterType = 'OrderByPrefilter';
  propertyName: string;
  descending: boolean;

  constructor(propertyName: string, descending: boolean = false) {
    this.propertyName = propertyName;
    this.descending = descending;
  }

  public execute(values: any[]) {
    return ArrayHelper.orderBy(values, v => v[this.propertyName], this.descending);
  }
}
