export interface IPrefilter {
  prefilterType: string;
  execute(values: any[]): any[];
  hash(): string;
}
