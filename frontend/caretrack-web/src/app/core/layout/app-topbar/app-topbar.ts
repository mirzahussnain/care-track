import { Component, input } from '@angular/core';

@Component({
  selector: 'app-topbar',
  imports: [],
  templateUrl: './app-topbar.html',
  styleUrl: './app-topbar.css',
})
export class AppTopbar {
    readonly areaLabel=input.required<string>();
}