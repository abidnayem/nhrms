import { Routes } from '@angular/router';
import { AccessPageComponent } from './pages/access/access.component';
import { AttendancePageComponent } from './pages/attendance/attendance.component';
import { EmployeesPageComponent } from './pages/employees/employees.component';
import { LeavePageComponent } from './pages/leave/leave.component';
import { OverviewPageComponent } from './pages/overview/overview.component';
import { OnboardingPageComponent } from './pages/onboarding/onboarding.component';
import { LoginPageComponent } from './pages/login/login.component';
import { permissionGuard } from './permission.guard';
import { ForbiddenPageComponent } from './pages/forbidden/forbidden.component';
import { ProbationPageComponent } from './pages/probation/probation.component';
import { PayrollPageComponent } from './pages/payroll/payroll.component';
import { EmploymentChangesPageComponent } from './pages/employment-changes/employment-changes.component';
import { DisciplinePageComponent } from './pages/discipline/discipline.component';

export const routes: Routes = [
  { path: 'login', component: LoginPageComponent },
  { path: 'forbidden', component: ForbiddenPageComponent },
  { path: '', redirectTo: 'overview', pathMatch: 'full' },
  { path: 'overview', component: OverviewPageComponent, canActivate: [permissionGuard], data: { permission: 'REPORT_READ' } },
  { path: 'employees', component: EmployeesPageComponent, canActivate: [permissionGuard], data: { permission: 'EMPLOYEE_READ' } },
  { path: 'onboarding', component: OnboardingPageComponent, canActivate: [permissionGuard], data: { permission: 'ONBOARDING_MANAGE' } },
  { path: 'probation', component: ProbationPageComponent, canActivate: [permissionGuard], data: { permission: 'PROBATION_REVIEW' } },
  { path: 'employment-changes', component: EmploymentChangesPageComponent, canActivate: [permissionGuard], data: { permission: 'EMPLOYEE_READ' } },
  { path: 'discipline', component: DisciplinePageComponent, canActivate: [permissionGuard], data: { permission: 'DISCIPLINE_READ' } },
  { path: 'attendance', component: AttendancePageComponent, canActivate: [permissionGuard], data: { permission: 'ATTENDANCE_MANAGE' } },
  { path: 'leave', component: LeavePageComponent, canActivate: [permissionGuard], data: { permission: 'LEAVE_MANAGE' } },
  { path: 'payroll', component: PayrollPageComponent, canActivate: [permissionGuard], data: { permission: 'PAYROLL_RUN' } },
  { path: 'access', component: AccessPageComponent, canActivate: [permissionGuard], data: { permission: 'USER_ADMIN' } },
  { path: '**', redirectTo: 'overview' }
];
