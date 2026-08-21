import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-topbar',
  imports: [],
  templateUrl: './app-topbar.html',
  styleUrl: './app-topbar.css',
})
export class AppTopbar {
    readonly areaLabel=input.required<string>();
    readonly mobileMenuOpen = output<void>();
}