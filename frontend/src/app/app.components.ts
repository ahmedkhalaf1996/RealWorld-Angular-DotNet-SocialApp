import { User } from './types/User';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { filter, Subject, Subscription, takeUntil } from 'rxjs';
import { UsersService } from './services/users.service';
import { SoundService } from './services/sound.service';
import { RealtimeNotifyService } from './services/realtime-notify.service';
import { RealtimechatService } from './services/realtimechat.service';

@Component({
  selector: 'app-root',
  standalone: false,
  templateUrl: './app.components.html',
  styleUrl: './app.components.css',
})
export class AppComponent implements OnInit, OnDestroy {

  private notificationSubscription!: Subscription;
  private destroy$ = new Subject<void>()

  constructor(
    public AppusersService: UsersService,
    private soundService: SoundService,
    public RealTimeNotification: RealtimeNotifyService,
    public realtimeChat : RealtimechatService,
  ){}

  async ngOnInit(): Promise<void> {
    this.AppusersService.UpdateAuth();
    this.AppusersService.isUserAuth();

    this.soundService.bindAutoUnlock();

    this.AppusersService.user$
        .pipe(
          filter((user): user is User => !!user && !!user._id),
          takeUntil(this.destroy$)
        )
        .subscribe(async ()=>{
          await this.RealTimeNotification.connect();
          await this.realtimeChat.StartUserConnection();
        })
    // start subscirption
    this.notificationSubscription = this.RealTimeNotification
        .reciveNotification()
        .subscribe(()=>{});
  }


  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
    this.notificationSubscription?.unsubscribe();
    this.RealTimeNotification.stop();
    this.realtimeChat.stopChatConnection();
  }
}
