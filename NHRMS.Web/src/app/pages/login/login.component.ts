import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';
import { AuthService } from '../../auth.service';
import { dateInputValue } from '../../hrms-api.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  mode: 'login' | 'bootstrap' = 'login';
  busy = false;
  error = '';
  readonly loginForm = { userName: '', password: '' };
  readonly bootstrapForm = {
    key: '', companyCode: 'NHRMS', firstName: '', lastName: '', email: '', userName: '', password: '', dateOfJoining: dateInputValue(),
  };

  login(): void {
    this.busy = true;
    this.error = '';
    this.auth.login(this.loginForm.userName, this.loginForm.password).subscribe({
      next: () => this.finishLogin(),
      error: () => { this.error = 'Sign in failed. Check your credentials and try again.'; this.busy = false; },
    });
  }

  bootstrap(): void {
    const { key, ...request } = this.bootstrapForm;
    this.busy = true;
    this.error = '';
    this.auth.bootstrap(key, request).pipe(finalize(() => this.busy = false)).subscribe({
      next: () => this.finishLogin(),
      error: error => { this.error = this.bootstrapError(error); },
    });
  }

  private bootstrapError(error: unknown): string {
    if (!(error instanceof HttpErrorResponse))
      return 'Setup failed unexpectedly. Check the API and try again.';
    if (error.status === 0)
      return 'The API did not respond. Check that the API is running and reachable from this browser.';
    if (error.status === 400)
      return 'Some setup details are invalid. Check the company code, email, joining date, and password.';
    if (error.status === 401)
      return 'The bootstrap key is incorrect.';
    if (error.status === 404)
      return 'First-time setup is disabled because no bootstrap key is configured on the API.';
    if (error.status === 409)
      return 'First-time setup is already complete, or the HR manager role is not available.';
    if (error.status === 429)
      return 'Too many setup attempts. Wait one minute before trying again.';
    if (error.status === 503)
      return 'The database is unavailable. Check the SQL Server connection and encryption settings, then retry.';
    return `Setup failed (HTTP ${error.status}). Check the API logs before trying again.`;
  }

  private finishLogin(): void {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    const destination = returnUrl?.startsWith('/') && !returnUrl.startsWith('//') ? returnUrl : '/overview';
    void this.router.navigateByUrl(destination);
    this.busy = false;
  }
}
