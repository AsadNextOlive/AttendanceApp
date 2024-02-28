import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './register/register.component';
import { VerifyComponent } from './verify/verify.component';
import { DepartmentComponent } from './department/department.component';
import { MarkAttendanceComponent } from './mark-attendance/mark-attendance.component';
import { ViewAttendanceComponent } from './view-attendance/view-attendance.component';
import { LoginComponent } from './login/login.component';

const routes: Routes = [
  {title: 'Home', path: "", component: HomeComponent},
  {title: 'Register', path: 'register', component: RegisterComponent},
  {title: 'Verify', path: 'verify', component: VerifyComponent},
  {title: 'Department', path: 'department', component: DepartmentComponent},
  {title: 'Mark Attendance', path: 'mark-attendance', component: MarkAttendanceComponent},
  {title: 'View Attendance', path: 'view-attendance', component: ViewAttendanceComponent},
  {title: 'Login', path: 'login', component: LoginComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
