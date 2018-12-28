import {Observable, of} from 'rxjs';
import {SubscribeMessageCommand} from './command/subscribe-message-command';
import {finalize, map} from 'rxjs/operators';
import {TopicResponse} from './response/topic-response';
import {UnsubscribeMessageCommand} from './command/unsubscribe-message-command';
import {WebsocketService} from '../websocket.service';
import {PublishCommand} from './command/publish-command';
import {MessageResponse} from './response/message-response';
import {MessageCommand} from './command/message-command';

export class Messaging {
  constructor(private websocket: WebsocketService) {

  }

  /**
   * Get all messages for the client
   */
  public messages(): Observable<any> {
    return this.websocket.registerServerMessageHandler().pipe(map((response: MessageResponse) => {
      return response.data;
    }));
  }

  /**
   * Subscribe to a topic on the server
   * @param topic The topic to subscribe
   */
  public topic(topic: string): Observable<any> {
    const subscribeCommand = new SubscribeMessageCommand(topic);
    return this.websocket.sendCommand(subscribeCommand, true).pipe(
      map((response: TopicResponse) => {
        return response.message;
      }),
      finalize(() => {
        this.websocket.sendCommand(new UnsubscribeMessageCommand(topic, subscribeCommand.referenceId), false, true);
      })
    );
  }

  /**
   * Send data to other clients
   * @param data The data to send
   */
  public send(data: any) {
    this.websocket.sendCommand(new MessageCommand(data), false, true);
  }

  /**
   * Publish data to a topic
   * @param topic The topic for publishing
   * @param data The data to publish
   */
  public publish(topic: string, data: any) {
    this.websocket.sendCommand(new PublishCommand(topic, data), false, true);
  }
}
