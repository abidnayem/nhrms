import { Routes } from '@angular/router';
import { AccessPageComponent } from './pages/access/access.component';
import { AttendancePageComponent } from './pages/attendance/attendance.component';
import { EmployeesPageComponent } from './pages/employees/employees.component';
import { LeavePageComponent } from './pages/leave/leave.component';
import { OverviewPageComponent } from './pages/overview/overview.component';
import { PayrollPageComponent } from './pages/payroll/payroll.component';

export const routes: Routes = [
  { path: '', redirectTo: 'overview', pathMatch: 'full' },
  { path: 'overview', component: OverviewPageComponent },
  { path: 'employees', component: EmployeesPageComponent },
  { path: 'attendance', component: AttendancePageComponent },
  { path: 'leave', component: LeavePageComponent },
  { path: 'payroll', component: PayrollPageComponent },
  { path: 'access', component: AccessPageComponent },
  { path: '**', redirectTo: 'overview' }
];
