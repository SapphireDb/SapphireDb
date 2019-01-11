import {IPrefilter} from './iprefilter';

export class SkipPrefilter<T> implements IPrefilter<T> {
  prefilterType = 'SkipPrefilter';
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
