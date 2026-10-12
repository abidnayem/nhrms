import { HttpClient } from '@angular/common/http';
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

export interface Employee {
  id: string;
  companyId: string;
  departmentId: string | null;
  designationId: string | null;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string | null;
  dateOfJoining: string;
  isActive: boolean;
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

  getEmployees(): Observable<Employee[]> {
    return this.http.get<Employee[]>(`${API_URL}/employees`);
  }

  createEmployee(employee: {
    companyId: string;
    departmentId: string | null;
    designationId: string | null;
    firstName: string;
    lastName: string;
    email: string;
    dateOfJoining: string;
  }): Observable<Employee> {
    return this.http.post<Employee>(`${API_URL}/employees`, employee);
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
