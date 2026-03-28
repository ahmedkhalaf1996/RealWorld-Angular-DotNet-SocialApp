import { Component, OnDestroy, OnInit } from '@angular/core';
import { filter, Subject, takeUntil } from 'rxjs';
import { UsersService } from '../services/users.service';
import { Router } from '@angular/router';
import { resolveImageSrc } from '../utils/image';

@Component({
  selector: 'app-rightbar',
  standalone: false,
  templateUrl: './rightbar.component.html',
  styleUrl: './rightbar.component.css',
})
export class RightbarComponent implements OnInit, OnDestroy {

  sugUsersList: any = []
  private destroy$ = new  Subject<void>();
  constructor(public usersService: UsersService, private route:Router){}

  resolveImage = resolveImageSrc;

  ngOnInit(): void {
    this.loadSugtestions();

    this.usersService.user$
        .pipe(
          filter((user)=> !! user && !!user._id),
          takeUntil(this.destroy$)
        )
    .subscribe(()=>{
      this.loadSugtestions();
    })
  }

  MoveToUserPage(id:any){
    this.route.navigate(['/profile/', id])
  }

  followUser(event: Event, user: any){
    event.stopPropagation();
    if(!user?._id){
      return;
    }
    this.usersService.following(String(user._id)).subscribe({
      next:()=>{
        this.sugUsersList = this.sugUsersList.filter((u: any)=> u?._id !== user._id);
      }
    })
  }

  trackByUserID(_index: number, user: any) {
    return user?._id ?? _index;
  }

  private loadSugtestions(){
    if(this.usersService.isAuthenticated && this.usersService.UserServiceData?._id != null){
      this.usersService.getSugUser(String(this.usersService.UserServiceData?._id)).subscribe({
        next: ({users}) => {
          console.log("%%%%%%%%%%%%%%%%%%%%55555555",users);
          this.sugUsersList = users
        }
      })
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }
}
