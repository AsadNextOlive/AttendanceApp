import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Department } from './department.model';
import { NgForm } from '@angular/forms';

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {

  //Initializing an array to get the list
  list: Department[] = [];

  //Declaration by default for form
  formSubmitted: boolean = false;

  //calling apiBaseUrl from the environment.ts
  url: string = environment.apiBaseUrl + '/Department'

  //Initializing an array Department from Model
  formData: Department = new Department();

  //Adding Http for API
  constructor(private http: HttpClient) { }

  //GET: Fetch data from the database
  refreshList(){
    this.http.get(this.url)
    .subscribe({
      next: res => {
        console.log(res);
        this.list = res as Department[];
      },
      error: err => {console.log(err)}
    })
  }

  //POST: Insert Department into the Database
  postDepartment(){
    return this.http.post(this.url, this.formData)
  }

  //Reset Form
  resetForm(form: NgForm){
    form.form.reset();
    this.formData = new Department();
    this.formSubmitted = false;
  }
}
