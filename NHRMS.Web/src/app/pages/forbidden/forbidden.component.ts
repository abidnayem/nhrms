import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../auth.service';

@Component({
  selector: 'app-forbidden-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="forbidden-card">
      <p class="eyebrow">Access restricted</p>
      <h1>This account does not have access to that page.</h1>
      <p>Ask an administrator to assign the permission required for this area.</p>
      <button type="button" (click)="logout()">Sign out</button>
    </section>
  `,
  styles: [`
    :host { display: grid; min-height: 70vh; place-items: center; }
    .forbidden-card { max-width: 36rem; padding: 2rem; border: 1px solid #e4e7ec; border-radius: 18px; background: white; }
    .eyebrow { color: #64748b; font-size: 0.76rem; letter-spacing: 0.1em; text-transform: uppercase; }
    h1 { color: #101828; font-size: 1.65rem; }
    p { color: #667085; }
    button { padding: 0.7rem 1rem; border: 0; border-radius: 9px; background: #2563eb; color: white; cursor: pointer; }
  `],
})
export class ForbiddenPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}
