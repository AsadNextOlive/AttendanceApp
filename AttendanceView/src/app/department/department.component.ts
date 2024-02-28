import { Component } from '@angular/core';
import { DepartmentService } from '../shared/department.service';
import { NgForm } from '@angular/forms';
import { Department } from '../shared/department.model';

@Component({
  selector: 'app-department',
  templateUrl: './department.component.html',
  styleUrl: './department.component.css'
})
export class DepartmentComponent {
  //Declaring Form by default for form validation
  formSubmitten: boolean=false

  //Adding Department Service to fetch get, post, put etc methods and trigger it on Submit Button
  constructor(public service: DepartmentService) { }

  ngOnInit(): void{
    this.service.refreshList();
  }

  onSubmit(form: NgForm){
    if (form.valid) {
      if (this.service.formData.DepartmentId == 0) {
        this.insertRecord(form)
      }
    }
  }

  //Insert Record into Department
  insertRecord(form: NgForm){
    this.service.postDepartment()
    .subscribe({
      next: res => {
        console.log(res);
        this.service.list = res as Department[];
        this.service.resetForm(form);
        this.service.refreshList();
      },
      error: err => {console.log(err)}
    })
  }
}
