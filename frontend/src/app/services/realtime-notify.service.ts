import { Injectable, NgZone } from '@angular/core';
import {HubConnection , HubConnectionBuilder} from '@microsoft/signalr';
import { Observable, Subject } from 'rxjs';
import { NotificationService } from './notification.service';
import { SoundService } from './sound.service';
import { UsersService } from './users.service';
import { environment } from '../../environment/environment';

@Injectable({
  providedIn: 'root',
})

export class RealtimeNotifyService {
  private hubCommention!: HubConnection;
  userid : string = '';
  TryConnectedTime = 0;
  notifyideslist : string[] = [];
  private notificationSubject = new Subject<any>();
  private listenerBound = false;


  constructor(
    private notService: NotificationService,
    private ngZone : NgZone,
    private soundService: SoundService,
    private usersService: UsersService,
  ){
    this.hubCommention = new HubConnectionBuilder()
    .withUrl(`${environment.RealtimeNotification}`)
    .withAutomaticReconnect()
    .build();
  }

  async connect(){
    this.userid = JSON.parse(localStorage.getItem('profile') as string)?.result?._id;
    if(this.userid && this.TryConnectedTime == 0){
      this.TryConnectedTime = this.TryConnectedTime + 1;

      this.hubCommention.start()
        .then(()=>{
          console.log("Notification hub Connected");
          if(this.userid){
            this.hubCommention.invoke('JoinChannel', this.userid).catch(error =>{
              console.log('Error Joining notification channel:', error);
            });
          }
        })
        .catch(error =>{
              console.log('Error connecting to notification hub', error);
              this.TryConnectedTime = 0; // reste to allow retry
        })
    }
  }

  stop(): void {
    if(this.listenerBound){
      this.hubCommention.off('ReceiveNotification');
      this.listenerBound = false;
    }
    this.hubCommention.stop();
    this.TryConnectedTime = 0;
    this.notifyideslist = [];
  }


  reciveNotification(): Observable<any>{
    this.ensureNotificationListener();
    return this.notificationSubject.asObservable();
  }

  private isSelfNotification(notificaton: any): boolean {
    const current = this .usersService.UserServiceData;;
    if(!current?._id){
      return false;
    }

    const notiifUser = notificaton?.user ?? {};

    return notiifUser.userId === current._id;
    }


    private ensureNotificationListener(){
      console.log("hey ensure notificaton")
      if(this.listenerBound){
        return;
      }
      this.listenerBound = true;
      this.hubCommention.on('ReceiveNotification', (notification: any) =>{
        this.ngZone.run(()=>{
          if(!this.isSelfNotification(notification)){
            this.soundService.playNotification();
          }
          const notifId = notification?.id ?? notification?._id ?? '';
          if(notifId){
            if(!this.notifyideslist.includes(notifId) || this.notifyideslist.length == 0) {
              this.notifyideslist.push(notifId);
              this.notService.incrementUnreadCount(1);
            }
          } else {
            this.notService.incrementUnreadCount(1);
          }
          this.notificationSubject.next(notification);
        })
      })
    }
}
