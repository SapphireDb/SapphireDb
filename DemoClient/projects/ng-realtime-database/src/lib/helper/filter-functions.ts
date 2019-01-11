import {IPrefilter} from '../models/prefilter/iprefilter';
import {CollectionData} from '../models/collection-data';

// @dynamic
export class FilterFunctions {
  static comparePrimaryKeysFunction(primaryKeys: string[], newValue: any) {
    return (currentValue) => {
      let result = 0;

      primaryKeys.forEach(key => {
        if (currentValue[key] === newValue[key]) {
          result++;
        }
      });

      return result === primaryKeys.length;
    };
  }

  static comparePrefilterFunction<T>(prefilters: IPrefilter<T>[]) {
    return (data: CollectionData<T>) => {
      if (data.prefilters.length !== prefilters.length) {
        return false;
      }

      for (let i = 0; i < prefilters.length; i++) {
        const prefilterArg = prefilters[0];
        const prefilter = data.prefilters[0];

        const keys = Object.keys(prefilters[0]);

        for (const key of keys) {
          if (prefilter[key] !== prefilterArg[key]) {
            return false;
          }
        }
      }
    };
  }
}
