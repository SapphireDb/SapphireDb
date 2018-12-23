import {CommandBase} from './command-base';

export class DeleteUserCommand extends CommandBase {
  id: string;

  constructor(id: string) {
    super('DeleteUserCommand');
    this.id = id;
  }
}
