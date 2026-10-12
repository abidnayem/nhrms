import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Company, Department, Designation, Employee, HrmsApiService, dateInputValue } from '../../hrms-api.service';

@Component({
  selector: 'app-employees-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './employees.component.html',
  styleUrls: ['./employees.component.scss'],
})
export class EmployeesPageComponent implements OnInit {
  private readonly api = inject(HrmsApiService);

  employees: Employee[] = [];
  companies: Company[] = [];
  departments: Department[] = [];
  designations: Designation[] = [];
  loading = true;
  saving = false;
  showForm = false;
  error = '';
  readonly form = {
    companyId: '',
    firstName: '',
    lastName: '',
    email: '',
    dateOfJoining: dateInputValue(),
    departmentId: '',
    designationId: '',
  };

  get summary() {
    const currentMonth = new Date();
    const newHires = this.employees.filter(employee => {
      const joined = new Date(employee.dateOfJoining);
      return joined.getMonth() === currentMonth.getMonth() && joined.getFullYear() === currentMonth.getFullYear();
    }).length;
    return [
      { label: 'Total employees', value: this.employees.length },
      { label: 'New hires this month', value: newHires },
      { label: 'Active employees', value: this.employees.filter(employee => employee.isActive).length },
      { label: 'Inactive employees', value: this.employees.filter(employee => !employee.isActive).length },
    ];
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    forkJoin({
      employees: this.api.getEmployees(),
      companies: this.api.getCompanies(),
      departments: this.api.getDepartments(),
      designations: this.api.getDesignations(),
    }).subscribe({
      next: data => {
        this.employees = data.employees;
        this.companies = data.companies;
        this.departments = data.departments;
        this.designations = data.designations;
        this.form.companyId = this.form.companyId || data.companies[0]?.id || '';
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load employee data. Confirm that the HRMS API and database are running.';
        this.loading = false;
      },
    });
  }

  createEmployee(): void {
    if (!this.form.companyId) {
      this.error = 'Create or configure a company before adding employees.';
      return;
    }

    this.saving = true;
    this.error = '';
    this.api.createEmployee({
      ...this.form,
      departmentId: this.form.departmentId || null,
      designationId: this.form.designationId || null,
    }).subscribe({
      next: () => {
        this.showForm = false;
        this.saving = false;
        Object.assign(this.form, {
          firstName: '',
          lastName: '',
          email: '',
          dateOfJoining: dateInputValue(),
          departmentId: '',
          designationId: '',
        });
        this.load();
      },
      error: () => {
        this.error = 'Could not save the employee. Check the employee code and required fields.';
        this.saving = false;
      },
    });
  }

  displayName(employee: Employee): string {
    return `${employee.firstName} ${employee.lastName}`;
  }

  departmentName(id: string | null): string {
    return this.departments.find(department => department.id === id)?.name ?? '—';
  }

  designationName(id: string | null): string {
    return this.designations.find(designation => designation.id === id)?.name ?? '—';
  }
}
