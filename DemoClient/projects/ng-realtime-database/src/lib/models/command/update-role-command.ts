import {CommandBase} from './command-base';

export class UpdateRoleCommand extends CommandBase {
  id: string;
  name: string;

  constructor(id: string, name: string) {
    super('UpdateRoleCommand');
    this.id = id;
    this.name = name;
  }
}
