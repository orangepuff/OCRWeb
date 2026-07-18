import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OCRWebFrontendShared } from './ocrweb.frontend.shared';

describe('OCRWebFrontendShared', () => {
  let component: OCRWebFrontendShared;
  let fixture: ComponentFixture<OCRWebFrontendShared>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OCRWebFrontendShared],
    }).compileComponents();

    fixture = TestBed.createComponent(OCRWebFrontendShared);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
