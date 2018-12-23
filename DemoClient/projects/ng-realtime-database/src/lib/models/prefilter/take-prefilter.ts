import {IPrefilter} from './iprefilter';

export class TakePrefilter implements IPrefilter {
  prefilterType = 'TakePrefilter';
  number: number;

  constructor(number: number) {
    this.number = number;
  }

  execute(values: any[]) {
    return values;
  }

  public hash() {
    return `${this.prefilterType},${this.number}`;
  }
}
