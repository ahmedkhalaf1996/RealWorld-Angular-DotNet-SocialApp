import { RealtimechatService } from './realtimechat.service';
import { environment } from './../../environment/environment';
import { Injectable, NgZone } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import {tap} from 'rxjs/operators';
import { HttpClient , HttpHeaders } from '@angular/common/http';
import { jwtDecode } from 'jwt-decode';

import { Router } from '@angular/router';
import { User } from '../types/User';
import { Post} from '../types/Post';
import { RealtimeNotifyService } from './realtime-notify.service';

function httpOptions(){
  var opt = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${JSON.parse(localStorage.getItem('profile') as string)?.token}`
    })
  }
  return opt;
}



@Injectable({
  providedIn: 'root',
})
export class UsersService {

  UserServiceData:User = {};
  private apiUrl = environment.APIUrl+'user';

  userid : string = JSON.parse(localStorage.getItem('profile') as string)?.result?._id;

  private userSubject = new BehaviorSubject<User | null>(null);
  readonly user$ = this.userSubject.asObservable();

  private authSubject = new BehaviorSubject<boolean>(false);
  readonly isAuthenticated$ = this.authSubject.asObservable();
  isAuthenticated: boolean = false;

  constructor(
    private http: HttpClient,
    private router: Router,
    private ngZone: NgZone,
    private realtimechatService: RealtimechatService
  ) {
    this.initilizeFromStorage();
  }


  UpdateAuth(){
    this.initilizeFromStorage();

  }

  //  Get user following followers
  GetUserFollowersFollowing(){
    var followers = this.UserServiceData.followers;
    var following = this.UserServiceData.following;
    if(!followers) {followers = []};
    if(!following) {following = []};

    const combineArry = [...followers, ...following];
    const uniqueArray = Array.from(new Set(combineArry));
    return uniqueArray;
  }
  // Login User
  signIn(formData:User): Observable<any> {
    let auth = this.http.post<User>(`${this.apiUrl}/signin`, formData);
    auth.subscribe({
      next:(res:any)=> {
        this.UserServiceData = res.result;
        localStorage.setItem('profile', JSON.stringify({...res}));
      }
    })
    return auth;
  }
  // Sign Up User
  signUp(formData:User): Observable<any> {
    return this.http.post<User>(`${this.apiUrl}/signup`, formData);
   }

  // Get User Data & posts
  GetUserData(id:string, page:number = 1): Observable<Post&User|any> {
    return this.http.get<Post&User|any>(`${this.apiUrl}/getUser/${id}?page=${page}`);
  }
  // Get Suggeestion Users
  getSugUser(id: string): Observable<User[] | any> {
    return this.http.get<User[]>(`${this.apiUrl}/getSug?id=${id}`, httpOptions());
  }
  // Update User Data
  UpdateUser(userData: User): Observable<User> {
    return this.http.patch<User>(`${this.apiUrl}/Update/${userData._id}`, userData, httpOptions());
  }

  // Follow /unFollow User
  following(id: string) : Observable<User> {
    return this.http
      .patch<any>(`${this.apiUrl}/${id}/following`, {}, httpOptions())
      .pipe(
        tap((res: any) => {
          const updatedUser = res?.user1 ?? res?.user ?? null;
          if (updatedUser && updatedUser._id ) {
            this.updateUserState(updatedUser as User);
        }
      }
        )
      );
  }


  // check is user is authenticatied
  async isUserAuth() {
    const profile  = JSON.parse(localStorage.getItem('profile') as string);
    const token = profile?.token;

    if(!profile || !token){
      this.emitAuthState(null, false);
      return;
    }

    const decoededToken: any = jwtDecode(token);
    if (decoededToken.exp * 1000 < new Date().getTime()) {
      this.LogOut();
      return;
    }

    const storedUser: User = profile.result ?? ({} as User);
    this.emitAuthState(storedUser, true);

    const up = this.GetUserData(String(storedUser._id));
    up.subscribe({
      next: (res: any) => {
        const updatedUser = {
          ...storedUser,
          following: res?.user?.following ?? storedUser.following,
          followers: res?.user?.followers ?? storedUser.followers,
          name: res?.user?.name ?? storedUser.name,
          bio: res?.user?.bio ?? storedUser.bio,
          imageUrl: res?.user?.imageUrl ?? storedUser.imageUrl,
        } as User;
        const nextPrifile = {...profile, result: updatedUser};
        localStorage.setItem('profile', JSON.stringify(nextPrifile));
        this.emitAuthState(updatedUser, true);
      }
    })
  }


  // logout user
  LogOut(){
    localStorage.clear();
    // todo stop connection to realtime chat
    this.realtimechatService.stopChatConnection();
    this.emitAuthState(null, false);
    this.router.navigate(['/auth']);
  }

  private initilizeFromStorage() {
    const profile  = JSON.parse(localStorage.getItem('profile') as string);
    const storedUser: User | null = profile?.result ?? null;
    const token: string = profile?.token;
    const isAuth = !!storedUser && !!token;
    this.emitAuthState(storedUser, isAuth);

    if(isAuth){
      this.isUserAuth();
    }

  }

  private emitAuthState(user: User | null, isAuth: boolean) {
    this.ngZone.run(() => {
      this.UserServiceData = user ?? {};
      this.isAuthenticated = isAuth;
      this.userid = user?._id ?? '';

      this.userSubject.next(user);
    this.authSubject.next(isAuth);
    });
  }

  setAuthFromProfile( profile: any ) {
    const user: User | null = profile.result ?? null;
    const token: string = profile.token;
    if(!user || !token){
      this.emitAuthState(null, false);
      return;
    }
    localStorage.setItem('profile', JSON.stringify(profile));
    this.emitAuthState(user, true);
  }

  updateUserState(updatedUsser: User) {
    if(!updatedUsser){
      return;
    }
    const profile  = JSON.parse(localStorage.getItem('profile') as string);
    const token: string = profile?.token;
    const mergedUser = {...(profile?.result ?? {}), ...updatedUsser} ;
    const nextProfile = token ? {...profile, result: mergedUser} : {result: mergedUser};
    localStorage.setItem('profile', JSON.stringify(nextProfile));
    this.emitAuthState(mergedUser as User, !!token);
   }




}
