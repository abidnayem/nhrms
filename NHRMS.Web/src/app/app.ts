import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  get navItems() {
    return [
      { label: 'Overview', path: '/overview', permission: 'REPORT_READ' },
      { label: 'Employees', path: '/employees', permission: 'EMPLOYEE_READ' },
      { label: 'Onboarding', path: '/onboarding', permission: 'ONBOARDING_MANAGE' },
      { label: 'Probation', path: '/probation', permission: 'PROBATION_REVIEW' },
      { label: 'Employment history', path: '/employment-changes', permission: 'EMPLOYEE_READ' },
      { label: 'Employee relations', path: '/discipline', permission: 'DISCIPLINE_READ' },
      { label: 'Attendance', path: '/attendance', permission: 'ATTENDANCE_MANAGE' },
      { label: 'Leave', path: '/leave', permission: 'LEAVE_MANAGE' },
      { label: 'Payroll', path: '/payroll', permission: 'PAYROLL_RUN' },
      { label: 'Access', path: '/access', permission: 'USER_ADMIN' }
    ].filter(item => this.auth.hasPermission(item.permission));
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}
