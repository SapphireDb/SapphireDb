import {CollectionCommandBase} from './collection-command-base';
import {CommandBase} from './command-base';

export class UnsubscribeUsersCommand extends CommandBase {
  constructor() {
    super('UnsubscribeUsersCommand');
  }
}
