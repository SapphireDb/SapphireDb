import { Component, OnInit } from '@angular/core';
import {FormControl, FormGroup} from '@angular/forms';
import {AccountService} from '../../shared/services/account.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.less']
})
export class LoginComponent implements OnInit {

  public form: FormGroup;

  constructor(private accountService: AccountService) {
    this.form = new FormGroup({
      email: new FormControl(),
      password: new FormControl()
    });
  }

  ngOnInit() {
  }

  login() {
    this.accountService.login(this.form.get('email').value, this.form.get('password').value);
  }
}
