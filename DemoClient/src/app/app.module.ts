import {BrowserModule} from '@angular/platform-browser';
import {NgModule} from '@angular/core';

import {AppRoutingModule} from './app-routing.module';
import {AppComponent} from './app.component';
import {RealtimeDatabaseModule} from 'ng-realtime-database';
import {ReactiveFormsModule} from '@angular/forms';
import {HttpClientModule} from '@angular/common/http';
import {Angular2PromiseButtonModule} from 'angular2-promise-buttons';
import {RealtimeAuthGuard} from '../../projects/ng-realtime-database/src/lib/realtime-auth.guard';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    ReactiveFormsModule,
    HttpClientModule,
    RealtimeDatabaseModule.config({
      serverBaseUrl: `${location.hostname}:${location.port}`,
      loginRedirect: 'account/login',
      unauthorizedRedirect: 'account/unauthorized'
      // secret: 'test123'
    }),
    AppRoutingModule,
    Angular2PromiseButtonModule.forRoot()
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
