import { AccountModule } from './account.module';

describe('AccountModule', () => {
  let accountModule: AccountModule;

  beforeEach(() => {
    accountModule = new AccountModule();
  });

  it('should create an instance', () => {
    expect(accountModule).toBeTruthy();
  });
});
