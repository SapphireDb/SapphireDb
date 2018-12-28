import {BehaviorSubject, Subscription} from 'rxjs';
import {UserData} from './user-data';
import {RoleData} from './role-data';

export interface AuthSubscriptionReference {
  count: number;
  subject$: BehaviorSubject<(UserData|RoleData)[]>;
  subscription: Subscription;
  referenceId: string;
}
