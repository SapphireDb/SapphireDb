import {IPrefilter} from './iprefilter';

export type comparisonType = '==' | '!=' | '<' | '<='| '>' | '>=';

export class WherePrefilter implements IPrefilter {
  prefilterType = 'WherePrefilter';
  propertyName: string;
  comparision: comparisonType;
  compareValue: string;

  constructor(propertyName: string, comparision: comparisonType, compareValue: string) {
    this.propertyName = propertyName;
    this.comparision = comparision;
    this.compareValue = compareValue;
  }

  public execute(values: any[]) {
    return values;
  }

  public hash() {
    return `${this.prefilterType},${this.propertyName},${this.comparision},${this.compareValue}`;
  }
}
