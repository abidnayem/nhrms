import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../auth.service';
import { Department, Designation, Employee, EmploymentChange, EmploymentType, HrmsApiService, dateInputValue } from '../../hrms-api.service';

@Component({
  selector: 'app-employment-changes-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './employment-changes.component.html',
  styleUrl: './employment-changes.component.scss',
})
export class EmploymentChangesPageComponent implements OnInit {
  private readonly api = inject(HrmsApiService);
  private readonly auth = inject(AuthService);
  employees: Employee[] = [];
  departments: Department[] = [];
  designations: Designation[] = [];
  employmentTypes: EmploymentType[] = [];
  changes: EmploymentChange[] = [];
  selectedEmployeeId = '';
  loading = true;
  saving = false;
  error = '';
  notes = '';
  readonly form = { departmentId: '', designationId: '', employmentTypeId: '', effectiveDate: dateInputValue(), reason: '' };

  get employee(): Employee | undefined { return this.employees.find(x => x.id === this.selectedEmployeeId); }
  get companyDepartments(): Department[] { return this.departments.filter(x => x.companyId === this.employee?.companyId); }
  get companyDesignations(): Designation[] { return this.designations.filter(x => x.companyId === this.employee?.companyId); }
  get companyEmploymentTypes(): EmploymentType[] { return this.employmentTypes.filter(x => x.companyId === this.employee?.companyId); }
  get canManage(): boolean { return this.auth.hasPermission('EMPLOYMENT_CHANGE_MANAGE'); }
  get canApprove(): boolean { return this.auth.hasPermission('EMPLOYMENT_CHANGE_APPROVE'); }

  nameOf(id: string | null, kind: 'department' | 'designation' | 'employmentType'): string {
    if (!id) return '—';
    const list = kind === 'department' ? this.departments : kind === 'designation' ? this.designations : this.employmentTypes;
    return list.find(item => item.id === id)?.name ?? 'Unavailable';
  }

  ngOnInit(): void {
    this.api.getEmployees().subscribe({
      next: employees => {
        this.employees = employees;
        this.selectedEmployeeId = employees[0]?.id ?? '';
        this.api.getDepartments().subscribe(items => this.departments = items);
        this.api.getDesignations().subscribe(items => this.designations = items);
        this.api.getEmploymentTypes().subscribe(items => this.employmentTypes = items);
        this.loading = false;
        this.selectEmployee();
      },
      error: () => { this.error = 'Unable to load employees.'; this.loading = false; },
    });
  }

  selectEmployee(): void {
    const employee = this.employee;
    this.form.departmentId = employee?.departmentId ?? '';
    this.form.designationId = employee?.designationId ?? '';
    this.form.employmentTypeId = employee?.employmentTypeId ?? '';
    this.changes = [];
    if (!employee) return;
    this.loading = true;
    this.api.getEmploymentChanges(employee.id).subscribe({
      next: items => { this.changes = items; this.loading = false; },
      error: () => { this.error = 'Unable to load employment history.'; this.loading = false; },
    });
  }

  submit(): void {
    if (!this.employee) return;
    this.saving = true;
    this.error = '';
    this.api.requestEmploymentChange(this.employee.id, {
      departmentId: this.form.departmentId || null,
      designationId: this.form.designationId || null,
      employmentTypeId: this.form.employmentTypeId || null,
      effectiveDate: this.form.effectiveDate,
      reason: this.form.reason.trim(),
    }).subscribe({
      next: change => { this.changes = [change, ...this.changes]; this.form.reason = ''; this.saving = false; },
      error: () => { this.error = 'Could not submit the change. Check that at least one assignment changes and no other request is open.'; this.saving = false; },
    });
  }

  decide(change: EmploymentChange, approve: boolean): void {
    if (!this.employee) return;
    this.api.decideEmploymentChange(this.employee.id, change.id, approve, this.notes).subscribe({
      next: updated => { this.replace(updated); this.notes = ''; },
      error: () => this.error = 'Could not record the decision. Requesters cannot approve their own changes.',
    });
  }

  applyDue(): void {
    if (!this.employee) return;
    this.api.applyDueEmploymentChanges(this.employee.id).subscribe({
      next: () => this.selectEmployee(),
      error: () => this.error = 'Could not apply due approved changes.',
    });
  }

  private replace(updated: EmploymentChange): void {
    this.changes = this.changes.map(item => item.id === updated.id ? updated : item);
    if (updated.status === 'Applied') this.selectEmployee();
  }
}
