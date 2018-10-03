import { TestBed, inject } from '@angular/core/testing';

import { RealtimeDatabase } from './realtime-database.service';

describe('RealtimeDatabase', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [RealtimeDatabase]
    });
  });

  it('should be created', inject([RealtimeDatabase], (service: RealtimeDatabase) => {
    expect(service).toBeTruthy();
  }));
});
