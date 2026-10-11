import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-attendance-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './attendance.component.html',
  styleUrls: ['./attendance.component.scss'],
})
export class AttendancePageComponent {
  readonly stats = [
    { label: 'Present today', value: '892' },
    { label: 'Late arrivals', value: '23' },
    { label: 'Absent', value: '41' },
    { label: 'Approval rate', value: '96%' }
  ];

  readonly logs = [
    { employee: 'Maya Rahman', time: '08:58', status: 'On time' },
    { employee: 'Daniel Kim', time: '09:14', status: 'Late' },
    { employee: 'Aisha Niazi', time: '09:02', status: 'On time' },
    { employee: 'Leah Brown', time: '08:49', status: 'On time' }
  ];
}
