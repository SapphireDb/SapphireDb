import {CommandBase} from './command-base';

export class RenewCommand extends CommandBase {
  userId: string;
  refreshToken: string;

  constructor(userId: string, refreshToken: string) {
    super('RenewCommand');
    this.userId = userId;
    this.refreshToken = refreshToken;
  }
}
