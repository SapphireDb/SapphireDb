import {CommandBase} from './command-base';

export class LoginCommand extends CommandBase {
  username: string;
  password: string;

  constructor(username: string, password: string) {
    super('LoginCommand');
    this.username = username;
    this.password = password;
  }
}
