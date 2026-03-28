import { Injectable, NgZone } from '@angular/core';
import { environment } from '../../environment/environment';
import { Notification } from '../types/Notification';
import { BehaviorSubject, catchError, Observable, of, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private apiUrl = environment.APIUrl+'notification'
  NotifyServiceData:Array<Notification> = []
  userid : string =  JSON.parse(localStorage.getItem('profile') as string)?.result?._id;
  IsThereIsUnreadedNotifiy: number = 0;
  public unreadCount$ = new BehaviorSubject<number>(0);

  constructor(private http:HttpClient, private ngZone: NgZone) {}

  private setUnreadedCount(count: number) {
    const safeCount = Number.isFinite(count) ? count : 0;
    this.ngZone.run(()=>{
      this.IsThereIsUnreadedNotifiy = safeCount;
      this.unreadCount$.next(safeCount);
    });
  }

  incrementUnreadCount(delta: number =1){
    const next = (this.IsThereIsUnreadedNotifiy ?? 0) + delta;
    this.setUnreadedCount(next)
  }

  syncUnreadCountFromList(list?: Notification[]){
    const source = list ?? this.NotifyServiceData ?? [];
    let unread = 0;
    source.forEach(noty =>{
      if(noty.isreded === false){
        unread = unread + 1;
      }
    })
    this.setUnreadedCount(unread);
  }

  markAllAsReadLocal(){
    if(this.NotifyServiceData?.length){
      this.NotifyServiceData = this.NotifyServiceData.map((noty)=>({
        ...noty,
        isreded: true
      }))
    }
    this.setUnreadedCount(0);
  }

  checkifWeHaveUnreadedNotifications(){
    this.GetUserNotification().subscribe({
      next: ()=> {
        this.syncUnreadCountFromList(this.NotifyServiceData);
        console.log('notservice', this.IsThereIsUnreadedNotifiy)
      }
    })
  }

  GetUserNotification(): Observable<{ notifications: Notification[] }> {
      this.userid =  JSON.parse(localStorage.getItem('profile') as string)?.result?._id;
      const notify = this.http.get<{ notifications: Notification[] }>(`${this.apiUrl}/${this.userid}`).pipe(
        tap((res: { notifications: Notification[] }) =>{
          console.log('notificaton res', res)
          const { notifications } = res;
          this.NotifyServiceData = notifications ?? [];
        }),
        catchError(()=>{
          this.NotifyServiceData = [];
          this.setUnreadedCount(0);
          return of({ notifications:[] as Notification[] });
        })
      );
      return notify;
  }

  MarkUserNotificationAsReaded(): Observable<{ notifications: Notification[] }> {
      this.userid =  JSON.parse(localStorage.getItem('profile') as string)?.result?._id;
      const notify = this.http.get<{ notifications: Notification[] }>(`${this.apiUrl}/mark-notification-asreaded?id=${this.userid}`).pipe(
        tap((res: { notifications: Notification[] }) =>{
          const { notifications } = res;
          this.NotifyServiceData = notifications ?? [];
          this.setUnreadedCount(0);
        }),
        catchError(()=>{
          this.setUnreadedCount(0)
          return of({ notifications:[] as Notification[] });
        })
      );
      return notify;
    }

}
