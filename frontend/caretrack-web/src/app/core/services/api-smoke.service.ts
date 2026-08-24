import { HttpClient } from "@angular/common/http";
import { inject,Injectable } from "@angular/core";
import { environment } from "../../../environments/environment";

@Injectable({
    providedIn:'root',
})

export class ApiSmokeService {
  private readonly http =
    inject(HttpClient);

  getPatient() {
    return this.http.get(
      `${environment.apiBaseUrl}/api/patients`
    );
  }
}