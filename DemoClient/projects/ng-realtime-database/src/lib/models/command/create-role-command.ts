import {CommandBase} from './command-base';

export class CreateRoleCommand extends CommandBase {
  name: string;

  constructor(name: string) {
    super('CreateRoleCommand');
    this.name = name;
  }
}
