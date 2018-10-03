import {IPrefilter} from './iprefilter';

export class SkipPrefilter implements IPrefilter {
  prefilterType = 'SkipPrefilter';
  number: number;

  constructor(number: number) {
    this.number = number;
  }

  execute(values: any[]) {
    return values;
  }
}
