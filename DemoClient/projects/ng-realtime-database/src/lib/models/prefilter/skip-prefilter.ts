import {TakePrefilter} from './take-prefilter';

export class SkipPrefilter<T> extends TakePrefilter<T> {
  prefilterType = 'SkipPrefilter';

  constructor(number: number) {
    super(number);
  }
}
