import {CommandBase} from './command-base';

export class SubscribeMessageCommand extends CommandBase {
  topic: string;

  constructor(topic: string) {
    super('SubscribeMessageCommand');
    this.topic = topic;
  }
}
