import { HttpClient } from '@angular/common/http';
import { Injectable, NgZone } from '@angular/core';
import { environment } from '../../environment/environment';
import { Message, UnrededMsg } from '../types/Message';
import { BehaviorSubject, Observable } from 'rxjs';
import { Router } from '@angular/router';
import { ChatStateService } from './chat-state.service';



@Injectable({
  providedIn: 'root',
})

export class ChatService {
  private apiUrl = environment.APIUrl+'chat'
  public UnreadedMsgNum : number = 0;
  public UnreadedMsgList: UnrededMsg[] = [];
  public unreadCount$ = new BehaviorSubject<number>(0);
  userid : string = JSON.parse(localStorage.getItem('profile') as string)?.result?._id;

  constructor(
    private http:HttpClient,
    private router: Router,
    private ngZone: NgZone,
    private chatState: ChatStateService
  ){}

  // setUnreadCount
  private setUnreadCount(count: number){
    const safeCount = Number.isFinite(count) ? count : 0;
    this.ngZone.run(()=>{
      this.UnreadedMsgNum = safeCount;
      this.unreadCount$.next(safeCount);
    })
  }
  // incrementUnreadCount
  incrementUnreadCount(delta: number = 1){
    const next = (this.UnreadedMsgNum ?? 0) + delta;
    this.setUnreadCount(next);
  }

  // sendMessage
  sendMessage(formData:Message): Observable<any> {
    return this.http.post<Message>(`${this.apiUrl}/sendmessage`, formData)
  }
  // getMessgesBetweenTowUsers
  getMessgesBetweenTowUsers(from:any, firstuid: string, seconduid: string): Observable<{ msgs: Message[] }> {
    return this.http.get<{ msgs: Message[] }>(`${this.apiUrl}/getmsgsbynums?from=${from}&firstuid=${firstuid}&seconduid=${seconduid}`)
  }
  // getUnreadedMessgesNum
  getUnreadedMessgesNum(): Observable<any>{
    this.userid = JSON.parse(localStorage.getItem('profile') as string)?.result?._id;
    var data = this.http.get<{ total: number, messages: UnrededMsg[] }>(`${this.apiUrl}/get-user-unreadedmsg?userid=${this.userid}`)
    data.subscribe({
      next:(res: { total: number; messages: UnrededMsg[] })=>{
        const {total, messages} = res;
        this.UnreadedMsgList = Array.isArray(messages) ? messages : [];
        this.chatState.setUnreadFromServer(this.UnreadedMsgList);
        this.setUnreadCount(Number.isFinite(total) ? total : 0);
        console.log('service data here urm', this.UnreadedMsgList)
      }
    })
    return data;
  }
  // MarkMsgAsReaded
  async MarkMsgAsReaded(fuid: string, suid: string) {
    var data = await this.http.get<any>(`${this.apiUrl}/mark-msg-asreaded?mainuid=${fuid}&otheruid=${suid}`)
    data.subscribe({
      next: (res: { isMarked?: boolean }) =>{
        console.log("res mark", res)
        if(res?.isMarked){
          this.getUnreadedMessgesNum()
        }
      }
    })
  }
}
