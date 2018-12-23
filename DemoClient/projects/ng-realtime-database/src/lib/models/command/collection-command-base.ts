import {CommandBase} from './command-base';

export class CollectionCommandBase extends CommandBase {
  collectionName: string;

  constructor(commandType: string, collectionName: string) {
    super(commandType);
    this.collectionName = collectionName;
  }
}
