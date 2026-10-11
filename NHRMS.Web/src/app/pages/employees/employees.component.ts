import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-employees-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './employees.component.html',
  styleUrls: ['./employees.component.scss'],
})
export class EmployeesPageComponent {
  readonly summary = [
    { label: 'Total employees', value: '1,284' },
    { label: 'New hires', value: '46' },
    { label: 'On leave', value: '34' },
    { label: 'Attrition risk', value: '3.1%' }
  ];

  readonly employees = [
    { name: 'Maya Rahman', department: 'People Ops', role: 'HR Admin', status: 'Active' },
    { name: 'Daniel Kim', department: 'Finance', role: 'Payroll Manager', status: 'Active' },
    { name: 'Aisha Niazi', department: 'Engineering', role: 'Department Lead', status: 'Pending' },
    { name: 'Omar Hassan', department: 'Operations', role: 'Branch Head', status: 'Active' },
    { name: 'Leah Brown', department: 'Support', role: 'Team Lead', status: 'Active' }
  ];
}
