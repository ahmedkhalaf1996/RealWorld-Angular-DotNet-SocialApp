import { ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { User, UserInChat } from '../types/User';
import { Message } from '../types/Message';
import { resolveImageSrc } from '../utils/image';
import { UsersService } from '../services/users.service';
import { ChatService } from '../services/chat.service';
import { RealtimechatService } from '../services/realtimechat.service';
import { ChatStateService } from '../services/chat-state.service';
import {takeUntil , filter} from 'rxjs/operators'
import { Subject } from 'rxjs';

@Component({
  selector: 'app-chat',
  standalone: false,
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css',
})
export class ChatComponent implements OnInit , OnDestroy{
  @ViewChild('chatContent', {static: true}) chatContent: ElementRef | any;
  MainUser:User = {} as User;

  StartPointGetingChat: number = 0;

  FrindsListUsers: UserInChat[] = [];
  onlineUsers: string[] = [];
  chatMessage: Message[] = [];

  newMessage: string = '';
  isLoadingOlder = false;
  hasMoreMessages = true;
  private readonly scrollLoadThreshold = 8;
  private readonly initialPagesToload = 2;
  private readonly bottomThreshold = 128;
  isNearBottom = true;
  showNewMessageIndicator = false;


  selectedUser: User = {} as User;
  private lastUserId: string | null = null;
  private lastFollowKey: string = '';
  private activeChatKey: string = '';
  private destroy$ = new Subject<void>();
  private refreshOnlineUsersTimeout : any;
  private periodicRefreshInterval: any;
  resolveImage = resolveImageSrc;

  constructor(
    private userService: UsersService,
    private chatService: ChatService,
    public realtimeChat: RealtimechatService,
    private chatstate: ChatStateService,
    private cdr : ChangeDetectorRef
  ){}

  isUserOnline(userId: string): boolean {
    return this.onlineUsers.includes(userId);
  }

  async ngOnInit(): Promise<void> {
    this.userService.user$
        .pipe(
          filter((user): user is User => !!user && !!user._id),
          takeUntil(this.destroy$)
        ).subscribe(async (user)=>{
          await this.syncForUser(user);
        })

    this.realtimeChat.onlieUsers$
        .pipe(takeUntil(this.destroy$))
        .subscribe((onlineUsers)=>{
          this.onlineUsers = onlineUsers;
          this.cdr.markForCheck();
        })


    this.realtimeChat.messageReceived$
        .pipe(takeUntil(this.destroy$))
        .subscribe((newMessage)=>{
          if(!newMessage){
            return;
          }
          const isSelfEcho = this.tryResolveSelfEcho(newMessage);
          if(isSelfEcho){
            return;
          }
          if(newMessage._id && this.hasMessageId(newMessage._id)){
            return
          }
          const mainId = this.MainUser?._id;
          if(mainId && newMessage.recever === mainId){
            if(this.selectedUser?._id && newMessage.sender === this.selectedUser._id){
              this.chatstate.clearUnread(String(newMessage.sender))
              this.chatService.MarkMsgAsReaded((String(mainId)), String(newMessage.sender));
            } else if(newMessage.sender){
              this.chatstate.incrementUnred(String(newMessage.sender));
            }
          }
          if(newMessage.sender === this.selectedUser._id || newMessage.recever === this.selectedUser._id){
            this.chatMessage.push(newMessage);
            requestAnimationFrame(()=>{
              if(this.isNearBottom){
                this.scrollToBottom(true);
              } else {
                this.showNewMessageIndicator = true;
              }
            })
          }
        })

        //
        this.chatstate.unreadByUser$
            .pipe(takeUntil(this.destroy$))
            .subscribe((map)=>{
              this.FrindsListUsers.forEach((user)=>{
                const count = Number(map?.[String(user._id)] ?? 0);
                user.unReadedMessage = Number.isFinite(count) ? count : 0;
              })
              this.cdr.markForCheck();
            })

      this.realtimeChat.refreshOnlineUses$
          .pipe(takeUntil(this.destroy$))
          .subscribe(()=>{
            console.log('recenved refersh online users signal')
            this.refreshOnlineUsersStatus(true);
          })

      //
      this.StartPeriodicFollowerCheck();
  }

  private hasMessageId(id: string): boolean {
    return this.chatMessage.some(m => m._id === id);
  }
  private StartPeriodicFollowerCheck (){
    this.periodicRefreshInterval = setTimeout(() => {
      if(!this.MainUser?._id){
        return;
      }

      const currentFollowKey = this.buildFollowKey(this.MainUser);
      if(currentFollowKey !== this.lastFollowKey){
        this.lastFollowKey = currentFollowKey;
        const uid = String(this.MainUser._id)
        this.realtimeChat.refreshUserRooms(uid).then(()=>{
          setTimeout(()=>{
            this.refreshOnlineUsersStatus(true);
          }, 200)
        })
      }
    }, 30000);
  }

  private async syncForUser(user: User) {
    const uid = user?._id ? String(user._id) : null;
    if(!uid){
      return;
    }

    this.MainUser = user;
    const followKey = this.buildFollowKey(user);
    const isNewUser = uid !== this.lastUserId;
    const followChanged = followKey !== this.lastFollowKey;

    if(isNewUser){
      this.selectedUser = {} as User;
      this.chatMessage = [];
      this.StartPointGetingChat =0;
      this.hasMoreMessages = true;
      await this.realtimeChat.StartUserConnection();
      this.onlineUsers = this.realtimeChat.onlineUsers;
      this.getunrmsg();
    }

    if(isNewUser || followChanged){
      this.GetFollowingUsersList(user);
      this.realtimeChat.refreshUserRooms(uid).then(()=>{
        setTimeout(() => {
          this.refreshOnlineUsersStatus(true);
        }, 1000);
      })
    }

    this.lastUserId = uid;
    this.lastFollowKey = followKey;
    this.cdr.markForCheck();


  }

  getunrmsg(){
    if(!this.MainUser?._id){
      return;
    }
    this.chatService.getUnreadedMessgesNum().subscribe();
  }


  private buildFollowKey(user: User): string {
    const followers = Array.isArray(user.followers) ? user.followers : [];
    const following = Array.isArray(user.following) ? user.following : [];
    return [...followers, following].sort().join('|');
  }

  GetFollowingUsersList(sourceUser?:User){
    const baseUser = sourceUser ?? this.MainUser;
    if(!baseUser?._id){
      return;
    }

    this.FrindsListUsers = [];
    const followers = Array.isArray(baseUser.followers) ? baseUser.followers : [];
    const following = Array.isArray(baseUser.following) ? baseUser.following : [];

    const uides = Array.from(new Set([...followers, ...following])).filter((id) => !! id);
    if(uides && uides?.length > 0){
      uides.forEach(id =>{
        this.userService.GetUserData(id).subscribe({
          next:({user}) =>{
            console.log('chat users', user)
            var uichat: UserInChat = {
              _id: user._id,
              name: user.name,
              imageUrl: user.imageUrl,
              unReadedMessage: this.chatstate.getUnread(String(user._id))
            }
            this.FrindsListUsers.push(uichat);
            this.removeDuplicates();
            if(!this.selectedUser?._id && this.FrindsListUsers.length > 0){
              this.selectUser(this.FrindsListUsers[0]);
            }
            if(this.selectedUser?._id && !this.FrindsListUsers.find(u => String(u.id) === String(this.selectedUser.id)))
            {
              this.selectedUser = {} as User;
              this.chatMessage = [];
            }
            this.cdr.markForCheck();
            this.refreshOnlineUsersStatus(true);
          }
        })
      })
    }
  }

  private refreshOnlineUsersStatus(immediate: boolean = false){
    const mainUserId = String(this.MainUser?._id);
    if(!mainUserId || mainUserId === 'undefined'){
      return;
    }

    if(immediate){
      if(this.refreshOnlineUsersTimeout){
        clearTimeout(this.refreshOnlineUsersTimeout);
        this.refreshOnlineUsersTimeout = null
      }

      this.realtimeChat.getOnlineUsersStatus(mainUserId)
          .then((onlineUsers: string[]) =>{
           if(Array.isArray(onlineUsers)){
             this.onlineUsers = onlineUsers;
            this.realtimeChat.onlineUsers = onlineUsers;
            this.realtimeChat.onlineUsersSubject.next(onlineUsers);
            this.cdr.markForCheck();
            console.log("refreshed online users (imimediate):", onlineUsers);
           }
          })
          .catch(error => console.error('Error fetching online users status:', error));
          return;

    }

    // debounce refhres calles to avoid to many backed req
    if(this.refreshOnlineUsersTimeout){
      clearTimeout(this.refreshOnlineUsersTimeout);
    }

    this.refreshOnlineUsersTimeout = setTimeout(() => {
      this.realtimeChat.getOnlineUsersStatus(mainUserId)
          .then((onlineUsers: string[])=>{
            if(Array.isArray(onlineUsers)){
             this.onlineUsers = onlineUsers;
            this.realtimeChat.onlineUsers = onlineUsers;
            this.realtimeChat.onlineUsersSubject.next(onlineUsers);
            this.cdr.markForCheck();
            console.log("refreshed online users (decounced):", onlineUsers);
           }
          })
          .catch(error => console.error('Error fetching online users status:', error));
    }, 500);
  }

  removeDuplicates(){
    this.FrindsListUsers = this.FrindsListUsers.filter((user, index, self) =>
     index === self.findIndex((u) => u._id === user._id)
    )
  }

  selectUser(user: User){
    this.selectedUser = user;
    var fuid = String(this.MainUser?._id);
    var suid = String(user?._id);
    this.activeChatKey = `${fuid}::${suid}`;
    this.StartPointGetingChat = 0;
    this.hasMoreMessages = true;
    this.isLoadingOlder = false;

    this.loadInitalMessages();
    this.clearUnnReadedMsg(suid)
    this.chatService.MarkMsgAsReaded(fuid, suid);
  }

  clearUnnReadedMsg(suid: string){
    this.chatstate.clearUnread(suid);
  }

  private loadInitalMessages(){
    const totalPages = Math.max(1, this.initialPagesToload);
    const laodNext = (remaining: number, isFirst: boolean) =>{
      if(remaining <= 0){
        this.ensureScrollable();
        return;
      }
      this.loadMessagePage({
        reset: isFirst,
        anchor: 'bottom',
        after: ()=> laodNext(remaining -1, false)
      })
    }
    laodNext(totalPages, true)
  }

  private ensureScrollable (){
    const element = this.chatContent?.nativeElement;
    if(!element || !this.hasMoreMessages){
      return;
    }
    if(element.scrollHeight <= element.clientHeight){
        this.loadMessagePage({
        reset: false,
        anchor: 'bottom',
        after: ()=> this.ensureScrollable()
      })
    }
  }


  private loadMessagePage(options: {reset: boolean; anchor: 'bottom' | 'preserve'; after?: ()=> void}){
    if(this.isLoadingOlder){
      return;
    }
    if(!options.reset && !this.hasMoreMessages){
      return;
    }
    if(!this.MainUser?._id || !this.selectedUser?._id){
      return;
    }

    const fuid = String(this.MainUser?._id);
    const suid = String(this.selectedUser?._id);
    const requestKey = `${fuid}::${suid}`;
    const element = this.chatContent?.nativeElement;
    const prevScrollHeight = element?.scrollHeight ?? 0;
    const prevScrollTop = element?.scrollTop ?? 0;

    this.isLoadingOlder = true;
    this.chatService
        .getMessgesBetweenTowUsers(this.StartPointGetingChat, fuid, suid)
        .subscribe({
          next:(res: { msgs: Message[] }) =>{
            if(this.activeChatKey !== requestKey){
              this.isLoadingOlder = false;
              return;
            }
            const existingIds = new Set(
              this.chatMessage
                  .map(m => m._id)
                  .filter((id): id is string => !!id)
            );
            const msgs = (res?.msgs ?? []).filter(m => !m._id || !existingIds.has(m._id));
            if(msgs.length === 0){
              this.hasMoreMessages = false;
              this.isLoadingOlder = false;
              return;
            }
            this.StartPointGetingChat = this.StartPointGetingChat + 1;
            if(options.reset){
              this.chatMessage = msgs;
            } else {
              this.chatMessage = [...msgs, ...this.chatMessage];
            }

            this.cdr.markForCheck();
            this.isLoadingOlder = false;

            if(options.anchor === 'bottom'){
              requestAnimationFrame(()=>{
                this.scrollToBottom(false);
                options.after?.();
              })
              return;
            }

            if(element){
               requestAnimationFrame(()=>{
                const newScrollHeight = element.scrollHeight;
                element.scrollTop = newScrollHeight - prevScrollHeight + prevScrollTop;
                options.after?.()
               });
            } else {
              options.after?.();
            }
          },
          error:()=>{
            this.isLoadingOlder = false;
          }
        })

  }

  private scrollToBottom(smooth: boolean){
    const element = this.chatContent?.nativeElement;
    if(!element){
      return;
    }
    element.scrollTo({ top: element.scrollHeight, behavior: smooth ? 'smooth': 'auto' });
    this.isNearBottom = true;
    this.showNewMessageIndicator = false;
  }

  private tryResolveSelfEcho(incoming: Message): boolean {
    const mainId = this.MainUser?._id;
    if(!mainId || incoming.sender !== mainId){
      return false
    }
    if(incoming.recever !== this.selectedUser?._id){
      return false
    }

    if(!incoming.recever || !incoming.content){
      return false;
    }

    const candidates = this.chatMessage
        .map((m, i) => ({m , i}))
        .filter(({ m }) =>
         m.sender === incoming.sender &&
        m.recever === incoming.recever &&
        m.content === incoming.content &&
        (m.status === 'pending' || m.status ===  'sent')
        );

    if(candidates.length === 0){
      return false
    }

    const chosen = candidates
      .sort((a,b) => (a.m.localCreatedAt ?? 0) - (b.m.localCreatedAt ?? 0))[0];

    this.chatMessage[chosen.i] = {
      ...this.chatMessage[chosen.i],
      status: 'sent'
    };
    this.cdr.markForCheck();
    return true;
  }

  private createOptimisticMessage(content: string, sender: string, recever: string): Message {
    return {
      content,
      sender,
      recever,
      clientId: `c-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
      status: 'pending',
      localCreatedAt: Date.now()
    }
  }

  private markMessageStatus(clientId: string, status: 'pending' | 'sent' | 'failed' | undefined){
    if(!clientId){
      return;
    }
    const index = this.chatMessage.findIndex(m => m.clientId === clientId);
    if(index >=0){
      this.chatMessage[index] = {...this.chatMessage[index], status};
      this.cdr.markForCheck()

    }
  }


  OnScrollGetOldMessages(){
    const element = this.chatContent.nativeElement;
    if(!element || !this.selectedUser?._id || !this.MainUser?._id){
      return;
    }
    this.updateScrollState();
    if(element.scrollTop <= this.scrollLoadThreshold){
      this.loadMessagePage({reset: false, anchor :'preserve'});
    }
  }

  private updateScrollState(){
    const element = this.chatContent?.nativeElement;
    if(!element){
      return;
    }
    const distanceFromBottom = element.scrollHeight - element.scrollTop - element.clientHeight;
    this.isNearBottom = distanceFromBottom <= this.bottomThreshold;
    if(this.isNearBottom){
      this.showNewMessageIndicator = false
    }
  }

  jumpToBottom(){
    this.scrollToBottom(true);
  }

  sendMessage(){
    if(this.newMessage.trim() !== ''){
      var fuid = String(this.MainUser?._id)
      var suid = String(this.selectedUser?._id)

      const newMsg: Message = {
        content: this.newMessage,
        sender: fuid,
        recever : suid
      }
      const optimistic = this.createOptimisticMessage(newMsg.content ?? '', fuid, suid);
      this.chatMessage.push(optimistic);
      this.newMessage = '';
      this.cdr.markForCheck();
      if(this.isNearBottom){
        requestAnimationFrame(()=> this.scrollToBottom(true));
      } else {
        this.showNewMessageIndicator = true;
      }

      if(this.isUserOnline(suid)){
        this.realtimeChat.sendPrivateMessage(newMsg)
        ?.then(()=> {
          this.markMessageStatus(optimistic.clientId ?? '', 'sent')
        })
        .catch(()=>{
          this.markMessageStatus(optimistic.clientId ?? '' , 'failed')
        })
      } else {
        // offline message
        this.chatService.sendMessage(newMsg).subscribe({
          next: ()=>{
            this.markMessageStatus(optimistic.clientId ?? '', 'sent');
          },
          error:()=>{
            this.markMessageStatus(optimistic.clientId ?? '', 'failed');
          }
        })
      }

    }
  }

  trackMessage(index: number, message: Message): string {
  return (
    message._id ||
    message.clientId ||
    `${message.sender}-${message.recever}-${message.localCreatedAt}-${index}`
  );
}

  ngOnDestroy(): void {
    if(this.refreshOnlineUsersTimeout){
      clearTimeout(this.refreshOnlineUsersTimeout);
    }
    if(this.periodicRefreshInterval){
      clearInterval(this.periodicRefreshInterval);
    }
    this.destroy$.next();
    this.destroy$.complete();
  }

}
