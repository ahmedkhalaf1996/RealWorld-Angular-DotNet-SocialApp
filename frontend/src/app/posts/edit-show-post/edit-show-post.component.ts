import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { Post } from '../../types/Post';
import { filter, Subject, takeUntil } from 'rxjs';
import { PostsService } from '../../services/posts.service';
import { ActivatedRoute, Router } from '@angular/router';
import { UsersService } from '../../services/users.service';

@Component({
  selector: 'app-edit-show-post',
  standalone: false,
  templateUrl: './edit-show-post.component.html',
  styleUrl: './edit-show-post.component.css',
})
export class EditShowPostComponent implements OnInit, OnDestroy {
  IS_Edit: boolean = false;
  post:Post = {} as Post;

  if_UserIsTheCreator = false;
  private destory$ = new Subject<void>();

  constructor(
    private postsService: PostsService,
    private route: ActivatedRoute,
    private userS:UsersService,
    private router:Router,
    private cdr: ChangeDetectorRef
  ){}

  ngOnInit(){
    this.postsService.fetchPost(this.route.snapshot.params['id']).subscribe({
      next:({post})=>{
        this.post = post;
        this.refreshCreatorFlag();
        this.cdr.detectChanges();
      }
    })

    this.userS.user$
        .pipe(
          filter((user)=> !!user && !!user._id),
          takeUntil(this.destory$)
        )
        .subscribe(()=>{
          this.refreshCreatorFlag();
          this.cdr.detectChanges();
        });
  }


 upload(event: any){
  var file = event.target.files.length;
  for(let i=0;i<file; i++){
    var reader = new FileReader();
    reader.onload = (event: any) => {
      this.post.selectedFile = event.target.result;
    }
    reader.readAsDataURL(event.target.files[i]);
  }
 }

 UpdateAndSave(){
  this.IS_Edit = false

  if(this.if_UserIsTheCreator){
    this.postsService.udpatePost(String(this.post._id), this.post).subscribe({
      next:(res)=>{
        console.log("up post", res)
      }
    })
  }
 }

Delete(){
  if(this.if_UserIsTheCreator){
    this.postsService.deletePost(String(this.post._id)).subscribe({
      next:(res)=>{
        console.log("delted post", res)
        this.router.navigate(['/'])
      }
    })
  }
}

  private refreshCreatorFlag(){
    if(!this.post?._id){
      return;
    }
    const currentUserId = this.userS.UserServiceData._id;
    this.if_UserIsTheCreator = !!currentUserId && currentUserId === this.post.creator;
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }
}
