import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Employee, HrmsApiService, OnboardingTask, dateInputValue } from '../../hrms-api.service';

@Component({
  selector: 'app-onboarding-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './onboarding.component.html',
  styleUrl: './onboarding.component.scss',
})
export class OnboardingPageComponent implements OnInit {
  private readonly api = inject(HrmsApiService);

  employees: Employee[] = [];
  tasks: OnboardingTask[] = [];
  selectedEmployeeId = '';
  loading = true;
  saving = false;
  error = '';
  readonly form = { title: '', description: '', dueDate: dateInputValue(), isRequired: true };

  get selectedEmployee(): Employee | undefined {
    return this.employees.find(employee => employee.id === this.selectedEmployeeId);
  }

  ngOnInit(): void {
    this.api.getEmployees().subscribe({
      next: employees => {
        this.employees = employees;
        this.selectedEmployeeId = employees.find(employee => employee.isActive)?.id ?? '';
        this.loading = false;
        if (this.selectedEmployeeId) this.loadTasks();
      },
      error: () => {
        this.error = 'Unable to load employees. Confirm that the HRMS API and database are running.';
        this.loading = false;
      },
    });
  }

  loadTasks(): void {
    if (!this.selectedEmployeeId) {
      this.tasks = [];
      return;
    }
    this.loading = true;
    this.error = '';
    this.api.getOnboardingTasks(this.selectedEmployeeId).subscribe({
      next: tasks => { this.tasks = tasks; this.loading = false; },
      error: () => { this.error = 'Unable to load onboarding tasks.'; this.loading = false; },
    });
  }

  createTask(): void {
    if (!this.selectedEmployeeId || !this.form.title.trim()) return;
    this.saving = true;
    this.error = '';
    this.api.createOnboardingTask(this.selectedEmployeeId, {
      ...this.form,
      dueDate: this.form.dueDate || null,
    }).subscribe({
      next: () => {
        Object.assign(this.form, { title: '', description: '', dueDate: dateInputValue(), isRequired: true });
        this.saving = false;
        this.loadTasks();
      },
      error: () => { this.error = 'Could not add the onboarding task.'; this.saving = false; },
    });
  }

  updateStatus(task: OnboardingTask, status: 'Completed' | 'Waived'): void {
    this.api.updateOnboardingTaskStatus(this.selectedEmployeeId, task.id, status).subscribe({
      next: updated => this.tasks = this.tasks.map(item => item.id === updated.id ? updated : item),
      error: () => this.error = 'Could not update the task status.',
    });
  }
}
