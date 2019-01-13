import { Component, OnInit } from '@angular/core';
import {RealtimeDatabase, Collection, OrderByPrefilter, ThenOrderByPrefilter} from 'ng-realtime-database';
import {User} from '../../model/user';
import * as faker from 'faker';

@Component({
  selector: 'app-collection-test',
  templateUrl: './collection-test.component.html',
  styleUrls: ['./collection-test.component.less']
})
export class CollectionTestComponent implements OnInit {

  collection: Collection<User>;

  constructor(private db: RealtimeDatabase) { }

  ngOnInit() {
    this.collection = this.db.collection<User>('users');

    // const sub1 = this.collection.values(new OrderByPrefilter(x => x.id)).subscribe(console.table);
    // const sub2 = this.collection.values().subscribe(console.table);
    const sub3 = this.collection.values(new OrderByPrefilter(x => x.username),
      new ThenOrderByPrefilter(x => x.id, true)).subscribe(console.table);
  }

  addUser() {
    this.collection.add({
      username: faker.internet.password(),
      firstName: faker.name.firstName(),
      lastName: faker.name.lastName()
    }).subscribe(console.log);
  }

}
