import {CollectionCommandBase} from './collection-command-base';

export class DeleteCommand extends CollectionCommandBase {
  primaryKeys: { [propertyName: string]: any };

  constructor(collectionName: string, primaryKeys: { [propertyName: string]: any }) {
    super('DeleteCommand', collectionName);
    this.primaryKeys = primaryKeys;
  }
}
