import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {MainComponent} from './main/main.component';
import {TestComponent} from './test/test.component';
import {AuthComponent} from './auth/auth.component';

const routes: Routes = [
  { path: '', component: MainComponent },
  { path: 'test', component: TestComponent },
  { path: 'auth', component: AuthComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
