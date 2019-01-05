import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DashboardRoutingModule } from './dashboard-routing.module';
import { MainComponent } from './main/main.component';
import {FormsModule} from '@angular/forms';
import { TestComponent } from './test/test.component';
import { AuthComponent } from './auth/auth.component';
import { CollectionTestComponent } from './collection-test/collection-test.component';
import {Angular2PromiseButtonModule} from 'angular2-promise-buttons';
import {RealtimeAuthGuard} from '../../../projects/ng-realtime-database/src/lib/realtime-auth.guard';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    DashboardRoutingModule,
    Angular2PromiseButtonModule,
  ],
  declarations: [MainComponent, TestComponent, AuthComponent, CollectionTestComponent]
})
export class DashboardModule { }
