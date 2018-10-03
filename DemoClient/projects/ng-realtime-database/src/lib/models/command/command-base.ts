import {GuidHelper} from '../../helper/guid-helper';

export class CommandBase {
  commandType: string;
  referenceId: string;

  constructor(commandType: string) {
    this.commandType = commandType;
    this.referenceId = GuidHelper.generateGuid();
  }
}
