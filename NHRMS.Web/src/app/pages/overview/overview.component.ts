import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-overview-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss'],
})
export class OverviewPageComponent {
  readonly metrics = [
    { label: 'Employees', value: '1,284', delta: '+12.4%', tone: 'blue' },
    { label: 'Active Users', value: '863', delta: '+8.1%', tone: 'green' },
    { label: 'Open Leave', value: '34', delta: '-3.2%', tone: 'amber' },
    { label: 'Payroll Run', value: '92%', delta: '+5.7%', tone: 'purple' }
  ];

  readonly actions = [
    'Add employee',
    'Create role',
    'Approve leave',
    'Run payroll'
  ];

  readonly users = [
    { name: 'Maya Rahman', role: 'HR Admin', status: 'Active', email: 'maya@acme.com' },
    { name: 'Daniel Kim', role: 'Payroll Manager', status: 'Active', email: 'daniel@acme.com' },
    { name: 'Aisha Niazi', role: 'Department Lead', status: 'Pending', email: 'aisha@acme.com' },
    { name: 'Omar Hassan', role: 'Operations Lead', status: 'Active', email: 'omar@acme.com' }
  ];

  readonly roles = [
    { name: 'System Administrator', members: 8, description: 'Full platform access and policy configuration.' },
    { name: 'HR Manager', members: 12, description: 'Manages employee lifecycle, leave, and approvals.' },
    { name: 'Payroll Officer', members: 5, description: 'Runs payroll cycles and resolves deductions.' },
    { name: 'Department Lead', members: 19, description: 'Oversees team performance and capacity planning.' }
  ];
}
