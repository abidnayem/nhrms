import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Employee, HrmsApiService, Role, UserAccount } from '../../hrms-api.service';

@Component({
  selector: 'app-access-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './access.component.html',
  styleUrls: ['./access.component.scss'],
})
export class AccessPageComponent implements OnInit {
  private readonly api = inject(HrmsApiService);

  roles: Role[] = [];
  users: UserAccount[] = [];
  employees: Employee[] = [];
  loading = true;
  saving = false;
  showForm = false;
  showUserForm = false;
  error = '';
  readonly form = { name: '', code: '', description: '' };
  readonly userForm = { employeeId: '', userName: '', email: '', password: '' };
  readonly roleAssignments: Record<string, string> = {};

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    forkJoin({
      roles: this.api.getRoles(),
      users: this.api.getUsers(),
      employees: this.api.getEmployees(),
    }).subscribe({
      next: data => {
        this.roles = data.roles;
        this.users = data.users;
        this.employees = data.employees;
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load access data. Confirm that the HRMS API and database are running.';
        this.loading = false;
      },
    });
  }

  createRole(): void {
    this.saving = true;
    this.error = '';
    this.api.createRole({
      name: this.form.name,
      code: this.form.code,
      description: this.form.description,
    }).subscribe({
      next: role => {
        this.roles = [...this.roles, role];
        this.form.name = '';
        this.form.code = '';
        this.form.description = '';
        this.showForm = false;
        this.saving = false;
      },
      error: () => {
        this.error = 'Could not create this role. Role codes must be unique.';
        this.saving = false;
      },
    });
  }

  createUser(): void {
    this.saving = true;
    this.error = '';
    this.api.createUser({ ...this.userForm, isActive: true }).subscribe({
      next: user => {
        this.users = [...this.users, user];
        this.userForm.employeeId = '';
        this.userForm.userName = '';
        this.userForm.email = '';
        this.userForm.password = '';
        this.showUserForm = false;
        this.saving = false;
      },
      error: () => {
        this.error = 'Could not create this account. Check the employee, username, email, and password.';
        this.saving = false;
      },
    });
  }

  assignRole(user: UserAccount): void {
    const roleId = this.roleAssignments[user.id];
    if (!roleId) return;

    this.saving = true;
    this.error = '';
    this.api.assignRole(user.id, roleId).subscribe({
      next: updatedUser => {
        this.users = this.users.map(item => item.id === updatedUser.id ? updatedUser : item);
        this.roleAssignments[user.id] = '';
        this.saving = false;
      },
      error: () => {
        this.error = 'Could not assign this role. It may already be assigned to the user.';
        this.saving = false;
      },
    });
  }

  availableRoles(user: UserAccount): Role[] {
    return this.roles.filter(role => !user.roleIds.includes(role.id));
  }

  hasAccount(employeeId: string): boolean {
    return this.users.some(user => user.employeeId === employeeId);
  }

  memberCount(roleId: string): number {
    return this.users.filter(user => user.roleIds.includes(roleId)).length;
  }

  employeeName(id: string): string {
    const employee = this.employees.find(item => item.id === id);
    return employee ? `${employee.firstName} ${employee.lastName}` : 'Unknown employee';
  }

  roleNames(user: UserAccount): string {
    const names = user.roleIds
      .map(roleId => this.roles.find(role => role.id === roleId)?.name)
      .filter((name): name is string => Boolean(name));
    return names.length ? names.join(', ') : 'No roles assigned';
  }
}
