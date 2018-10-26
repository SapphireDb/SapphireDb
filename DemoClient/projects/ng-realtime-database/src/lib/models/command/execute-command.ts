import {CommandBase} from './command-base';

export class ExecuteCommand extends CommandBase {
  actionHandlerName: string;
  actionName: string;
  parameters: any[];

  constructor(actionHandlerName: string, actionName: string, parameters: any[]) {
    super('ExecuteCommand');
    this.actionHandlerName = actionHandlerName;
    this.actionName = actionName;
    this.parameters = parameters;
  }
}
