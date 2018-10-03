export class CommandResult<T> {
  error: any;
  validationResults: { [propertyName: string]: string[] };

  value?: T;

  constructor(error: any, validationResults: { [propertyName: string]: string[] }, value?: T) {
    this.error = error;
    this.validationResults = validationResults;
    this.value = value;
  }

  hasSuccess(): boolean {
    return !this.hasErrors() && !this.hasValidationErrors();
  }

  hasErrors(): boolean {
    return this.error;
  }

  hasValidationErrors(): boolean {
    return this.validationResults != null;
  }
}
