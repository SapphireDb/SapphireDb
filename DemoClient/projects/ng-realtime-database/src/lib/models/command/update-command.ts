import {CollectionCommandBase} from './collection-command-base';

export class UpdateCommand  extends CollectionCommandBase {
  updateValue: any;

  constructor(collectionName: string, updateValue: any) {
    super('UpdateCommand', collectionName);
    this.updateValue = updateValue;
  }
}
