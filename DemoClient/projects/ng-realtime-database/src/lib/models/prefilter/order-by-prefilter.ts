import {IPrefilter} from './iprefilter';
import {ArrayHelper} from '../../helper/array-helper';

export class OrderByPrefilter<T> implements IPrefilter<T> {
  prefilterType = 'OrderByPrefilter';
  selectFunction: (x: T) => any;
  selectFunctionString: string;
  descending: boolean;
  contextData: { [key: string]: string };

  constructor(selectFunction: (x: T) => any, descending: boolean = false, contextData?: { [key: string]: any }) {
    this.selectFunction = selectFunction;
    this.selectFunctionString = selectFunction.toString();
    this.descending = descending;

    if (contextData) {
      this.contextData = {};

      for (const key of Object.keys(contextData)) {
        this.contextData[key] = JSON.stringify(contextData[key]);
      }
    }
  }

  public execute(values: T[]) {
    return ArrayHelper.orderBy(values, this.selectFunction, this.descending);
  }

  public hash() {
    return `${this.prefilterType},${this.selectFunctionString},${this.descending},${JSON.stringify(this.contextData)}`;
  }
}
