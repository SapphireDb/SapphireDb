import {IPrefilter} from './iprefilter';

export class TakePrefilter<T> implements IPrefilter<T> {
  prefilterType = 'TakePrefilter';
  number: number;

  constructor(number: number) {
    this.number = number;
  }

  execute(values: T[]) {
    return values;
  }

  public hash() {
    return `${this.prefilterType},${this.number}`;
  }
}
