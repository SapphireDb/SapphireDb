export class GuidHelper {
  static generateGuid(): string {
    const template = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx';

    return template.replace(/[xy]/g, (c) => {
      const r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }
}
