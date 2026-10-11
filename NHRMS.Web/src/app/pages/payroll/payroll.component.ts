import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-payroll-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payroll.component.html',
  styleUrls: ['./payroll.component.scss'],
})
export class PayrollPageComponent {
  readonly summary = [
    { label: 'Net payroll', value: '$836k' },
    { label: 'Deductions', value: '$52k' },
    { label: 'Employees paid', value: '1,284' },
    { label: 'Approval score', value: '98%' }
  ];

  readonly cycles = [
    { name: 'Monthly cycle', status: 'Closed', amount: '$412,000' },
    { name: 'Bonus review', status: 'Reviewing', amount: '$94,500' },
    { name: 'Compliance audit', status: 'Pending', amount: '$27,300' }
  ];
}
