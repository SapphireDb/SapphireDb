import {Inject, Injectable} from '@angular/core';
import {Observable, of, Subject} from 'rxjs';
import {SubscribeCommand} from './models/command/subscribe-command';
import {ResponseBase} from './models/response/response-base';
import {CommandReferences} from './models/command-references';
import {CommandBase} from './models/command/command-base';
import {finalize, shareReplay, switchMap, take} from 'rxjs/operators';
import {UnsubscribeCommand} from './models/command/unsubscribe-command';
import {RealtimeDatabaseOptions} from './models/realtime-database-options';
import {SubscribeMessageCommand} from './models/command/subscribe-message-command';
import {UnsubscribeMessageCommand} from './models/command/unsubscribe-message-command';
import {GuidHelper} from './helper/guid-helper';
import {LocalstoragePaths} from './helper/localstorage-paths';
import {AuthData} from './models/auth-data';
import {UnsubscribeUsersCommand} from './models/command/unsubscribe-users-command';
import {UnsubscribeRolesCommand} from './models/command/unsubscribe-roles-command';
import {SubscribeUsersCommand} from './models/command/subscribe-users-command';
import {SubscribeRolesCommand} from './models/command/subscribe-roles-command';
import {MessageResponse} from './models/response/message-response';

@Injectable()
export class WebsocketService {
  private bearer: string;

  private socket: WebSocket;
  private connectSubject$;

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

  private createWebsocket() {
    this.socket = new WebSocket(this.createWebsocketConnectionString());

    this.socket.onopen = () => {
      const waitCommand = () => {
        if (this.socket.readyState !== WebSocket.OPEN) {
          setTimeout(waitCommand, 25);
        }

        this.unsendCommandStorage.forEach(cmd => {
          this.socket.send(JSON.stringify(cmd));
        });

        this.connectSubject$.next(true);
        this.connectSubject$.complete();
        this.connectSubject$ = null;
      };

      waitCommand();
    };

    this.socket.onmessage = (msg: MessageEvent) => {
      this.handleResponse(JSON.parse(msg.data));
    };

    this.socket.onclose = () => {
      setTimeout(() => {
        this.connectToWebsocket(true);
      }, 1000);
    };

    this.socket.onerror = () => {
      this.socket.close();
    };
  }

  private createWebsocketConnectionString(): string {
    let wsUrl = `${this.options.useSecuredSocket === true ? 'wss' : 'ws'}://${this.options.serverBaseUrl}/realtimedatabase/socket?`;

    if (this.options.secret) {
      wsUrl += `secret=${this.options.secret}&`;
    }

    if (!!this.bearer) {
      wsUrl += `bearer=${this.bearer}`;
    }

    return wsUrl;
  }

  private connectToWebsocket(connectionFailed: boolean = false): Observable<boolean> {
    if (!this.connectSubject$ && this.socket && this.socket.readyState === WebSocket.OPEN) {
      return of(true);
    }

    if (!this.connectSubject$ || connectionFailed) {
      if (!connectionFailed) {
        this.connectSubject$ = new Subject<boolean>();
      }

      this.createWebsocket();
    }

    return this.connectSubject$;
  }

  private storeSubscribeCommands(command: CommandBase) {
    if (command instanceof UnsubscribeCommand || command instanceof UnsubscribeMessageCommand
      || command instanceof UnsubscribeUsersCommand || command instanceof UnsubscribeRolesCommand) {
      this.unsendCommandStorage = this.unsendCommandStorage.filter(cs => cs.referenceId !== command.referenceId);
    } else if (command instanceof SubscribeCommand || command instanceof SubscribeMessageCommand
      || command instanceof SubscribeUsersCommand || command instanceof SubscribeRolesCommand) {
      if (this.unsendCommandStorage.findIndex(c => c.referenceId === command.referenceId) === -1) {
        this.unsendCommandStorage.push(command);
      }
    }
  }

  private createHotCommandObservable(referenceObservable$: Observable<ResponseBase>, command: CommandBase): Observable<ResponseBase> {
    const makeHotSubject$ = new Subject<ResponseBase>();
    referenceObservable$.subscribe(c => makeHotSubject$.next(c), ex => makeHotSubject$.error(ex));
    return makeHotSubject$.asObservable().pipe(finalize(() => {
      delete this.commandReferences[command.referenceId];
    }));
  }

  public sendCommand(command: CommandBase, keep?: boolean, onlySend?: boolean): Observable<ResponseBase> {
    const referenceObservable$ = this.connectToWebsocket().pipe(take(1), switchMap((v) => {
      const referenceSubject = new Subject<ResponseBase>();
      this.commandReferences[command.referenceId] = { subject$: referenceSubject, keep: keep};
      this.socket.send(JSON.stringify(command));
      this.storeSubscribeCommands(command);

      if (onlySend === true) {
        referenceSubject.complete();
        referenceSubject.unsubscribe();
        delete this.commandReferences[command.referenceId];

        return of(null);
      } else {
        return referenceSubject;
      }
    })).pipe(shareReplay());

    return this.createHotCommandObservable(referenceObservable$, command);
  }

  public registerServerMessageHandler(): Observable<ResponseBase> {
    const guid = GuidHelper.generateGuid();

    const referenceSubject = new Subject<ResponseBase>();
    this.serverMessageHandler[guid] = { subject$: referenceSubject, keep: true };

    return referenceSubject.pipe(finalize(() => {
      delete this.serverMessageHandler[guid];
    }));
  }

  private handleMessageResponse(response: MessageResponse) {
    Object.keys(this.serverMessageHandler).map(k => this.serverMessageHandler[k]).forEach(handler => {
      if (response.error) {
        handler.subject$.error(response);
      }

      handler.subject$.next(response);
    });
  }

  private handleResponse(response: ResponseBase) {
    if (response.responseType === 'MessageResponse') {
      this.handleMessageResponse(<MessageResponse>response);
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
      this.socket.onclose = () => {};
      this.socket.close();
    }

    setTimeout(() => {
      this.connectToWebsocket();
    }, 10);
  }
}
