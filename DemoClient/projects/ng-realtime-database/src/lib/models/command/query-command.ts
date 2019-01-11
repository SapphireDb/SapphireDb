import {CollectionCommandBase} from './collection-command-base';
import {IPrefilter} from '../prefilter/iprefilter';

export class QueryCommand extends CollectionCommandBase {
  prefilters: IPrefilter<any>[];

  constructor(collectionName: string, prefilters: IPrefilter<any>[]) {
    super('QueryCommand', collectionName);
    this.prefilters = prefilters;
  }
}
