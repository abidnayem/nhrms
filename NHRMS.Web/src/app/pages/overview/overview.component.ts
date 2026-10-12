import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { Employee, HrmsApiService, LeaveRequest, PayrollCycle, Role, UserAccount } from '../../hrms-api.service';

@Component({
  selector: 'app-overview-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss'],
})
export class OverviewPageComponent implements OnInit {
  private readonly api = inject(HrmsApiService);

  employees: Employee[] = [];
  users: UserAccount[] = [];
  roles: Role[] = [];
  leaveRequests: LeaveRequest[] = [];
  payrollCycles: PayrollCycle[] = [];
  loading = true;
  error = '';
  readonly actions = [
    { label: 'Add employee', path: '/employees' },
    { label: 'Create role', path: '/access' },
    { label: 'Review leave', path: '/leave' },
    { label: 'Create payroll cycle', path: '/payroll' },
  ];

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
      users: this.api.getUsers(),
      roles: this.api.getRoles(),
      leaveRequests: this.api.getLeaveRequests(),
      payrollCycles: this.api.getPayrollCycles(),
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
