import { TestBed } from '@angular/core/testing';

import { CollectionInformationService } from './collection-information.service';

describe('CollectionInformationService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: CollectionInformationService = TestBed.get(CollectionInformationService);
    expect(service).toBeTruthy();
  });
});
