import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { User } from '../../types/User';
import { UsersService } from '../../services/users.service';
import { resolveImageSrc } from '../../utils/image';

@Component({
  selector: 'app-update',
  standalone: false,
  templateUrl: './update.component.html',
  styleUrl: './update.component.css',
})
export class UpdateComponent   {
  @Output() ChangeUpdate: EventEmitter<boolean> = new EventEmitter();
  @Input() user:User = {} as User

  constructor (private usersService: UsersService, private cdr: ChangeDetectorRef){}

  resolveImage = resolveImageSrc;

  upload(event: any){
    var file = event.target.files.length;
    for(let i=0; i<file; i++){
      var reader = new FileReader();
      reader.onload = (event: any) => {
        this.user.imageUrl = event.target.result;
        this.cdr.markForCheck();
      }
      reader.readAsDataURL(event.target.files[i]);
    }
  }

  Update(){
    this.usersService.UpdateUser(this.user).subscribe({
      next: (res: any)=> {
        const updated = res?.user ?? res ?? this.user;
        this.usersService.updateUserState(updated);
        this.ChangeUpdate.emit();
      }
    })
  }
}
