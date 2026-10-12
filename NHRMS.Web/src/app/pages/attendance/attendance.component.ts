import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Attendance, Employee, HrmsApiService, dateInputValue } from '../../hrms-api.service';

@Component({
  selector: 'app-attendance-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './attendance.component.html',
  styleUrls: ['./attendance.component.scss'],
})
export class AttendancePageComponent implements OnInit {
  private readonly api = inject(HrmsApiService);

  employees: Employee[] = [];
  logs: Attendance[] = [];
  loading = true;
  saving = false;
  showForm = false;
  error = '';
  readonly today = dateInputValue();
  readonly form = {
    employeeId: '',
    attendanceDate: dateInputValue(),
    checkInTime: new Date().toTimeString().slice(0, 5),
  };

  get stats() {
    const activeEmployees = this.employees.filter(employee => employee.isActive).length;
    return [
      { label: 'Checked in today', value: this.logs.length },
      { label: 'Active employees', value: activeEmployees },
      { label: 'Not checked in', value: Math.max(0, activeEmployees - this.logs.length) },
      { label: 'Attendance coverage', value: `${activeEmployees ? Math.round(this.logs.length / activeEmployees * 100) : 0}%` },
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
      attendance: this.api.getAttendance(),
    }).subscribe({
      next: data => {
        this.employees = data.employees;
        this.logs = data.attendance
          .filter(entry => entry.attendanceDate.slice(0, 10) === this.today)
          .sort((a, b) => b.checkInUtc.localeCompare(a.checkInUtc));
        this.form.employeeId = this.form.employeeId || data.employees.find(employee => employee.isActive)?.id || '';
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load attendance. Confirm that the HRMS API and database are running.';
        this.loading = false;
      },
    });
  }

  recordCheckIn(): void {
    if (!this.form.employeeId) {
      this.error = 'Add an employee before recording attendance.';
      return;
    }

    const localCheckIn = new Date(`${this.form.attendanceDate}T${this.form.checkInTime}`);
    this.saving = true;
    this.error = '';
    this.api.createAttendance({
      employeeId: this.form.employeeId,
      attendanceDate: this.form.attendanceDate,
      checkInUtc: localCheckIn.toISOString(),
    }).subscribe({
      next: () => {
        this.showForm = false;
        this.saving = false;
        this.load();
      },
      error: () => {
        this.error = 'Could not record this check-in. Verify the employee and time.';
        this.saving = false;
      },
    });
  }

  employeeName(id: string): string {
    const employee = this.employees.find(item => item.id === id);
    return employee ? `${employee.firstName} ${employee.lastName}` : 'Unknown employee';
  }

  localTime(value: string): string {
    return new Date(value).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
}
