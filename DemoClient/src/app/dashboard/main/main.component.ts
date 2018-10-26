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

    combineLatest(
      collection.authInfo.queryAuth(),
      collection.authInfo.canQuery(roles),
      collection.authInfo.createAuth(),
      collection.authInfo.canCreate(roles),
      collection.authInfo.updateAuth(),
      collection.authInfo.canUpdate(roles),
      collection.authInfo.removeAuth(),
      collection.authInfo.canRemove(roles),
      collection.authInfo.queryPropertyAuth('firstName'),
      collection.authInfo.canQueryProperty('firstName', roles),
      collection.authInfo.updatePropertyAuth('firstName'),
      collection.authInfo.canUpdateProperty('firstName', roles)
    ).subscribe(([q, cq, c, cc, u, cu, r, cr, p, cp, up, ucp]:
                       [boolean, boolean, boolean, boolean, boolean, boolean, boolean, boolean, boolean, boolean, boolean, boolean]) => {
      console.warn(`Query: Authentication: ${q}, Can query: ${cq}`);
      console.warn(`Query Property FirstName: Authentication: ${p}, Can query: ${cp}`);
      console.warn(`Create: Authentication: ${c}, Can create: ${cc}`);
      console.warn(`Update: Authentication: ${u}, Can update: ${cu}`);
      console.warn(`Update Property FirstName: Authentication: ${up}, Can update: ${ucp}`);
      console.warn(`Remove: Authentication: ${r}, Can remove: ${cr}`);
    });

    // this.user$ = this.db.collection<User>('users').values(new OrderByPrefilter('username', false), new ThenOrderByPrefilter('id', true));

    this.user$ = this.offset$.pipe(switchMap((i: number) => {
      return this.db.collection<User>('users')
        .values(new SkipPrefilter(i), new TakePrefilter(10));
    }));

    this.db.collection<User>('users')
      .snapshot(new SkipPrefilter(5), new TakePrefilter(5)).subscribe(console.table);

    this.db.collection('tests').values();

    this.db.execute('example', 'GenerateRandomNumber').subscribe(v => console.log(v));
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
