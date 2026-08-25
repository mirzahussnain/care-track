import {
  HttpClient,
} from '@angular/common/http';

import {
  inject,
  Injectable,
} from '@angular/core';

import {
  Observable,
} from 'rxjs';

import {
  environment,
} from '../../../environments/environment';

import {
  AuthenticatedUser,
} from './auth.models';

@Injectable({
  providedIn: 'root',
})
export class CurrentUserApiService {
  private readonly http =
    inject(HttpClient);

  getCurrentUser():
    Observable<AuthenticatedUser> {
    return this.http.get<AuthenticatedUser>(
      `${environment.apiBaseUrl}/api/me`
    );
  }
}