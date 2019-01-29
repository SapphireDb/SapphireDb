import { Component } from '@angular/core';
import {Collection, RealtimeDatabase} from 'ng-realtime-database';
import {Observable} from 'rxjs';
import {User} from './model/user';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.less']
})
export class AppComponent {
  title = 'DemoClientApplication';

  userCollection: Collection<User>;
  users$: Observable<User[]>;

  constructor(private db: RealtimeDatabase) {
    this.userCollection = this.db.collection<User>('users');
    this.users$ = this.userCollection.values();
  }

  create() {
    const result = prompt('Enter new username');

    if (result) {
      this.userCollection.add({
        firstname: result,
        lastname: result,
        username: result,
        email: result
      }).subscribe(c => console.log(c));
    }
  }

  updateUser(user: User) {
    user.firstname = 'das ist ein test';
    this.userCollection.update(user);
  }
}
