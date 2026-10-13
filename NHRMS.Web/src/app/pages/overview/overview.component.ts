import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { Employee, HrmsApiService, LeaveRequest, PayrollCycle, Role, UserAccount } from '../../hrms-api.service';
import { AuthService } from '../../auth.service';

@Component({
  selector: 'app-overview-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss'],
})
export class OverviewPageComponent implements OnInit {
  private readonly api = inject(HrmsApiService);
  private readonly auth = inject(AuthService);

  employees: Employee[] = [];
  users: UserAccount[] = [];
  roles: Role[] = [];
  leaveRequests: LeaveRequest[] = [];
  payrollCycles: PayrollCycle[] = [];
  loading = true;
  error = '';
  readonly actions = [
    { label: 'Add employee', path: '/employees', permission: 'EMPLOYEE_WRITE' },
    { label: 'Create role', path: '/access', permission: 'USER_ADMIN' },
    { label: 'Review leave', path: '/leave', permission: 'LEAVE_MANAGE' },
    { label: 'Create payroll cycle', path: '/payroll', permission: 'PAYROLL_RUN' },
  ].filter(action => this.auth.hasPermission(action.permission));

  get metrics() {
    const latestCycle = [...this.payrollCycles].sort((a, b) => b.periodStart.localeCompare(a.periodStart))[0];
    return [
      { label: 'Employees', value: this.employees.filter(employee => employee.isActive).length.toLocaleString(), detail: `${this.employees.length} total records`, tone: 'blue' },
      { label: 'Active Users', value: this.users.filter(user => user.isActive).length.toLocaleString(), detail: 'Enabled accounts', tone: 'green' },
      { label: 'Open Leave', value: this.leaveRequests.filter(request => request.status === 'Pending').length.toLocaleString(), detail: 'Requests awaiting review', tone: 'amber' },
      { label: 'Payroll Run', value: latestCycle?.status ?? '—', detail: latestCycle ? `${new Date(latestCycle.periodStart).toLocaleDateString()} cycle` : 'No payroll cycles', tone: 'purple' },
    ];
  }

  get activeUsers() {
    return this.users
      .filter(user => user.isActive)
      .slice(0, 5)
      .map(user => {
        const employee = this.employees.find(item => item.id === user.employeeId);
        return {
          name: employee ? `${employee.firstName} ${employee.lastName}` : user.userName,
          email: user.email,
          role: this.roleNames(user),
          status: 'Active',
        };
      });
  }

  get roleCards() {
    return this.roles.slice(0, 5).map(role => ({
      name: role.name,
      members: this.users.filter(user => user.roleIds.includes(role.id)).length,
      description: role.description || role.code,
    }));
  }

  ngOnInit(): void {
    forkJoin({
      employees: this.api.getEmployees(),
      users: this.auth.hasPermission('USER_ADMIN') ? this.api.getUsers() : of([] as UserAccount[]),
      roles: this.auth.hasPermission('USER_ADMIN') ? this.api.getRoles() : of([] as Role[]),
      leaveRequests: this.auth.hasPermission('LEAVE_MANAGE') ? this.api.getLeaveRequests() : of([] as LeaveRequest[]),
      payrollCycles: this.auth.hasPermission('PAYROLL_RUN') ? this.api.getPayrollCycles() : of([] as PayrollCycle[]),
    }).subscribe({
      next: data => {
        this.employees = data.employees;
        this.users = data.users;
        this.roles = data.roles;
        this.leaveRequests = data.leaveRequests;
        this.payrollCycles = data.payrollCycles;
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load dashboard data. Confirm that the HRMS API and database are running.';
        this.loading = false;
      },
    });
  }

  roleNames(user: UserAccount): string {
    const names = user.roleIds
      .map(roleId => this.roles.find(role => role.id === roleId)?.name)
      .filter((name): name is string => Boolean(name));
    return names.length ? names.join(', ') : 'No role assigned';
  }
}
