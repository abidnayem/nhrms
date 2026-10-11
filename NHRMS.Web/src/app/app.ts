import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  readonly navItems = [
    { label: 'Overview', path: '/overview' },
    { label: 'Employees', path: '/employees' },
    { label: 'Attendance', path: '/attendance' },
    { label: 'Leave', path: '/leave' },
    { label: 'Payroll', path: '/payroll' },
    { label: 'Access', path: '/access' }
  ];
}
