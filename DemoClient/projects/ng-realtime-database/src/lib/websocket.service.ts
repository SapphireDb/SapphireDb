import {Inject, Injectable} from '@angular/core';
import {Observable, of, Subject} from 'rxjs';
import {SubscribeCommand} from './models/command/subscribe-command';
import {ResponseBase} from './models/response/response-base';
import {CommandReferences} from './models/command-references';
import {CommandBase} from './models/command/command-base';
import {finalize} from 'rxjs/operators';
import {UnsubscribeCommand} from './models/command/unsubscribe-command';
import {RealtimeDatabaseOptions} from './models/realtime-database-options';
import {SubscribeMessageCommand} from './models/command/subscribe-message-command';
import {UnsubscribeMessageCommand} from './models/command/unsubscribe-message-command';
import {GuidHelper} from './helper/guid-helper';
import {LocalstoragePaths} from './helper/localstorage-paths';
import {AuthData} from './models/auth-data';

@Injectable()
export class WebsocketService {
  private bearer: string;

  private socket: WebSocket;

  private unsendCommandStorage: CommandBase[] = [];

  private commandReferences: CommandReferences  = {};
  private serverMessageHandler: CommandReferences = {};

  constructor(@Inject('realtimedatabase.options') private options: RealtimeDatabaseOptions) {
    const authData = localStorage.getItem(LocalstoragePaths.authPath);

    if (authData) {
      this.bearer = JSON.parse(authData).authToken;
    } else {
      this.bearer = localStorage.getItem(LocalstoragePaths.bearerPath);
    }
  }

  private connectToWebsocket() {
    if (this.socket && this.socket.readyState !== WebSocket.CLOSED) {
      return;
    }

    let wsUrl = `${this.options.useSecuredSocket === true ? 'wss' : 'ws'}://${this.options.serverBaseUrl}/realtimedatabase/socket?`;

    if (this.options.secret) {
      wsUrl += `secret=${this.options.secret}&`;
    }

    if (!!this.bearer) {
      wsUrl += `bearer=${this.bearer}`;
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

        this.unsendCommandStorage = this.unsendCommandStorage
          .filter(cmd => cmd instanceof SubscribeCommand || cmd instanceof SubscribeMessageCommand);
      };

      waitCommand();
    };

    this.socket.onmessage = (msg: MessageEvent) => {
      this.handleResponse(JSON.parse(msg.data));
    };

    this.socket.onclose = () => {
      // for (const key of Object.keys(this.commandReferences)) {
      //   const commandReference = this.commandReferences[key];

        // if (!commandReference.keep) {
        //   commandReference.subject$.error('Websocket connection lost.');
        //   commandReference.subject$.complete();
        //   delete this.commandReferences[key];
        // }
      // }

      setTimeout(() => {
        this.connectToWebsocket();
      }, 1000);
    };

    this.socket.onerror = () => {
      this.socket.close();
    };
  }

  public sendCommand(command: CommandBase, keep?: boolean, onlySend?: boolean): Observable<ResponseBase> {
    this.connectToWebsocket();

    const referenceSubject = new Subject<ResponseBase>();
    this.commandReferences[command.referenceId] = { subject$: referenceSubject, keep: keep};

    if (this.socket.readyState === WebSocket.OPEN) {
      this.socket.send(JSON.stringify(command));
    } else {
      this.unsendCommandStorage.push(command);
    }

    if (command instanceof UnsubscribeCommand || command instanceof UnsubscribeMessageCommand) {
      this.unsendCommandStorage = this.unsendCommandStorage.filter(cs => cs.referenceId !== command.referenceId);
    } else if (command instanceof SubscribeCommand || command instanceof SubscribeMessageCommand) {
      if (this.unsendCommandStorage.findIndex(c => c.referenceId === command.referenceId) === -1) {
        this.unsendCommandStorage.push(command);
      }
    }

    if (onlySend === true) {
      referenceSubject.complete();
      referenceSubject.unsubscribe();
      delete this.commandReferences[command.referenceId];

      return of(null);
    } else {
      return referenceSubject.pipe(finalize(() => {
        delete this.commandReferences[command.referenceId];
      }));
    }
  }

  public registerServerMessageHandler(): Observable<ResponseBase> {
    const guid = GuidHelper.generateGuid();

    const referenceSubject = new Subject<ResponseBase>();
    this.serverMessageHandler[guid] = { subject$: referenceSubject, keep: true };

    return referenceSubject.pipe(finalize(() => {
      delete this.serverMessageHandler[guid];
    }));
  }

  private handleResponse(response: ResponseBase) {
    if (response.responseType === 'MessageResponse') {
      Object.keys(this.serverMessageHandler).map(k => this.serverMessageHandler[k]).forEach(handler => {
        if (response.error) {
          handler.subject$.error(response);
        }

        handler.subject$.next(response);
      });
    } else {
      const commandReference = this.commandReferences[response.referenceId];

      if (commandReference) {

        if (response.error) {
          commandReference.subject$.error(response.error);
        }

        commandReference.subject$.next(response);

        if (commandReference.keep !== true) {
          commandReference.subject$.complete();

          delete this.commandReferences[response.referenceId];
        }
      }
    }
  }

  public setBearer(bearer?: string) {
    if (bearer) {
      this.bearer = bearer;

      if (!localStorage.getItem(LocalstoragePaths.authPath)) {
        localStorage.setItem(LocalstoragePaths.bearerPath, bearer);
      }
    } else {
      localStorage.removeItem(LocalstoragePaths.bearerPath);
    }

    if (this.socket) {
      this.socket.close();
    }

    this.connectToWebsocket();
  }
}
