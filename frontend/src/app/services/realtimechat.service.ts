import { EventEmitter, Injectable, NgZone } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Message } from '../types/Message';
import { BehaviorSubject, Subject } from 'rxjs';
import { SoundService } from './sound.service';
import { environment } from '../../environment/environment';

@Injectable({
  providedIn: 'root',
})
export class RealtimechatService {

private chatConnection?: HubConnection;
onlineUsers: string[] = [];
privateMessage: Message[] = [];
userid : string = "";
tryConnectedTime = 0;
privateMessageUpdated: EventEmitter<Message[]> = new EventEmitter<Message[]>();

onlineUsersSubject = new BehaviorSubject<string[]>([]);
readonly onlieUsers$ = this.onlineUsersSubject.asObservable();

private MessageReceivedSubject = new Subject<Message>();
readonly messageReceived$ = this.MessageReceivedSubject.asObservable();

private refreshOnlineUsersSubject = new Subject<void>();
readonly refreshOnlineUses$ = this.refreshOnlineUsersSubject.asObservable();


constructor(private NgZone:NgZone, private soundService: SoundService){}

// StartUserConnection
// async StartUserConnection(){
//   this.userid = JSON.parse(localStorage.getItem('profile') as string)?.result?._id;

//   if(this.tryConnectedTime == 0 && this.userid){
//     console.log('Realtime Chat Service Start user called!!')

//     await this.createChatConnection();
//     this.tryConnectedTime = this.tryConnectedTime +1;
//     this.onlineUsers = [];
//     this.onlineUsersSubject.next([]);
//   }

// }

async StartUserConnection(){
  this.userid = JSON.parse(localStorage.getItem('profile') as string)?.result?._id;

  // Check actual connection state, not just a counter
  if(this.chatConnection?.state === 'Connected' ||
     this.chatConnection?.state === 'Connecting'){
    return; // already connected, do nothing
  }

  if(this.userid){
    await this.createChatConnection();
  }
}

// createChatConnection
async createChatConnection(){
  this.chatConnection = new HubConnectionBuilder()
  .withUrl(`${environment.RaeltimeUrl}hubs/chat?UserID=${this.userid}`).withAutomaticReconnect().build();

  await  this.chatConnection.start().catch(error => {
    console.log("error starting chat connection:", error);
    this.tryConnectedTime = 0;
  })

  this.chatConnection.on('UserConnected', ()=>{
    this.NgZone.run(()=>{
      console.log('UserConnected')
      if(this.userid){
        this.addUserConnectionId(this.userid);
      }
    })
  })


  this.chatConnection.on('OnlineUsers' + this.userid, (onlineUsers)=>{
    this.NgZone.run(()=>{
      console.log('OnlineUsers', onlineUsers);
      const uniqeUsers: string[] = Array.from(new Set(onlineUsers));
      this.onlineUsers = [...uniqeUsers];
      this.onlineUsersSubject.next(this.onlineUsers);
    })
  })

  this.chatConnection.on('OpenPrivateChat', (newMessage: Message)=>{
    this.NgZone.run(()=>{
      console.log('OpenPrivateChat', newMessage);
      this.privateMessage = [...this.privateMessage, newMessage];
    })
  })

  this.chatConnection.on('NewPrivateMessage', (newMessage: Message)=>{
    this.NgZone.run(()=>{
      this.privateMessage = [...this.privateMessage, newMessage];
      this.privateMessageUpdated.emit(this.privateMessage);
      this.MessageReceivedSubject.next(newMessage);
      if(this.userid && newMessage?.recever === this.userid && newMessage?.sender !== this.userid){
        this.soundService.playMessage();
      }
      console.log('NewPrivateMessage', this.privateMessage);
    })
  })

  this.chatConnection.on('ClosePrivateChat', ()=>{
    this.NgZone.run(()=>{
      console.log('ClosePrivateChat')
      this.privateMessage = []
    })
  })

  this.chatConnection.on('RefreshOnlineUsersNeeded', (newlyConnectedUserId: string)=>{
    this.NgZone.run(()=>{
      console.log('RefreshOnlineUsersNeeded', newlyConnectedUserId);
      this.refreshOnlineUsersSubject.next();
    })
  })
}
// stopChatConnection
stopChatConnection(){
  this.tryConnectedTime = 0;
  this.chatConnection?.stop().catch(error => console.log(error));
  this.onlineUsers = [];
  this.onlineUsersSubject.next([]);
}
// addUserConnectionId
async addUserConnectionId(userid: string){
  return this.chatConnection?.invoke('AddUserConnectionId', userid)
             .catch(error => console.log(error));
}
// sendPrivateMessage
async sendPrivateMessage(message:Message){
  return this.chatConnection?.invoke('RecivePrivateMessage', message)
              .catch(error => console.log(error));
}
// closePrivateChatMessage
async closePrivateChatMessage(otherUser: string) {
  return this.chatConnection?.invoke('RemovePrivateChat', this.userid, otherUser)
}
// getOnlineUsersStatus
async getOnlineUsersStatus(userId: string): Promise<string[]>{
  try {
    const result = await this.chatConnection?.invoke('GetOnlineUsersStatus',userId);
    if(Array.isArray(result)){
      return result;
    }
    return [];
  } catch (error) {
    console.error('error geting online users status:', error);
    return [];
  }
}
// refreshUserRooms
async refreshUserRooms(userId: string): Promise<void> {
  try {
    await this.chatConnection?.invoke('RefreshUserRooms', userId)
    console.log('user romms refresh for :', userId)
  } catch (error) {
    console.error('Error refreshing user romms : ', error)
  }
}
}
