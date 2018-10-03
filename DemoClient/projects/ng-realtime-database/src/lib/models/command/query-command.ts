import {CollectionCommandBase} from './collection-command-base';
import {IPrefilter} from '../prefilter/iprefilter';

export class QueryCommand extends CollectionCommandBase {
  prefilters: IPrefilter[];

  constructor(collectionName: string, prefilters: IPrefilter[]) {
    super('QueryCommand', collectionName);
    this.prefilters = prefilters;
  }
}
