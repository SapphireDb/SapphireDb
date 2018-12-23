import {CommandBase} from './command-base';

export class CreateUserCommand extends CommandBase {
  userName: string;
  email: string;
  password: string;
  roles: string[];
  additionalData: { [key: string]: any };

  constructor(userName: string, email: string, password: string, roles: string[], addtionalData: { [key: string]: any }) {
    super('CreateUserCommand');
    this.userName = userName;
    this.email = email;
    this.password = password;
    this.roles = roles;
    this.additionalData = addtionalData;
  }
}
