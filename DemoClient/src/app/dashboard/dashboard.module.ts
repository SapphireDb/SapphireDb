import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DashboardRoutingModule } from './dashboard-routing.module';
import { MainComponent } from './main/main.component';
import {FormsModule} from '@angular/forms';
import { TestComponent } from './test/test.component';
import { AuthComponent } from './auth/auth.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    DashboardRoutingModule
  ],
  declarations: [MainComponent, TestComponent, AuthComponent]
})
export class DashboardModule { }
