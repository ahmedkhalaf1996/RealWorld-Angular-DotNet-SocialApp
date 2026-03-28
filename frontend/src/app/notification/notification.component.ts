import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { Notification } from '../types/Notification';
import { filter, Subject, Subscription, takeUntil } from 'rxjs';
import { NotificationService } from '../services/notification.service';
import { UsersService } from '../services/users.service';
import { RealtimeNotifyService } from '../services/realtime-notify.service';
import { Router } from '@angular/router';
import { resolveImageSrc } from '../utils/image';
import { User } from '../types/User';

@Component({
  selector: 'app-notification',
  standalone: false,
  templateUrl: './notification.component.html',
  styleUrl: './notification.component.css',
})
export class NotificationComponent implements OnInit, OnDestroy{

  notify: Notification[] = [];

  private notificationSubscription!: Subscription;
  private destroy$ = new Subject<void>()
  private markInFlight = false;

  constructor(
    public notifiyService: NotificationService,
    public userService: UsersService,
    private signalRService: RealtimeNotifyService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ){}

  resolveImage = resolveImageSrc

  ngOnInit(): void {
    console.log('ng clled')
    this.userService.user$
        .pipe(
            filter((user): user is User => !!user && !!user._id),
            takeUntil(this.destroy$)
      )
      .subscribe(()=>{
        this.signalRService.connect();
        this.notifiyService.GetUserNotification().subscribe({
          next:()=>{
            this.notify = this.notifiyService.NotifyServiceData;
            console.log('notify', this.notify)
            this.cdr.markForCheck();
            this.MarkNotifiyAsReaded();
          }
        })
      })
      // call realtime subscription
  this.notificationSubscription = this.signalRService.reciveNotification().subscribe((notification: any)=>{
      const converted = {
    ...notification,
    createdAt: notification.createdAt?.seconds
      ? new Date(notification.createdAt.seconds * 1000)
      : notification.createdAt
  };
    this.notify.unshift(converted);
    console.log("not", notification)
    this.notifiyService.NotifyServiceData = [...this.notify];
    this.cdr.markForCheck();
    this.MarkNotifiyAsReaded();
   })

  }




  MarkNotifiyAsReaded(){
    if(this.markInFlight){
      return;
    }

    const hasUnreded = this.notify.some((noty)=> noty?.isreded === false);
    if(!hasUnreded){
      this.notifiyService.markAllAsReadLocal();
      return;
    }

    this.markInFlight = true;
    this.notifiyService.markAllAsReadLocal();
    this.notifiyService.MarkUserNotificationAsReaded().subscribe({
      next:()=>{
        this.notify = this.notifiyService.NotifyServiceData;
        this.notify.forEach(notify =>{
          notify.isreded = true;
        })

        this.notifiyService.markAllAsReadLocal();
        this.cdr.markForCheck();
        this.markInFlight = false
      },
      error:()=>{
        this.markInFlight = false;
      }
    })
  }

  MoveToSource( notification: Notification ) {
    if(notification.deatils?.includes("Post")){
      this.router.navigate(['/EditShowPost' + `/${notification?.targetid}`])
    } else if(notification.deatils?.includes("Following")){
      this.router.navigate(['/profile' + `/${notification?.targetid}`])
    }
  }

  ngOnDestroy(): void {
    this.notificationSubscription.unsubscribe();
    this.destroy$.next();
    this.destroy$.complete();
  }

}


