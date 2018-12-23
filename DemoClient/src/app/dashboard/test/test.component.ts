import { Component, OnInit } from '@angular/core';
import {RealtimeDatabase} from 'ng-realtime-database';
import * as faker from 'faker';
import {User} from '../../model/user';

@Component({
  selector: 'app-test',
  templateUrl: './test.component.html',
  styleUrls: ['./test.component.less']
})
export class TestComponent implements OnInit {

  constructor(private db: RealtimeDatabase) { }

  ngOnInit() {
    const collection = this.db.collection<User>('users');

    collection.values().subscribe(v => {
      v.forEach(u => {
        collection.remove(u).subscribe(console.log);
      });
    });

    // for (let i = 0; i < 50; i++) {
    //   collection.add({
    //     username: faker.internet.password(),
    //     firstName: faker.name.firstName(),
    //     lastName: faker.name.lastName()
    //   }).subscribe(console.log);
    // }

  }
}
