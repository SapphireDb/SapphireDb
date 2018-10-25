import {Inject, Injectable} from '@angular/core';
import {Observable, Subject} from 'rxjs';
import {SubscribeCommand} from './models/command/subscribe-command';
import {ResponseBase} from './models/response/response-base';
import {CommandReferences} from './models/command-references';
import {CommandBase} from './models/command/command-base';
import {finalize} from 'rxjs/operators';
import {UnsubscribeCommand} from './models/command/unsubscribe-command';
import {RealtimeDatabaseOptions} from './models/realtime-database-options';

@Injectable()
export class WebsocketService {
  private bearer: string;

  private socket: WebSocket;

  private subscribeCommandStorage: SubscribeCommand[] = [];
  private unsendCommandStorage: CommandBase[] = [];

  private commandReferences: CommandReferences  = {};

  constructor(@Inject('realtimedatabase.options') private options: RealtimeDatabaseOptions) {
    this.bearer = localStorage.getItem('realtimedatabase.bearer');
  }

  private connectToWebsocket() {
    if (this.socket && this.socket.readyState !== WebSocket.CLOSED) {
      return;
    }

    let wsUrl = `${this.options.useSecuredSocket === true ? 'wss' : 'ws'}://${this.options.serverBaseUrl}/realtimedatabase/socket`;

    if (!!this.bearer) {
      wsUrl += `?bearer=${this.bearer}`;
    }

    this.socket = new WebSocket(wsUrl);

    this.socket.onopen = () => {
      const waitCommand = () => {
        if (this.socket.readyState !== WebSocket.OPEN) {
          setTimeout(waitCommand, 25);
        }

        this.unsendCommandStorage.forEach(cmd => {
          this.socket.send(JSON.stringify(cmd));
        });

        this.unsendCommandStorage = this.unsendCommandStorage.filter(cmd => cmd instanceof SubscribeCommand);
      };

      waitCommand();
    };

    this.socket.onmessage = (msg: MessageEvent) => {
      this.handleResponse(JSON.parse(msg.data));
    };

    this.socket.onclose = () => {
      setTimeout(() => {
        this.connectToWebsocket();
      }, 1000);
    };

    this.socket.onerror = () => {
      this.socket.close();
    };
  }

  public sendCommand(command: CommandBase, keep?: boolean): Observable<ResponseBase> {
    this.connectToWebsocket();

    const referenceSubject = new Subject<ResponseBase>();
    this.commandReferences[command.referenceId] = { subject$: referenceSubject, keep: keep};

    if (this.socket.readyState === WebSocket.OPEN) {
      this.socket.send(JSON.stringify(command));
    } else {
      this.unsendCommandStorage.push(command);
    }

    if (command instanceof UnsubscribeCommand) {
      this.subscribeCommandStorage = this.subscribeCommandStorage.filter(cs => cs.referenceId !== command.referenceId);
    } else if (command instanceof SubscribeCommand) {
      this.subscribeCommandStorage.push(command);
    }

    return referenceSubject.pipe(finalize(() => {
      delete this.commandReferences[command.referenceId];
    }));
  }

  private handleResponse(response: ResponseBase) {
    const commandReference = this.commandReferences[response.referenceId];

    if (commandReference) {

      commandReference.subject$.next(response);

      if (response.error) {
        commandReference.subject$.error(response.error);
      }

      if (commandReference.keep !== true) {
        commandReference.subject$.complete();

        delete this.commandReferences[response.referenceId];
      }
    }
  }

  public setBearer(bearer?: string) {
    this.bearer = bearer;
    localStorage.setItem('realtimedatabase.bearer', bearer);

    if (this.socket) {
      this.socket.close();
    }

    this.connectToWebsocket();
  }
}
