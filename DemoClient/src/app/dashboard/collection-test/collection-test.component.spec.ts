import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CollectionTestComponent } from './collection-test.component';

describe('CollectionTestComponent', () => {
  let component: CollectionTestComponent;
  let fixture: ComponentFixture<CollectionTestComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CollectionTestComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CollectionTestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
