import { Component, OnInit } from '@angular/core';
import {RealtimeDatabase} from 'ng-realtime-database';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.less']
})
export class AuthComponent implements OnInit {

  username: string;
  password: string;

  constructor(private db: RealtimeDatabase) { }

  ngOnInit() {
  }

  login() {
    this.db.auth.login(this.username, this.password).subscribe(c => console.log(c));
  }
}

