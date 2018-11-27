import {CommandBase} from './command-base';

export class UnsubscribeMessageCommand extends CommandBase {
  topic: string;

  constructor(topic: string, referenceId: string) {
    super('UnsubscribeMessageCommand');
    this.topic = topic;
    this.referenceId = referenceId;
  }
}
