import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Company, Department, Designation, Employee, EmploymentType, HrmsApiService, dateInputValue } from '../../hrms-api.service';

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
  totalEmployees = 0;
  companies: Company[] = [];
  departments: Department[] = [];
  designations: Designation[] = [];
  employmentTypes: EmploymentType[] = [];
  loading = true;
  saving = false;
  showForm = false;
  error = '';
  searchTerm = '';
  statusFilter = '';
  departmentFilter = '';
  companyFilter = '';
  page = 1;
  readonly pageSize = 25;

  get pageCount(): number {
    return Math.max(1, Math.ceil(this.totalEmployees / this.pageSize));
  }
  readonly form = {
    companyId: '',
    firstName: '',
    lastName: '',
    email: '',
    dateOfJoining: dateInputValue(),
    departmentId: '',
    designationId: '',
    employmentTypeId: '',
  };

  get summary() {
    const currentMonth = new Date();
    const newHires = this.employees.filter(employee => {
      const joined = new Date(employee.dateOfJoining);
      return joined.getMonth() === currentMonth.getMonth() && joined.getFullYear() === currentMonth.getFullYear();
    }).length;
    return [
      { label: 'Total employees', value: this.totalEmployees },
      { label: 'New hires this month · page', value: newHires },
      { label: 'Active · page', value: this.employees.filter(employee => employee.isActive).length },
      { label: 'Inactive · page', value: this.employees.filter(employee => !employee.isActive).length },
    ];
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    forkJoin({
      employees: this.api.getEmployeeDirectory({
        search: this.searchTerm.trim(),
        companyId: this.companyFilter,
        departmentId: this.departmentFilter,
        isActive: this.statusFilter,
        page: this.page,
        pageSize: this.pageSize,
      }),
      companies: this.api.getCompanies(),
      departments: this.api.getDepartments(),
      designations: this.api.getDesignations(),
      employmentTypes: this.api.getEmploymentTypes(),
    }).subscribe({
      next: data => {
        this.employees = data.employees.items;
        this.totalEmployees = data.employees.totalCount;
        this.companies = data.companies;
        this.departments = data.departments;
        this.designations = data.designations;
        this.employmentTypes = data.employmentTypes;
        this.form.companyId = this.form.companyId || data.companies[0]?.id || '';
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load employee data. Confirm that the HRMS API and database are running.';
        this.loading = false;
      },
    });
  }

  search(): void {
    this.page = 1;
    this.load();
  }

  changePage(page: number): void {
    const lastPage = Math.max(1, Math.ceil(this.totalEmployees / this.pageSize));
    if (page < 1 || page > lastPage) return;
    this.page = page;
    this.load();
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
      employmentTypeId: this.form.employmentTypeId || null,
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
          employmentTypeId: '',
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

  employmentTypeName(id: string | null): string {
    return this.employmentTypes.find(type => type.id === id)?.name ?? '—';
  }
}
