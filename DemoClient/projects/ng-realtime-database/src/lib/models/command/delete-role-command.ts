import {CommandBase} from './command-base';

export class DeleteRoleCommand extends CommandBase {
  id: string;

  constructor(id: string) {
    super('DeleteRoleCommand');
    this.id = id;
  }
}
