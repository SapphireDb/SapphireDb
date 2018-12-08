import { Component, OnInit } from '@angular/core';
import {RealtimeDatabase} from 'ng-realtime-database';

@Component({
  selector: 'app-test',
  templateUrl: './test.component.html',
  styleUrls: ['./test.component.less']
})
export class TestComponent implements OnInit {

  username: string;

  constructor(private db: RealtimeDatabase) { }

  ngOnInit() {
  }

  createUser() {
    this.db.execute('user', 'CreateUser',
      {firstName: this.username, lastName: this.username, email: this.username, password: 'pw1234'}, 'test').subscribe(console.log);
  }
}
