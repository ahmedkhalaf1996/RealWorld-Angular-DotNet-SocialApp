import { User } from './../types/User';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { debounceTime, distinctUntilChanged, filter, Subject, takeUntil } from 'rxjs';
import { UsersService } from '../services/users.service';
import { NotificationService } from '../services/notification.service';
import { ChatService } from '../services/chat.service';
import { RealtimechatService } from '../services/realtimechat.service';
import { SearchStateService } from '../services/search-state.service';
import { resolveImageSrc } from '../utils/image';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
})
export class NavbarComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  unreadChatCount = 0;
  unreadNotifyCount = 0;

  value: string = '';
  searchControl = new FormControl<string>('', { nonNullable: true })

  constructor(
    private router: Router,
    private cdr : ChangeDetectorRef,
    public userNavService: UsersService,
    public notificationService: NotificationService,
    public chatService: ChatService,
    public realtimeChat: RealtimechatService,
    private searchState: SearchStateService
  ){}

  resolveImage = resolveImageSrc;

  async ngOnInit(): Promise<void> {
    this.searchState.query$
        .pipe(takeUntil(this.destroy$))
        .subscribe((query)=>{
          if(query !== this.searchControl.value){
            this.searchControl.setValue(query, {emitEvent: false});
            this.cdr.markForCheck();
          }
        });
    this.searchControl.valueChanges
        .pipe(
          debounceTime(200),
          distinctUntilChanged(),
          takeUntil(this.destroy$)
        )
        .subscribe((query)=>{
          this.searchState.setQuery(query);
          if(this.router.url.startsWith('/search')){
            this.router.navigate([], {
              queryParams: {data: query || null},
              queryParamsHandling: 'merge',
              replaceUrl: true
            })
          }
        });

        this.chatService.unreadCount$
            .pipe(takeUntil(this.destroy$))
            .subscribe((count)=>{
              // console.log("%%%%%%%%%%%% unreded count", count)
              this.unreadChatCount = Number.isFinite(count) ? count : 0;
              this.cdr.markForCheck();
            })

        this.notificationService.unreadCount$
                .pipe(takeUntil(this.destroy$))
                .subscribe((count)=>{
                  // console.log("^^^%%%%%%$$%^^ notificatin", count)
                  this.unreadNotifyCount = Number.isFinite(count) ? count : 0;
                  this.cdr.markForCheck();
        })

        this.userNavService.user$
            .pipe(
              filter((user): user is User => !!user && !!user._id),
              takeUntil(this.destroy$)
            )
            .subscribe(()=>{
              this.chatService.getUnreadedMessgesNum();
              this.notificationService.checkifWeHaveUnreadedNotifications();
              this.cdr.markForCheck();
            });

        this.userNavService.isAuthenticated$
            .pipe(takeUntil(this.destroy$))
            .subscribe(()=>{
              this.cdr.markForCheck();
            })

            this.realtimeChat.messageReceived$
                  .pipe(takeUntil(this.destroy$))
                  .subscribe((message)=>{
                    if(message?.recever && message.recever === this.userNavService.userid){
                      this.chatService.incrementUnreadCount(1);
                    }
                  });
        this.notificationService.checkifWeHaveUnreadedNotifications();
  }

  Search(data: string){
    this.searchState.setQuery(data);
    this.router.navigate(['/search'], {queryParams:{data}})
  }

  logUserOut(){
    this.userNavService.LogOut();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
