import {CommandBase} from './command-base';

export class MessageCommand extends CommandBase {
  data: any;

  constructor(data: any) {
    super('MessageCommand');
    this.data = data;
  }
}
