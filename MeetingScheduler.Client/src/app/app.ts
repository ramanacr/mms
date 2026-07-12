import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastHostComponent } from './core/toast-host.component';

@Component({
  standalone: true,
  selector: 'app-root',
  imports: [RouterOutlet, ToastHostComponent],
  template: '<router-outlet /><app-toast-host />',
  styleUrls: ['./app.scss']
})
export class App {
}
