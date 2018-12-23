import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {LoginComponent} from './login/login.component';
import {AuthGuard} from '../shared/auth.guard';
import {ManageComponent} from './manage/manage.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'manage', component: ManageComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AccountRoutingModule { }
