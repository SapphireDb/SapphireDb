import {CommandBase} from './command-base';

export class PublishCommand extends CommandBase {
  topic: string;
  data: any;

  constructor(topic: string, data: any) {
    super('PublishCommand');
    this.topic = topic;
    this.data = data;
  }
}
