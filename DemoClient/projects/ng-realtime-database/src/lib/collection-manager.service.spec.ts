import { TestBed } from '@angular/core/testing';

import { CollectionManagerService } from './collection-manager.service';

describe('CollectionManagerService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: CollectionManagerService = TestBed.get(CollectionManagerService);
    expect(service).toBeTruthy();
  });
});
