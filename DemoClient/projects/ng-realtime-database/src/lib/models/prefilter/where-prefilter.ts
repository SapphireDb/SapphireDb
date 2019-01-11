import {IPrefilter} from './iprefilter';

export class WherePrefilter<T> implements IPrefilter<T> {
  prefilterType = 'WherePrefilter';
  compareFunction: (x: T) => boolean;
  compareFunctionString: string;
  contextData: { [key: string]: string };

  constructor(compareFunction: (x: T) => boolean, contextData?: { [key: string]: any }) {
    this.compareFunction = compareFunction;
    this.compareFunctionString = compareFunction.toString();

    if (contextData) {
      this.contextData = {};

      for (const key of Object.keys(contextData)) {
        this.contextData[key] = JSON.stringify(contextData[key]);
      }
    }
  }

  public execute(values: T[]) {
    return values;
  }

  public hash() {
    return `${this.prefilterType},${this.compareFunctionString},${JSON.stringify(this.contextData)}`;
  }
}
