import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { Post } from '../../types/Post';
import { User } from '../../types/User';
import { Router } from '@angular/router';
import { UsersService } from '../../services/users.service';
import { PostsService } from '../../services/posts.service';
import { MatDialog } from '@angular/material/dialog';
import { resolveImageSrc } from '../../utils/image';
import { CommentWithUser } from '../../types/Comment';
import { ConfirmDialogComponent } from '../../confirm-dialog.component/confirm-dialog.component';

@Component({
  selector: 'app-post',
  standalone: false,
  templateUrl: './post.component.html',
  styleUrl: './post.component.css',
})
export class PostComponent implements OnInit, OnChanges {

  @Input() IsShowEdit: boolean = false;
  @Input() inProfile: boolean = false;
  @Input() post: Post = {} as Post;

  CreatorUser:User = {}

  isLoved:Boolean = false;
  AddComment: string = '';
  showAllComments = false;
  isSubmittingComment = false;

  constructor(
    private router: Router,
    private usersService: UsersService,
    private postsService: PostsService,
    private dialog: MatDialog
  ){}


  ngOnInit(): void {
    this.hydrateCreator();
    if(this.IsShowEdit) {
      this.showAllComments = true;
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if(changes['post'] && !changes['post'].firstChange) {
      this.hydrateCreator()
    }
  }

  resolveImage = resolveImageSrc;
  get postImageSrc(): string {
    return resolveImageSrc(this.post?.selectedFile ?? '', '');
  }


  IsUserLiked(){
    const likes = this.post.likes ?? [];
    const currentUserId = this.usersService.UserServiceData._id || JSON.parse(localStorage.getItem('profile') as string)?.result?._id;
    this.isLoved = likes.includes(String(currentUserId))
    console.log('IsUserLiked', this.isLoved)
  }

  private hydrateCreator(){
    const { creator } = this.post;
    if(this.post.user){
      this.CreatorUser = { name: this.post.user.name, imageUrl: this.post.user.imageUrl};
      this.IsUserLiked();
      return;
    }

    if(creator) {
      this.usersService.GetUserData(String(creator)).subscribe({
        next:({user}) =>{
          this.CreatorUser = user;
          this.IsUserLiked()
        }
      })
    }
  }


  // GoToDeatils allow us to navigate to editshowpost compnnt..
  GotoDeatils(){
    this.router.navigate(['/EditShowPost' +  `/${this.post?._id}`])
  }
  // addCommentToPost
  addCommentToPost(){
    const value = this.AddComment?.trim();
    if(!value || value.length === 0 || !this.post?._id){
      return;
    }

    const currentUserId = this.usersService.UserServiceData._id || JSON.parse(localStorage.getItem('profile') as string)?.result?._id;
    const currentUserName = this.usersService.UserServiceData.name || 'You';
    const currentUserImage = this.usersService.UserServiceData.imageUrl || '';

    const tempId = `tmp-${Date.now()}`;
    const optimisiticComment: CommentWithUser = {
      _id: tempId,
      postId: this.post._id,
      value,
      createdAt: new Date(),
      userId: currentUserId,
      user : {name: currentUserName, imageUrl: currentUserImage}
    };
    const previousComments = [...this.commentsList];
    this.post = {
      ...this.post,
      comments: [optimisiticComment, ...previousComments]
    }

    this.AddComment = '';
    this.isSubmittingComment = true;

    this.postsService.comment(value, String(this.post?._id)).subscribe({
      next:({post})=>{
        if(post?.comments){
        this.post = {...this.post, ...post, comments: post.comments};
        }
      this.isSubmittingComment = false;

      },
      error:()=>{
        this.post = {...this.post, comments: previousComments};
        this.AddComment = value;
        this.isSubmittingComment = false;
      }
    })
  }


  // LovePost .. this works love or un love ..
  LovePost(){
    const currentUserId = this.usersService.UserServiceData._id ||
                          JSON.parse(localStorage.getItem('profile') as string)?.result?._id;
    if(!currentUserId || !this.post?._id){
      return;
    }

    const likesSet = new Set(this.post.likes ?? []);
    const shouldLike = !likesSet.has(currentUserId);
    if(shouldLike) {
      likesSet.add(currentUserId);
    } else {
      likesSet.delete(currentUserId)
    }

    this.post = {...this.post, likes: Array.from(likesSet)};
    this.isLoved = shouldLike;

    this.postsService.likePost(this.post._id).subscribe({
      next:(res: any)=>{
        const updated = res?.post ?? res?.updatedPost ?? res?.data ?? res;
        if(updated && updated._id){
          this.post = updated;
        }
        this.IsUserLiked()

      },
      error:()=>{
        if(shouldLike){
          likesSet.delete(currentUserId);
        } else {
          likesSet.add(currentUserId);
        }
        this.post = {...this.post, likes: Array.from(likesSet)};
        this.isLoved = !shouldLike;
      }
    })


  }
  // commentList ... return comemnt with user data correct sync data..
  get commentsList(): CommentWithUser[] {
    return this.post.comments ?? [];
  }
  // visibleComments
  get visibleComments(): CommentWithUser[]{
    if(this.showAllComments){
      return this.commentsList;
    }
    return this.commentsList.slice(0,2);
  }
  // toggleComments
  toogleComments(){
    this.showAllComments = !this.showAllComments;
  }
  // canDeleteComment ..
  canDeleteComment(comment: CommentWithUser): boolean {
    const currentUserId = this.usersService.UserServiceData._id ||
                          JSON.parse(localStorage.getItem('profile') as string)?.result?._id;
    return comment.userId === currentUserId || this.post.creator === currentUserId;
  }
  // deleteComment ...
  deleteComment(comment: CommentWithUser){
    if(!this.post._id || !comment._id){
      return;
    }
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data:{
        title: 'Delete Comment',
        message: 'Are you sure you want to delete this comment?',
        confirmText: 'Delete',
        cancelText:'Cancel'
      }
    });

    // start call api
    dialogRef.afterClosed().subscribe((confiremed: boolean)=>{
      if(!confiremed){
        return;
      }
      const previousComments = [...this.commentsList];
      this.post = {
        ...this.post,
        comments: previousComments.filter(c => c._id !== comment._id)
      };
      // call api
      this.postsService.deleteComment(this.post._id as string, comment._id as string).subscribe({
        next:()=>{},
        error:()=>{
          this.post = {...this.post, comments: previousComments};
        }
      })
    })
  }

}
