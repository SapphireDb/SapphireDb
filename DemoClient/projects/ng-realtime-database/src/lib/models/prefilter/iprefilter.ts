export interface IPrefilter<T> {
  prefilterType: string;
  execute(values: T[]): T[];
  hash(): string;
}
