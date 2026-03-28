import { ChangeDetectorRef, Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Post } from '../../types/Post';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PostsService } from '../../services/posts.service';
import { UsersService } from '../../services/users.service';

@Component({
  selector: 'app-create-post',
  standalone: false,
  templateUrl: './create-post.component.html',
  styleUrl: './create-post.component.css',
})
export class CreatePostComponent {
 constructor(public dialog: MatDialog){}

 openDialog(){
  this.dialog.open(showing ,{
    data: {title:'', message:'', selectedFile:'' as Post,}
  })
 }
}


@Component({
  selector:'showing',
  standalone: false,
  templateUrl: 'showing.html'
})

export class showing {
 base64Output : string = '';

 constructor(
  public dialogRef: MatDialogRef<CreatePostComponent>,
  @Inject(MAT_DIALOG_DATA) public data: Post,
  private _snackBar: MatSnackBar,
  private postsService: PostsService,
  private usersService: UsersService,
  private cdr: ChangeDetectorRef
 ){}

 CreatePost(){
  const {title, message, selectedFile} = this.data;
  if(!title || !message || !selectedFile){
    console.log('error');
    this.openSnackBar('Please Complete The Form Failds')
  } else {
    // create post
    this.postsService.createPost(this.data).subscribe({
      next:(res)=>{
        setTimeout(() => {
          this.postsService.resetFeed();
          this.postsService.fetchPosts(1, this.usersService.UserServiceData._id as string, false)
          .subscribe();
        }, 0);
        this.dialogRef.close();
        this.openSnackBar('Succesfully Creating The post')
      },
      error: ({error})=>{
        this.openSnackBar(error.message)
      }
    })
  }
 }


 openSnackBar(message: string){
  this._snackBar.open(message, 'Okay');
 }

 upload(event: any){
  var file = event.target.files.length;
  for(let i=0;i<file; i++){
    var reader = new FileReader();
    reader.onload = (event: any) => {
      this.data.selectedFile = event.target.result;
      this.cdr.detectChanges();
    }
    reader.readAsDataURL(event.target.files[i]);
  }
 }
}

