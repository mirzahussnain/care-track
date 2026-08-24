import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { MsalBroadcastService,MsalService } from '@azure/msal-angular';
import { InteractionStatus } from '@azure/msal-browser';
import { of } from 'rxjs';

import { App } from './app';

describe('AppComponent', () => {
  const msalServiceMock={
    handleRedirectObservable:vi.fn(()=>of(null)),
    instance:{
      getActiveAccount:vi.fn(()=>of(null)),
       getAllAccounts: vi.fn(() => []),
       setActiveAccount: vi.fn(),
    }
  };

  const msalBroadcastServiceMock = {
    inProgress$: of(
      InteractionStatus.None
    ),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers:[provideRouter([]),
        {
          provide: MsalService,
          useValue: msalServiceMock,
        },

        {
          provide: MsalBroadcastService,
          useValue:msalBroadcastServiceMock,
        },
    
    
    ],

    }).compileComponents();
  });

  it('creates the app', () => {
    const fixture =
      TestBed.createComponent(App);

    expect(fixture.componentInstance)
      .toBeTruthy();
  });
});