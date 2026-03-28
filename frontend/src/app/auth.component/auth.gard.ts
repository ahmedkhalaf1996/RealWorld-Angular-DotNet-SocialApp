import {Injectable} from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, GuardResult, MaybeAsync, Router, RouterStateSnapshot } from '@angular/router';
import { UsersService } from '../services/users.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})

export class HomeGard implements CanActivate {
  constructor(public usersService: UsersService, public router:Router){}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot):
   Observable<boolean> | Promise<boolean> | boolean {
    if(this.usersService.isAuthenticated){
      this.router.navigate(['/'])
    }
    return true
  }
}
