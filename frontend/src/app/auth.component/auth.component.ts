import { Component } from '@angular/core';
import { SiginUpUser, User } from '../types/User';
import {MatSnackBar} from '@angular/material/snack-bar'
import { UsersService } from '../services/users.service';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
@Component({
  selector: 'app-auth',
  standalone: false,
  templateUrl: './auth.component.html',
  styleUrl: './auth.component.css',
})
export class AuthComponent {
loadLogin:boolean = false;
loadSiginup:boolean = false;

loginForm: User = {
  email: '',
  password:'',
}

SiginUpForm: SiginUpUser = {
  email: '',
  password:'',
  firstName:'',
  lastName:'',
}

constructor(private _snackBar: MatSnackBar,
            private userService: UsersService,
            private router:Router
) {}

private extractErrorMessage(error: any): string {
  return error?.message || error?.error?.message || 'Authentication failed. Please Try Again.';
}

private showSnack(mesasge: string) {
  this._snackBar.open(mesasge,'Dismiss', {duration: 4000})
}

Login(){
  if(this.loadLogin){
    return;
  }

  this.loadLogin = true;
  const {email, password} = this.loginForm;

  if(email=='' || password == ''){
    this.loadLogin = false;
    this.showSnack('Please Complete the Form Correctlly');
   } else {
    this.userService.signIn(this.loginForm)
        .pipe(finalize(()=> {this.loadLogin = false;}))
        .subscribe({
          next: (res) => {
            if(res) {
              this.userService.setAuthFromProfile(res);
              this.userService.isUserAuth();
              this.router.navigate(['/profile/', res?.result?._id])
              console.log(res)
            }
          },
          error:(err) => {
            this.showSnack(this.extractErrorMessage(err?.error ?? err));
          },
        })
   }
}

Register(){
  if(this.loadSiginup){
    return;
  }


  this.loadSiginup = true;
  const {email, password,firstName, lastName} = this.SiginUpForm;

  if(email=='' || password == '' || firstName == '' || lastName == ''){
    this.loadSiginup = false;
    this.showSnack('Please Complete the Form Correctlly');
   } else {
    this.userService.signUp(this.SiginUpForm)
        .pipe(finalize(()=> {this.loadSiginup = false;}))
        .subscribe({
          next:(res)=>{
            console.log('auth res', res)
            this.userService.setAuthFromProfile(res);
            this.userService.isUserAuth();
            this.router.navigate(['/profile/', res?.result?._id])
          },
          error:(err) => {
            this.showSnack(this.extractErrorMessage(err?.error ?? err));
          },
        })
   }

}

}
