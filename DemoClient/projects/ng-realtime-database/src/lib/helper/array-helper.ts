Object.defineProperty(Array.prototype, '_realtime_', {
  value: { sorting: [] },
  enumerable: false
});

export class ArrayHelper {
  static isAnyRolePresent(neededRoles: string[], presentRoles: string[]): boolean {
    for (const neededRole of neededRoles) {
      if (presentRoles.indexOf(neededRole) !== -1) {
        return true;
      }
    }

    return false;
  }

  static orderBy<T>(array: T[], valueSelector: (item: T) => any, descending: boolean): T[] {
    (<any>array)._realtime_.sorting = [{descending: descending, valueSelector: valueSelector}];

    return array.sort((a, b) => {
      return ArrayHelper.orderCompareFunction(valueSelector, a, b, descending);
    });
  }

  static thenOrderBy<T>(array: T[], valueSelector: (item: T) => any, descending: boolean): T[] {
    if ((<any>array)._realtime_.sorting == null || (<any>array)._realtime_.sorting .length === 0) {
      return array;
    }

    (<any>array)._realtime_.sorting.push({descending: descending, valueSelector: valueSelector});

    return array.sort((a, b) => {
      for (const entry of (<any>array)._realtime_.sorting) {
        const result = ArrayHelper.orderCompareFunction(entry.valueSelector, a, b, entry.descending);

        if (result !== 0) {
          return result;
        }
      }

      return 0;
    });
  }

  private static stringCompare(value_a: string, value_b: string, invert: boolean): number {
    value_a = value_a.toLowerCase();
    value_b = value_b.toLowerCase();

    if (value_a > value_b) {
      return invert === true ? -1 : 1;
    } else if (value_a < value_b) {
      return invert === true ? 1 : -1;
    } else {
      return 0;
    }
  }
  
  private static booleanCompare(value_a: boolean, value_b: boolean, invert: boolean): number {
    if (value_a === value_b) {
      return 0;
    } else {
      if (invert === true) {
        return value_a ? 1 : -1;
      } else {
        return value_a ? -1 : 1;
      }
    }
  }
  
  private static defaultCompare(type_a: string, type_b: string, invert: boolean): number {
    if (type_a === 'undefined' && type_a === type_b) {
      return 0;
    } else if (type_a === 'undefined') {
      return invert ? 1 : -1;
    } else if (type_b === 'undefined') {
      return invert ? -1 : 1;
    }

    return 0;
  }
  
  static orderCompareFunction<T>(valueSelector: (item: T) => any, a: T, b: T, invert: boolean): number {
    const value_a: any = valueSelector(a);
    const value_b: any = valueSelector(b);

    const type_a: string = typeof value_a;
    const type_b: string = typeof value_b;

    if (type_a === 'string' && type_a === type_b) {
      return this.stringCompare(value_a, value_b, invert);
    } else if (type_a === 'number' && type_a === type_b) {
      return invert === true ? value_b - value_a : value_a - value_b;
    } else if (type_a === 'boolean' && type_a === type_b) {
      return this.booleanCompare(value_a, value_b, invert);
    } else {
      return this.defaultCompare(type_a, type_b, invert);
    }
  }
}
