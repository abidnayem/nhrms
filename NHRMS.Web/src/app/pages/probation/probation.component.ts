import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../auth.service';
import { Employee, HrmsApiService, ProbationCase, dateInputValue } from '../../hrms-api.service';

@Component({
  selector: 'app-probation-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './probation.component.html',
  styleUrl: './probation.component.scss',
})
export class ProbationPageComponent implements OnInit {
  private readonly api = inject(HrmsApiService);
  private readonly auth = inject(AuthService);

  employees: Employee[] = [];
  cases: ProbationCase[] = [];
  selectedEmployeeId = '';
  selectedCaseId = '';
  loading = true;
  saving = false;
  error = '';
  decisionNotes = '';
  readonly caseForm = { startDate: dateInputValue(), plannedEndDate: '' };
  readonly reviewForm = { recommendation: 'Continue' as 'Continue' | 'Confirm' | 'Extend', reason: '', proposedEndDate: '' };

  get selectedCase(): ProbationCase | undefined {
    return this.cases.find(item => item.id === this.selectedCaseId);
  }

  get openCase(): ProbationCase | undefined {
    return this.cases.find(item => item.status !== 'Confirmed');
  }

  get canManage(): boolean { return this.auth.hasPermission('PROBATION_MANAGE'); }
  get canReview(): boolean { return this.auth.hasPermission('PROBATION_REVIEW'); }
  get canApprove(): boolean { return this.auth.hasPermission('PROBATION_APPROVE'); }

  hasPendingReview(item: ProbationCase): boolean {
    return item.reviews.some(review => review.status === 'Pending');
  }

  ngOnInit(): void {
    this.api.getEmployees().subscribe({
      next: employees => {
        this.employees = employees.filter(employee => employee.isActive);
        this.selectedEmployeeId = this.employees[0]?.id ?? '';
        this.loading = false;
        if (this.selectedEmployeeId) this.loadCases();
      },
      error: () => { this.error = 'Unable to load employees.'; this.loading = false; },
    });
  }

  loadCases(): void {
    if (!this.selectedEmployeeId) { this.cases = []; return; }
    this.loading = true;
    this.error = '';
    this.api.getProbationCases(this.selectedEmployeeId).subscribe({
      next: cases => {
        this.cases = cases;
        this.selectedCaseId = cases.find(item => item.status !== 'Confirmed')?.id ?? cases[0]?.id ?? '';
        this.loading = false;
      },
      error: () => { this.error = 'Unable to load probation records.'; this.loading = false; },
    });
  }

  startCase(): void {
    this.saving = true;
    this.error = '';
    this.api.startProbation(this.selectedEmployeeId, this.caseForm).subscribe({
      next: probationCase => {
        this.cases = [probationCase, ...this.cases];
        this.selectedCaseId = probationCase.id;
        this.caseForm.plannedEndDate = '';
        this.saving = false;
      },
      error: () => { this.error = 'Could not start probation. Check the dates and existing open case.'; this.saving = false; },
    });
  }

  recommend(): void {
    if (!this.selectedCase) return;
    this.saving = true;
    this.error = '';
    this.api.recommendProbationReview(this.selectedCase.id, {
      ...this.reviewForm,
      reason: this.reviewForm.reason.trim(),
      proposedEndDate: this.reviewForm.recommendation === 'Extend' ? this.reviewForm.proposedEndDate || null : null,
    }).subscribe({
      next: updated => { this.replaceCase(updated); this.reviewForm.reason = ''; this.reviewForm.proposedEndDate = ''; this.saving = false; },
      error: () => { this.error = 'Could not record the review recommendation.'; this.saving = false; },
    });
  }

  decide(reviewId: string, approve: boolean): void {
    if (!this.selectedCase) return;
    this.api.decideProbationReview(this.selectedCase.id, reviewId, approve, this.decisionNotes).subscribe({
      next: updated => { this.replaceCase(updated); this.decisionNotes = ''; },
      error: () => this.error = 'Could not record the approval decision. Reviewers cannot decide their own recommendation.',
    });
  }

  private replaceCase(updated: ProbationCase): void {
    this.cases = this.cases.map(item => item.id === updated.id ? updated : item);
  }
}
