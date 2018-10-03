import {CollectionCommandBase} from './collection-command-base';

export class InfoCommand extends CollectionCommandBase {
  constructor(collectionName: string) {
    super('InfoCommand', collectionName);
  }
}
