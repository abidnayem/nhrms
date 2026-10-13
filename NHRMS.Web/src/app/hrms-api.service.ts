import { HttpClient } from '@angular/common/http';
import { HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

const API_URL = 'http://localhost:5209/api';

export function dateInputValue(date = new Date()): string {
  return new Date(date.getTime() - date.getTimezoneOffset() * 60_000).toISOString().slice(0, 10);
}

export interface Company {
  id: string;
  name: string;
}

export interface Department {
  id: string;
  name: string;
  companyId: string;
}

export interface Designation {
  id: string;
  name: string;
  companyId: string;
}

export interface EmploymentType {
  id: string;
  name: string;
  companyId: string;
}

export interface Employee {
  id: string;
  companyId: string;
  departmentId: string | null;
  designationId: string | null;
  employmentTypeId: string | null;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string | null;
  dateOfJoining: string;
  isActive: boolean;
}

export interface EmployeeDirectory {
  items: Employee[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface OnboardingTask {
  id: string;
  employeeId: string;
  title: string;
  description: string | null;
  dueDate: string | null;
  isRequired: boolean;
  status: 'Pending' | 'Completed' | 'Waived';
  completedAtUtc: string | null;
}

export interface ProbationReview {
  id: string;
  recommendation: 'Continue' | 'Confirm' | 'Extend';
  reason: string;
  proposedEndDate: string | null;
  status: 'Pending' | 'Approved' | 'Rejected';
  reviewedBy: string;
  reviewedAtUtc: string;
  decidedBy: string | null;
  decidedAtUtc: string | null;
  decisionNotes: string | null;
}

export interface ProbationCase {
  id: string;
  employeeId: string;
  startDate: string;
  plannedEndDate: string;
  extendedEndDate: string | null;
  currentEndDate: string;
  status: 'InProgress' | 'Extended' | 'Confirmed';
  confirmedAtUtc: string | null;
  reviews: ProbationReview[];
}

export interface EmploymentChange {
  id: string;
  employeeId: string;
  changeType: 'CareerChange' | 'Transfer' | 'EmploymentTypeChange';
  status: 'Pending' | 'Approved' | 'Rejected' | 'Applied';
  effectiveDate: string;
  previousDepartmentId: string | null;
  newDepartmentId: string | null;
  previousDesignationId: string | null;
  newDesignationId: string | null;
  previousEmploymentTypeId: string | null;
  newEmploymentTypeId: string | null;
  reason: string;
  requestedBy: string;
  requestedAtUtc: string;
  decidedBy: string | null;
  decidedAtUtc: string | null;
  decisionNotes: string | null;
  appliedAtUtc: string | null;
}

export interface DisciplinaryCase {
  id: string;
  employeeId: string;
  caseType: 'ShowCause' | 'Warning' | 'Investigation' | 'Other';
  status: 'Open' | 'Resolved' | 'Dismissed';
  occurredOn: string;
  subject: string;
  summary: string;
  openedBy: string;
  openedAtUtc: string;
  decision: string | null;
  decidedBy: string | null;
  decidedAtUtc: string | null;
}

export interface Attendance {
  id: string;
  employeeId: string;
  attendanceDate: string;
  checkInUtc: string;
  checkOutUtc: string | null;
  hoursWorked: number;
  status: string;
}

export interface LeaveType {
  id: string;
  name: string;
}

export interface LeaveRequest {
  id: string;
  employeeId: string;
  leaveTypeId: string;
  startDate: string;
  endDate: string;
  status: string;
  reason: string | null;
}

export interface PayrollCycle {
  id: string;
  periodStart: string;
  periodEnd: string;
  totalGross: number;
  totalDeductions: number;
  totalNet: number;
  status: string;
}

export interface Role {
  id: string;
  name: string;
  code: string;
  description: string | null;
}

export interface UserAccount {
  id: string;
  employeeId: string;
  userName: string;
  email: string;
  isActive: boolean;
  roleIds: string[];
}

@Injectable({ providedIn: 'root' })
export class HrmsApiService {
  private readonly http = inject(HttpClient);

  getCompanies(): Observable<Company[]> {
    return this.http.get<Company[]>(`${API_URL}/companies`);
  }

  getDepartments(): Observable<Department[]> {
    return this.http.get<Department[]>(`${API_URL}/departments`);
  }

  getDesignations(): Observable<Designation[]> {
    return this.http.get<Designation[]>(`${API_URL}/designations`);
  }

  getEmploymentTypes(): Observable<EmploymentType[]> {
    return this.http.get<EmploymentType[]>(`${API_URL}/employmenttypes`);
  }

  getEmployees(): Observable<Employee[]> {
    return this.http.get<Employee[]>(`${API_URL}/employees`);
  }

  getEmployeeDirectory(query: {
    search: string;
    companyId: string;
    departmentId: string;
    isActive: string;
    page: number;
    pageSize: number;
  }): Observable<EmployeeDirectory> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) params = params.set('search', query.search);
    if (query.companyId) params = params.set('companyId', query.companyId);
    if (query.departmentId) params = params.set('departmentId', query.departmentId);
    if (query.isActive) params = params.set('isActive', query.isActive);
    return this.http.get<EmployeeDirectory>(`${API_URL}/employees/directory`, { params });
  }

  createEmployee(employee: {
    companyId: string;
    departmentId: string | null;
    designationId: string | null;
    employmentTypeId: string | null;
    firstName: string;
    lastName: string;
    email: string;
    dateOfJoining: string;
  }): Observable<Employee> {
    return this.http.post<Employee>(`${API_URL}/employees`, employee);
  }

  getOnboardingTasks(employeeId: string): Observable<OnboardingTask[]> {
    return this.http.get<OnboardingTask[]>(`${API_URL}/employees/${employeeId}/onboarding`);
  }

  createOnboardingTask(employeeId: string, task: {
    title: string;
    description: string;
    dueDate: string | null;
    isRequired: boolean;
  }): Observable<OnboardingTask> {
    return this.http.post<OnboardingTask>(`${API_URL}/employees/${employeeId}/onboarding`, task);
  }

  updateOnboardingTaskStatus(employeeId: string, taskId: string, status: 'Completed' | 'Waived'): Observable<OnboardingTask> {
    return this.http.put<OnboardingTask>(`${API_URL}/employees/${employeeId}/onboarding/${taskId}/status`, { status });
  }

  getProbationCases(employeeId: string): Observable<ProbationCase[]> {
    return this.http.get<ProbationCase[]>(`${API_URL}/employees/${employeeId}/probation`);
  }

  startProbation(employeeId: string, request: { startDate: string; plannedEndDate: string }): Observable<ProbationCase> {
    return this.http.post<ProbationCase>(`${API_URL}/employees/${employeeId}/probation`, request);
  }

  recommendProbationReview(caseId: string, request: {
    recommendation: 'Continue' | 'Confirm' | 'Extend'; reason: string; proposedEndDate: string | null;
  }): Observable<ProbationCase> {
    return this.http.post<ProbationCase>(`${API_URL}/probation/${caseId}/reviews`, request);
  }

  decideProbationReview(caseId: string, reviewId: string, approve: boolean, notes: string): Observable<ProbationCase> {
    return this.http.put<ProbationCase>(`${API_URL}/probation/${caseId}/reviews/${reviewId}/decision`, { approve, notes });
  }

  getEmploymentChanges(employeeId: string): Observable<EmploymentChange[]> {
    return this.http.get<EmploymentChange[]>(`${API_URL}/employees/${employeeId}/employment-changes`);
  }

  requestEmploymentChange(employeeId: string, request: {
    departmentId: string | null; designationId: string | null; employmentTypeId: string | null; effectiveDate: string; reason: string;
  }): Observable<EmploymentChange> {
    return this.http.post<EmploymentChange>(`${API_URL}/employees/${employeeId}/employment-changes`, request);
  }

  decideEmploymentChange(employeeId: string, changeId: string, approve: boolean, notes: string): Observable<EmploymentChange> {
    return this.http.put<EmploymentChange>(`${API_URL}/employees/${employeeId}/employment-changes/${changeId}/decision`, { approve, notes });
  }

  applyDueEmploymentChanges(employeeId: string): Observable<{ appliedCount: number }> {
    return this.http.post<{ appliedCount: number }>(`${API_URL}/employees/${employeeId}/employment-changes/apply-due`, {});
  }

  getDisciplinaryCases(employeeId: string): Observable<DisciplinaryCase[]> {
    return this.http.get<DisciplinaryCase[]>(`${API_URL}/employees/${employeeId}/disciplinary-cases`);
  }

  createDisciplinaryCase(employeeId: string, request: {
    caseType: DisciplinaryCase['caseType']; occurredOn: string; subject: string; summary: string;
  }): Observable<DisciplinaryCase> {
    return this.http.post<DisciplinaryCase>(`${API_URL}/employees/${employeeId}/disciplinary-cases`, request);
  }

  decideDisciplinaryCase(employeeId: string, caseId: string, uphold: boolean, decision: string): Observable<DisciplinaryCase> {
    return this.http.put<DisciplinaryCase>(`${API_URL}/employees/${employeeId}/disciplinary-cases/${caseId}/decision`, { uphold, decision });
  }

  getAttendance(): Observable<Attendance[]> {
    return this.http.get<Attendance[]>(`${API_URL}/attendance`);
  }

  createAttendance(attendance: {
    employeeId: string;
    attendanceDate: string;
    checkInUtc: string;
  }): Observable<Attendance> {
    return this.http.post<Attendance>(`${API_URL}/attendance`, attendance);
  }

  getLeaveRequests(): Observable<LeaveRequest[]> {
    return this.http.get<LeaveRequest[]>(`${API_URL}/leaverequests`);
  }

  getLeaveTypes(): Observable<LeaveType[]> {
    return this.http.get<LeaveType[]>(`${API_URL}/leavetypes`);
  }

  createLeaveRequest(request: {
    employeeId: string;
    leaveTypeId: string;
    startDate: string;
    endDate: string;
    reason: string;
  }): Observable<LeaveRequest> {
    return this.http.post<LeaveRequest>(`${API_URL}/leaverequests`, request);
  }

  updateLeaveStatus(id: string, status: 'Approved' | 'Rejected'): Observable<LeaveRequest> {
    return this.http.put<LeaveRequest>(`${API_URL}/leaverequests/${id}/status`, { status });
  }

  getPayrollCycles(): Observable<PayrollCycle[]> {
    return this.http.get<PayrollCycle[]>(`${API_URL}/payrollcycles`);
  }

  createPayrollCycle(cycle: {
    companyId: string;
    periodStart: string;
    periodEnd: string;
    totalGross: number;
    totalDeductions: number;
  }): Observable<PayrollCycle> {
    return this.http.post<PayrollCycle>(`${API_URL}/payrollcycles`, cycle);
  }

  getRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(`${API_URL}/roles`);
  }

  createRole(role: { name: string; code: string; description: string }): Observable<Role> {
    return this.http.post<Role>(`${API_URL}/roles`, role);
  }

  getUsers(): Observable<UserAccount[]> {
    return this.http.get<UserAccount[]>(`${API_URL}/users`);
  }

  createUser(user: {
    employeeId: string;
    userName: string;
    email: string;
    password: string;
    isActive: boolean;
  }): Observable<UserAccount> {
    return this.http.post<UserAccount>(`${API_URL}/users`, user);
  }

  assignRole(userId: string, roleId: string): Observable<UserAccount> {
    return this.http.post<UserAccount>(`${API_URL}/users/assign-role`, { userId, roleId });
  }
}
