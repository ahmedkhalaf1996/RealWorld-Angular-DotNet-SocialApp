import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { UnrededMsg } from '../types/Message';

@Injectable({
  providedIn: 'root',
})
export class ChatStateService {
  private unreadByUserSubject = new  BehaviorSubject<Record<string, number>>({});
  readonly unreadByUser$ =this.unreadByUserSubject.asObservable();

  setUnreadFromServer(list: UnrededMsg[]) {
    const map: Record<string, number> = {};
    (list ?? []).forEach((msg)=>{
      const id = String(msg.otherUserid ?? '');
      if(!id){
        return
      }
      const num = Number(msg.numOfUnreadedMessages);
      map[id] = Number.isFinite(num) ? num : 0;
    });
    this.unreadByUserSubject.next(map);
  }

  incrementUnred(userId: string, delta: number = 1) {
    if(!userId){
      return;
    }

    const current = this.unreadByUserSubject.value;
    const next = { ...current };
    const prevVal = Number(next[userId] ?? 0);
    next[userId] = Number.isFinite(prevVal) ? prevVal + delta : delta;
    this.unreadByUserSubject.next(next);
  }

  clearUnread(userId: string) {
    if(!userId){
      return;
    }
    const current = this.unreadByUserSubject.value;
    if(current[userId] === 0){
      return;
    }

    const next = {...current, [userId]: 0};
    this.unreadByUserSubject.next(next);
  }


  getUnread(userId: string): number {
    const current = this.unreadByUserSubject.value;
    const val = Number(current?.[userId] ?? 0);
    return Number.isFinite(val) ? val : 0;
  }

  reset(){
    this.unreadByUserSubject.next({});
  }

}
