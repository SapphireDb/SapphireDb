import { Component, OnInit } from '@angular/core';
import {RealtimeDatabase, UserData, RoleData} from 'ng-realtime-database';
import * as faker from 'faker';
import {Observable} from 'rxjs';

@Component({
  selector: 'app-manage',
  templateUrl: './manage.component.html',
  styleUrls: ['./manage.component.less']
})
export class ManageComponent implements OnInit {

  users$: Observable<UserData[]>;
  roles$: Observable<RoleData[]>;

  constructor(private db: RealtimeDatabase) { }

  ngOnInit() {
    this.users$ = this.db.auth.getUsers();
    this.roles$ = this.db.auth.getRoles();
  }

  createUser() {
    this.db.auth.createUser(
      faker.internet.userName(),
      faker.internet.email(),
      faker.internet.password(),
      ['admin', 'superTest'],
      {
        firstName: faker.name.firstName(),
        lastName: faker.name.lastName(),
        passwordHash: 'test'
      }
    ).subscribe(console.log);
  }

  updateUser(user: UserData) {
    this.db.auth.updateUser(user.id, null,
      null, null, ['user', 'test123'], { firstName: 'test123' })
      .subscribe(console.log);
  }

  deleteUser(user: UserData) {
    this.db.auth.deleteUser(user.id).subscribe(console.log);
  }

  createRole() {
    this.db.auth.createRole(faker.random.word())
      .subscribe(console.log);
  }

  updateRole(role: RoleData) {
    this.db.auth.updateRole(role.id, faker.random.word())
      .subscribe(console.log);
  }

  deleteRole(role: RoleData) {
    this.db.auth.deleteRole(role.id)
      .subscribe(console.log);
  }
}
