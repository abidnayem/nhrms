import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Company, HrmsApiService, PayrollCycle, dateInputValue } from '../../hrms-api.service';

@Component({
  selector: 'app-payroll-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payroll.component.html',
  styleUrls: ['./payroll.component.scss'],
})
export class PayrollPageComponent implements OnInit {
  private readonly api = inject(HrmsApiService);

  cycles: PayrollCycle[] = [];
  companies: Company[] = [];
  loading = true;
  saving = false;
  showForm = false;
  error = '';
  readonly form = {
    companyId: '',
    periodStart: dateInputValue(new Date(new Date().getFullYear(), new Date().getMonth(), 1)),
    periodEnd: dateInputValue(new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0)),
    totalGross: 0,
    totalDeductions: 0,
  };

  get summary() {
    return [
      { label: 'Net payroll total', value: this.cycles.reduce((sum, cycle) => sum + cycle.totalNet, 0), format: 'amount' },
      { label: 'Total deductions', value: this.cycles.reduce((sum, cycle) => sum + cycle.totalDeductions, 0), format: 'amount' },
      { label: 'Payroll cycles', value: this.cycles.length, format: 'count' },
      { label: 'Draft cycles', value: this.cycles.filter(cycle => cycle.status === 'Draft').length, format: 'count' },
    ];
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    forkJoin({
      cycles: this.api.getPayrollCycles(),
      companies: this.api.getCompanies(),
    }).subscribe({
      next: data => {
        this.cycles = data.cycles.sort((a, b) => b.periodStart.localeCompare(a.periodStart));
        this.companies = data.companies;
        this.form.companyId = this.form.companyId || data.companies[0]?.id || '';
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load payroll data. Confirm that the HRMS API and database are running.';
        this.loading = false;
      },
    });
  }

  createCycle(): void {
    if (!this.form.companyId) {
      this.error = 'Create or configure a company before creating payroll cycles.';
      return;
    }
    if (this.form.periodStart >= this.form.periodEnd) {
      this.error = 'The cycle end date must be later than its start date.';
      return;
    }
    if (this.form.totalGross < 0 || this.form.totalDeductions < 0 || this.form.totalDeductions > this.form.totalGross) {
      this.error = 'Enter valid non-negative amounts, with deductions not exceeding gross pay.';
      return;
    }

    this.saving = true;
    this.error = '';
    this.api.createPayrollCycle(this.form).subscribe({
      next: () => {
        this.showForm = false;
        this.saving = false;
        this.form.totalGross = 0;
        this.form.totalDeductions = 0;
        this.load();
      },
      error: () => {
        this.error = 'Could not create the payroll cycle. Check its date range and amounts.';
        this.saving = false;
      },
    });
  }
}
