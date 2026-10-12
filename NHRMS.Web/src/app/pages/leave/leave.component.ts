import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Employee, HrmsApiService, LeaveRequest, LeaveType, dateInputValue } from '../../hrms-api.service';

@Component({
  selector: 'app-leave-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './leave.component.html',
  styleUrls: ['./leave.component.scss'],
})
export class LeavePageComponent implements OnInit {
  private readonly api = inject(HrmsApiService);

  requests: LeaveRequest[] = [];
  employees: Employee[] = [];
  leaveTypes: LeaveType[] = [];
  loading = true;
  saving = false;
  reviewingId = '';
  showForm = false;
  error = '';
  readonly form = {
    employeeId: '',
    leaveTypeId: '',
    startDate: dateInputValue(),
    endDate: dateInputValue(),
    reason: '',
  };

  get summary() {
    const today = dateInputValue();
    return [
      { label: 'Pending', value: this.requests.filter(request => request.status === 'Pending').length },
      { label: 'Approved', value: this.requests.filter(request => request.status === 'Approved').length },
      { label: 'Rejected', value: this.requests.filter(request => request.status === 'Rejected').length },
      { label: 'Upcoming approved', value: this.requests.filter(request => request.status === 'Approved' && request.startDate.slice(0, 10) >= today).length },
    ];
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    forkJoin({
      requests: this.api.getLeaveRequests(),
      employees: this.api.getEmployees(),
      leaveTypes: this.api.getLeaveTypes(),
    }).subscribe({
      next: data => {
        this.requests = data.requests.sort((a, b) => b.startDate.localeCompare(a.startDate));
        this.employees = data.employees;
        this.leaveTypes = data.leaveTypes;
        this.form.employeeId = this.form.employeeId || data.employees.find(employee => employee.isActive)?.id || '';
        this.form.leaveTypeId = this.form.leaveTypeId || data.leaveTypes[0]?.id || '';
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load leave data. Confirm that the HRMS API and database are running.';
        this.loading = false;
      },
    });
  }

  createRequest(): void {
    if (!this.form.employeeId || !this.form.leaveTypeId) {
      this.error = 'Add an employee and configure a leave type before submitting a request.';
      return;
    }
    if (this.form.startDate > this.form.endDate) {
      this.error = 'The leave end date must be on or after the start date.';
      return;
    }

    this.saving = true;
    this.error = '';
    this.api.createLeaveRequest(this.form).subscribe({
      next: () => {
        this.showForm = false;
        this.saving = false;
        this.form.reason = '';
        this.load();
      },
      error: () => {
        this.error = 'Could not submit the leave request. Check its dates and selected employee.';
        this.saving = false;
      },
    });
  }

  review(request: LeaveRequest, status: 'Approved' | 'Rejected'): void {
    this.reviewingId = request.id;
    this.error = '';
    this.api.updateLeaveStatus(request.id, status).subscribe({
      next: updated => {
        this.requests = this.requests.map(item => item.id === updated.id ? updated : item);
        this.reviewingId = '';
      },
      error: () => {
        this.error = `Could not ${status.toLowerCase()} this request. Refresh and try again.`;
        this.reviewingId = '';
      },
    });
  }

  employeeName(id: string): string {
    const employee = this.employees.find(item => item.id === id);
    return employee ? `${employee.firstName} ${employee.lastName}` : 'Unknown employee';
  }

  leaveTypeName(id: string): string {
    return this.leaveTypes.find(type => type.id === id)?.name ?? 'Unknown leave type';
  }

  dayCount(request: LeaveRequest): number {
    const start = new Date(request.startDate);
    const end = new Date(request.endDate);
    return Math.round((end.getTime() - start.getTime()) / 86400000) + 1;
  }
}
