import { TestBed, async, inject } from '@angular/core/testing';

import { RealtimeAuthGuard } from './realtime-auth.guard';

describe('RealtimeAuthGuard', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [RealtimeAuthGuard]
    });
  });

  it('should ...', inject([RealtimeAuthGuard], (guard: RealtimeAuthGuard) => {
    expect(guard).toBeTruthy();
  }));
});
