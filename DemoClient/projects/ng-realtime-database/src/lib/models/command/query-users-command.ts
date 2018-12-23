import {CommandBase} from './command-base';

export class QueryUsersCommand extends CommandBase {
  constructor() {
    super('QueryUsersCommand');
  }
}
