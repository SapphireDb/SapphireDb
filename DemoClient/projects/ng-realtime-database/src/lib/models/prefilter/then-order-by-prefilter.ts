import {IPrefilter} from './iprefilter';

export class ThenOrderByPrefilter implements IPrefilter {
  prefilterType = 'ThenOrderByPrefilter';
  propertyName: string;
  descending: boolean;

  constructor(propertyName: string, descending: boolean = false) {
    this.propertyName = propertyName;
    this.descending = descending;
  }

  public execute(values: any[]) {
    if (this.descending) {
      return values.ThenByDescending(v => v[this.propertyName]);
    } else {
      return values.ThenBy(v => v[this.propertyName]);
    }
  }
}
