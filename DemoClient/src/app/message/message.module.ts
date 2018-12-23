import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MessageRoutingModule } from './message-routing.module';
import { MainComponent } from './main/main.component';
import {FormsModule} from '@angular/forms';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MessageRoutingModule
  ],
  declarations: [MainComponent]
})
export class MessageModule { }
