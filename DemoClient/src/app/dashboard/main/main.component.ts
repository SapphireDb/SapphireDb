import {Component, OnInit} from '@angular/core';
import {Collection, RealtimeDatabase, SkipPrefilter, TakePrefilter,
  ActionResult, ActionHelper, ExecuteResponseType, UserData} from 'ng-realtime-database';
import {BehaviorSubject, combineLatest, Observable, of, pipe, Subscription} from 'rxjs';
import {User} from '../../model/user';
import {concatMap, filter, map, switchMap, take, takeUntil, takeWhile} from 'rxjs/operators';
import {AccountService} from '../../shared/services/account.service';
import {Log} from '../../model/log';
import {OrderByPrefilter} from '../../../../projects/ng-realtime-database/src/lib/models/prefilter/order-by-prefilter';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.less']
})
export class MainComponent implements OnInit {

  offset$: BehaviorSubject<number> = new BehaviorSubject<number>(0);
  user$: Observable<any[]>;

  username: string;
  message: string;

  rangeValue: Observable<number>;

  testSub: Subscription;

  constructor(private db: RealtimeDatabase, private account: AccountService) { }

  ngOnInit() {
    this.account.userData().subscribe((userData: UserData) => {
      const roles = userData.roles;
      const collection: Collection<any> = this.db.collection('users');

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
    });

    this.db.execute('example', 'GenerateRandomNumber')
    // .subscribe((v: ActionResult<number, string>) => console.log(v));
      .subscribe(ActionHelper.result<number, string>(
        v => console.log('Result: ' + v),
        v => console.log('Notification: ' + v)));

    // this.user$ = this.db.collection<User>('users').values(new OrderByPrefilter('username', false), new ThenOrderByPrefilter('id', true));

    this.user$ = this.offset$.pipe(switchMap((i: number) => {
      return this.db.collection<User>('users')
        .values(new SkipPrefilter(i), new TakePrefilter(10));
    }));

    this.db.collection<Log>('logs')
      .values(new OrderByPrefilter(x => x.id, true), new TakePrefilter(1))
      .pipe(map(v => v[0]))
      .subscribe(console.warn);

    this.db.collection<User>('users')
      .snapshot(new SkipPrefilter(3), new TakePrefilter(5)).subscribe(console.table);

    // this.db.collection('tests').values();

    this.db.execute('example', 'TestWithParams', 'test1234', 'test2345')
      .subscribe(console.log);

    this.db.execute('example', 'NoReturn').subscribe(console.log);

    this.db.messaging.topic('test').pipe(take(3)).subscribe(alert);

    this.db.messaging.messages().subscribe(console.warn);
  }

  createUser() {
    this.db.collection<User>('users').add({
      firstName: 'Test',
      lastName: 'User',
      username: this.username
    }).subscribe(v => {
      console.table(v);
    });
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

  execute() {
    // this.db.execute('example', 'GenerateRandomNumber')
    // // .subscribe((v: ActionResult<number, string>) => console.log(v));
    //   .subscribe(ActionHelper.result<number, string>(
    //     v => console.log('Result: ' + v),
    //     (v) => {
    //       this.rangeValue = <any>v;
    //     }));

    this.rangeValue = this.db.execute('example', 'GenerateRandomNumber').pipe(
      filter((r: ActionResult<number, number>) => r.type === ExecuteResponseType.Notify),
      map((r: ActionResult<number, number>) => r.notification),
      concatMap(v => {
        if (v === 100) {
          return of(v, null);
        }

        return of(v);
      }),
      takeWhile(v => v !== null)
    );

    this.testSub = this.rangeValue.subscribe(console.log);
  }

  send() {
    this.db.execute('message', 'SendToAdmin', 'Das ist ei ntest').subscribe(console.warn);
    this.db.messaging.send({data: this.message});
    this.db.messaging.publish('test', this.message);
  }

  executeTest() {
    this.db.execute('user', 'Test');
  }
}
