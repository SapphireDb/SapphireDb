import {CollectionCommandBase} from './collection-command-base';
import {CommandBase} from './command-base';

export class UnsubscribeRolesCommand extends CommandBase {
  constructor() {
    super('UnsubscribeRolesCommand');
  }
}
