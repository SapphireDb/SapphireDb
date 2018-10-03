import {Component, OnInit} from '@angular/core';
import {RealtimeDatabase, SkipPrefilter, TakePrefilter} from 'ng-realtime-database';
import {BehaviorSubject, combineLatest, Observable} from 'rxjs';
import {User} from '../../model/user';
import {switchMap} from 'rxjs/operators';
import {AccountService} from '../../shared/services/account.service';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.less']
})
export class MainComponent implements OnInit {

  offset$: BehaviorSubject<number> = new BehaviorSubject<number>(0);
  user$: Observable<any[]>;

  username: string;

  constructor(private db: RealtimeDatabase, private account: AccountService) { }

  ngOnInit() {
    const roles = this.account.userData().roles;
    const collection = this.db.collection('users');

    combineLatest(collection.onlyAuthenticated(), collection.canRead(roles), collection.canWrite(roles), collection.canDelete(roles))
      .subscribe(([a, r, w, d]: [boolean, boolean, boolean, boolean]) => {
        console.warn(`Needs authentication: ${a}`, `Can read: ${r}`, `Can write: ${w}`, `Can delete: ${d}`);
      }
    );

    this.user$ = this.offset$.pipe(switchMap((i: number) => {
      return this.db.collection<User>('users')
        .values(new SkipPrefilter(i), new TakePrefilter(10));
    }));

    this.db.collection<User>('users')
      .snapshot(new SkipPrefilter(5), new TakePrefilter(5)).subscribe(console.table);

    this.db.collection('tests').values();
  }

  createUser() {
    this.db.collection<User>('users').add({
      firstName: 'Test',
      lastName: 'User',
      username: this.username
    }).subscribe(console.table);
    this.username = '';
  }

  remove(u: User) {
    const result = prompt('Enter new user');

    if (!result) {
      this.db.collection<User>('users').remove(u).subscribe(console.table);
    } else {
      const userClone = Object.assign({}, u);

      userClone.lastName = result;
      this.db.collection<User>('users').update(userClone).subscribe(console.table);
    }
  }

  addOffset(number: number) {
    this.offset$.next(this.offset$.value + number);
  }
}
