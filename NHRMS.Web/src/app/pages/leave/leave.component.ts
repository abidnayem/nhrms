import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-leave-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './leave.component.html',
  styleUrls: ['./leave.component.scss'],
})
export class LeavePageComponent {
  readonly summary = [
    { label: 'Pending', value: '18' },
    { label: 'Approved', value: '62' },
    { label: 'Upcoming', value: '11' },
    { label: 'Balance', value: '24d' }
  ];

  readonly requests = [
    { employee: 'Maya Rahman', type: 'Annual leave', days: '5 days', status: 'Pending' },
    { employee: 'Daniel Kim', type: 'Sick leave', days: '2 days', status: 'Approved' },
    { employee: 'Aisha Niazi', type: 'Parental leave', days: '10 days', status: 'Pending' },
    { employee: 'Leah Brown', type: 'Emergency leave', days: '1 day', status: 'Approved' }
  ];
}
