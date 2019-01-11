import {Component, OnInit} from '@angular/core';
import {Collection, RealtimeDatabase, UserData, RoleData} from 'ng-realtime-database';
import {Message} from '../../model/message';
import {BehaviorSubject, combineLatest, Observable, of} from 'rxjs';
import {map, publish, share, shareReplay, switchMap} from 'rxjs/operators';
import {WherePrefilter} from '../../../../projects/ng-realtime-database/src/lib/models/prefilter/where-prefilter';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.less']
})
export class MainComponent implements OnInit {

  messageCollection: Collection<Message>;
  messages$: Observable<Message[]>;
  users$: Observable<UserData[]>;
  // roles$: Observable<RoleData[]>;

  currentUser$: BehaviorSubject<UserData> = new BehaviorSubject<UserData>(null);
  userMessages$: Observable<Message[]>;
  message: string;

  constructor(private db: RealtimeDatabase) {
    this.messageCollection = this.db.collection<Message>('messages');
    this.messages$ = this.db.auth.getUserData().pipe(switchMap(u => {
      return this.messageCollection.values(new WherePrefilter(x => x.userId === u.id || x.toId === u.id, {u: u}));
    }));
    // this.messages$ = this.messageCollection.values(new WherePrefilter('', '<', '', (x) => x.userId == ));

    this.users$ = combineLatest(this.db.auth.info.getUsers(), this.db.auth.getUserData()).pipe(map(([users, user]: [UserData[], UserData]) => {
      return users.filter(u => u.id !== user.id);
    }));

    this.userMessages$ = combineLatest(this.currentUser$, this.messages$).pipe(map(([user, messages]: [UserData, Message[]]) => {
      if (user != null) {
        return messages.filter(msg => msg.toId === user.id || msg.userId === user.id).sort((a, b) =>
          a.createdOn > b.createdOn ? 1 : a.createdOn === b.createdOn ? 0 : -1);
      }
    }));

    // this.roles$ = this.db.auth.getRoles();
  }

  ngOnInit() {
  }

  selectUser(user: UserData) {
    this.currentUser$.next(user);
  }

  send() {
    const currentUser: UserData = this.currentUser$.value;

    if (currentUser) {
      this.messageCollection.add({
        content: this.message,
        toId: currentUser.id
      }).subscribe(console.log);

      this.message = '';
    }
  }
}
