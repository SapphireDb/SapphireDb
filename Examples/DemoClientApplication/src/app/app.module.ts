import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import {RealtimeDatabaseModule} from 'ng-realtime-database';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    RealtimeDatabaseModule.config({
      serverBaseUrl: 'realtimedatabasedemoapp.azurewebsites.net',
      useSecuredSocket: true
    })
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
