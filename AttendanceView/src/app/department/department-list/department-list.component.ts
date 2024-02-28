import { Component } from '@angular/core';
import { DepartmentService } from '../../shared/department.service';

@Component({
  selector: 'app-department-list',
  templateUrl: './department-list.component.html',
  styleUrl: './department-list.component.css'
})
export class DepartmentListComponent {

  constructor(public service: DepartmentService){}
  
  ngOnInIt(): void{
    // this.service.refreshList();
  }
}
