import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivate, GuardResult, MaybeAsync, Router, RouterStateSnapshot } from "@angular/router";
import { UsersService } from "../services/users.service";
import { Observable } from "rxjs";



@Injectable({
  providedIn:'root',
})

export class AuthGuard implements CanActivate {
  constructor(public userService: UsersService, public router:Router){}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot):  Observable<boolean> | Promise<boolean> | boolean {
    const profile = JSON.parse(localStorage.getItem('profile') as string);
    const hasToken = !!profile?.token;
    const isAuth = this.userService.isAuthenticated || hasToken;
    if(!isAuth){
      this.router.navigate(['auth'])
      return false;
    }
    return true;
  }
}
