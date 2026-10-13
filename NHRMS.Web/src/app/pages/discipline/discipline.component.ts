import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../auth.service';
import { DisciplinaryCase, Employee, HrmsApiService, dateInputValue } from '../../hrms-api.service';

@Component({
  selector: 'app-discipline-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './discipline.component.html',
  styleUrl: './discipline.component.scss',
})
export class DisciplinePageComponent implements OnInit {
  private readonly api = inject(HrmsApiService);
  private readonly auth = inject(AuthService);
  employees: Employee[] = [];
  cases: DisciplinaryCase[] = [];
  selectedEmployeeId = '';
  loading = true;
  saving = false;
  error = '';
  readonly form = { caseType: 'Investigation' as DisciplinaryCase['caseType'], occurredOn: dateInputValue(), subject: '', summary: '' };
  readonly decisions: Record<string, string> = {};

  get canManage(): boolean { return this.auth.hasPermission('DISCIPLINE_MANAGE'); }
  get canDecide(): boolean { return this.auth.hasPermission('DISCIPLINE_DECIDE'); }

  ngOnInit(): void {
    this.api.getEmployees().subscribe({
      next: employees => { this.employees = employees; this.selectedEmployeeId = employees[0]?.id ?? ''; this.loading = false; this.loadCases(); },
      error: () => { this.error = 'Unable to load employee directory.'; this.loading = false; },
    });
  }

  loadCases(): void {
    if (!this.selectedEmployeeId) { this.cases = []; return; }
    this.loading = true;
    this.api.getDisciplinaryCases(this.selectedEmployeeId).subscribe({
      next: cases => { this.cases = cases; this.loading = false; },
      error: () => { this.error = 'Unable to load restricted employee-relations records.'; this.loading = false; },
    });
  }

  create(): void {
    this.saving = true;
    this.api.createDisciplinaryCase(this.selectedEmployeeId, { ...this.form, subject: this.form.subject.trim(), summary: this.form.summary.trim() }).subscribe({
      next: item => { this.cases = [item, ...this.cases]; this.form.subject = ''; this.form.summary = ''; this.saving = false; },
      error: () => { this.error = 'Could not open the case. Verify the date and required details.'; this.saving = false; },
    });
  }

  decide(item: DisciplinaryCase, uphold: boolean): void {
    const decision = this.decisions[item.id]?.trim() ?? '';
    this.api.decideDisciplinaryCase(this.selectedEmployeeId, item.id, uphold, decision).subscribe({
      next: updated => { this.cases = this.cases.map(existing => existing.id === updated.id ? updated : existing); this.decisions[item.id] = ''; },
      error: () => this.error = 'Could not record the decision. A different authorized officer must decide the case.',
    });
  }
}
