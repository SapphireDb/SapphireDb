import {IPrefilter} from '../prefilter/iprefilter';
import {QueryCommand} from './query-command';

export class SubscribeCommand extends QueryCommand {
  constructor(collectionName: string, prefilters: IPrefilter<any>[]) {
    super(collectionName, prefilters);
    this.commandType = 'SubscribeCommand';
  }
}
