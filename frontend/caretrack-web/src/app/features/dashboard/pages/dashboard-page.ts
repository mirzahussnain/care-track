import { Component, inject } from "@angular/core";
import { ApiSmokeService } from "../../../core/services/api-smoke.service";


@Component({
    selector:'app-dashboard-page',
    templateUrl:'./dashboard-page.html',
    styleUrl:'./dashboard-page.css'
})

export class DashboardPage{
    private readonly apiSmokeService=inject(ApiSmokeService);
    ngOnInit():void{
         console.log('DASHBOARD INIT');
        this.apiSmokeService.getPatient().subscribe({
            next:response=>{
                console.log('Protected API success',response);
            },
            error:error=>{
                console.error('Protected API failed',error);
            },
        });
    }
}