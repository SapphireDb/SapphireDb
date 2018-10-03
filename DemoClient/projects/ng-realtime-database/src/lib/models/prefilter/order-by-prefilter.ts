import {IPrefilter} from './iprefilter';

export class OrderByPrefilter implements IPrefilter {
  prefilterType = 'OrderByPrefilter';
  propertyName: string;
  descending: boolean;

  constructor(propertyName: string, descending: boolean = false) {
    this.propertyName = propertyName;
    this.descending = descending;
  }

  public execute(values: any[]) {
    if (this.descending) {
      return values.OrderByDescending(v => v[this.propertyName]);
    } else {
      return values.OrderBy(v => v[this.propertyName]);
    }
  }
}
