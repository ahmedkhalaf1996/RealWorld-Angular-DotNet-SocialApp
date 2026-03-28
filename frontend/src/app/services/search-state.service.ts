import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SearchStateService {
  private querySubject = new BehaviorSubject<string>('');
  readonly query$ = this.querySubject.asObservable();

  setQuery(query:string){
    this.querySubject.next((query ?? '').trim());
  }

  get snapshot(): string {
    return this.querySubject.value;
  }
}
