import { Component } from '@angular/core';
import {RealtimeDatabase, UserData} from 'ng-realtime-database';
import {Observable} from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.less']
})
export class AppComponent {
  userInfo$: Observable<UserData>;

  constructor(private db: RealtimeDatabase) {
    this.userInfo$ = this.db.auth.getUserData();
  }

}
