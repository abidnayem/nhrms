import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-access-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './access.component.html',
  styleUrls: ['./access.component.scss'],
})
export class AccessPageComponent {
  readonly roles = [
    { name: 'System Administrator', members: 8, permissions: ['Manage users', 'Policy config', 'Audit logs'] },
    { name: 'HR Manager', members: 12, permissions: ['Hire team', 'Approve leave', 'Manage positions'] },
    { name: 'Payroll Officer', members: 5, permissions: ['Run payroll', 'Review adjustments', 'Export payslips'] },
    { name: 'Department Lead', members: 19, permissions: ['Review attendance', 'Approve requests', 'Team planning'] }
  ];
}
