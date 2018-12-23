import {CommandBase} from './command-base';
import {CreateUserCommand} from './create-user-command';

export class UpdateUserCommand extends CreateUserCommand {
  id: string;

  constructor(id: string, userName: string, email: string, password: string, roles: string[], addtionalData: { [key: string]: any }) {
    super(userName, email, password, roles, addtionalData);
    this.commandType = 'UpdateUserCommand';
    this.id = id;
  }
}
