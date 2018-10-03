import {CollectionCommandBase} from './collection-command-base';

export class UnsubscribeCommand extends CollectionCommandBase {
  constructor(collectionName: string, referenceId: string) {
    super('UnsubscribeCommand', collectionName);
    this.referenceId = referenceId;
  }
}
