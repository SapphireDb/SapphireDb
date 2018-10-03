import {CollectionCommandBase} from './collection-command-base';

export class CreateCommand extends CollectionCommandBase {
  value: any;

  constructor(collectionName: string, value: any) {
    super('CreateCommand', collectionName);
    this.value = value;
  }
}
