import { Component, OnInit } from '@angular/core';
import { UsersService } from '../services/users.service';

@Component({
  selector: 'app-sidebar',
  standalone: false,
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
})
export class SidebarComponent implements OnInit {

  userid: string = '';
  constructor(public usersService: UsersService){}
  ngOnInit(): void {
    const user = JSON.parse(localStorage.getItem('profile') as string);
    if(user) this.userid = user.result?._id
  }
}
