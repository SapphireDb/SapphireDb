import {BehaviorSubject, Subscription} from 'rxjs';
import {IPrefilter} from './prefilter/iprefilter';

export class CollectionValue<T> {
  referenceId: string;
  prefilterHash: string;
  subject: BehaviorSubject<T[]>;
  socketSubscription: Subscription;
  subscriberCount: number;

  constructor(referenceId: string, prefilters: IPrefilter<T>[]) {
    this.referenceId = referenceId;
    this.prefilterHash = this.generatePrefilterHash(prefilters);
    this.subject = new BehaviorSubject<T[]>([]);
    this.subscriberCount = 1;
  }

  public setSubscription(socketSubscription: Subscription) {
    this.socketSubscription = socketSubscription;
  }

  public samePrefilters(prefilters: IPrefilter<any>[]): boolean {
    return this.generatePrefilterHash(prefilters) === this.prefilterHash;
  }


  private generatePrefilterHash(prefilters: IPrefilter<T>[]): string {
    let result = 'prefilters?';

    for (const prefilter of prefilters) {
      result += `${prefilter.hash()}&`;
    }

    return result;
  }
}
