import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Post } from '../../types/Post';
import { User } from '../../types/User';
import { filter, finalize, Subject, takeUntil } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import { UsersService } from '../../services/users.service';
import { resolveImageSrc } from '../../utils/image';

@Component({
  selector: 'app-profile',
  standalone: false,
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class ProfileComponent implements OnInit, AfterViewInit, OnDestroy {

posts:Array<Post> = []
userData:User = {} as User

edit: boolean = false
isSameUser:boolean = false

isFollow: boolean = false
isLoading = false
hasMore = true
curentPage = 0
totalPages = 1
totalPosts =0

private pageSize = 0
private lastPageSize: number | null = null
private totalCountInFlight = false
private autoLoadScheduled = false
private profileUserId: string | null = null;

@ViewChild('profileEnd') profileEnd?: ElementRef<HTMLDivElement>;
private observer?: IntersectionObserver;
private destroy$ = new Subject<void>();

resolveImage = resolveImageSrc


constructor(private route:ActivatedRoute,
   private usersService: UsersService,
  private cdr: ChangeDetectorRef
){}




ngOnInit(): void {
  this.route.paramMap.subscribe((params)=>{
    const routeId = params.get('id');
    const storedId = JSON.parse(localStorage.getItem('profile') as string)?.result?._id;
    this.profileUserId = routeId ?? storedId ?? null;
    this.resetPagination();
    if(this.usersService.isAuthenticated && this.profileUserId){
      this.loadProfilePage(1);
    }
  });

  this.usersService.user$
      .pipe(
        filter((user) => !!user && !!user._id),
        takeUntil(this.destroy$)
      )
      .subscribe(()=>{
       this.resetPagination();
       if(this.profileUserId){
         this.loadProfilePage(1);
       }
      })
}

ngAfterViewInit(): void {
  if(this.profileEnd){
    this.observer = new IntersectionObserver(
      (entries)=>{
        if(entries[0].isIntersecting){
          this.loadProfilePage(this.curentPage + 1);
        }
      },
      {rootMargin: '200px'}
    );
    this.observer.observe(this.profileEnd.nativeElement);
  }
}

async loadProfilePage(page:number){
    if(this.isLoading || !this.hasMore || !this.profileUserId) {
      return;
    }

    const userId = this.profileUserId;
    this.isLoading = true;
    this.usersService.GetUserData(userId, page)
        .pipe(finalize(()=> {
          this.isLoading = false;
          this.cdr.markForCheck();
          console.log("auto loading")
          this.scheduleAutoLoadMore();
        }))
        .subscribe({
          next:(res) =>{
            const LogedUser =
                  this.usersService.UserServiceData._id ||
                  JSON.parse(localStorage.getItem('profile') as string)?.result?._id;
            this.userData = {
              followers:[],
              following:[],
              ...res.user
            };

            if(res.user._id == LogedUser) {this.isSameUser = true} else {this.isSameUser = false}
            if ( LogedUser && res.user.followers?.find((id: any) => id === LogedUser)){
              this.isFollow = true
            } else {
              this.isFollow = false
            }

            const postsData = res?.posts?.data ?? res?.posts ?? res?.data ?? [];
            if(page === 1) { this.posts = postsData} else {this.posts = [...this.posts, ...postsData]}

            this.curentPage = res?.posts?.curentPage ?? page;
            this.totalPages = res?.posts?.numberOfPages ?? 1;
            this.hasMore = this.curentPage < this.totalPages;

            console.log("res", res)
            this.updateTotalPostsCount(postsData, this.curentPage, this.totalPages)
          }

        })
  }

  private resetPagination(){
    this.posts = [];
    this.curentPage = 0;
    this.totalPages = 1;
    this.hasMore = true;
    this.totalPosts = 0;
    this.pageSize = 0;
    this.lastPageSize = null
    this.totalCountInFlight = false;
    this.autoLoadScheduled = false
  }

  Update(){
    this.edit = !this.edit
  }

  follow(){
    this.isFollow = !this.isFollow

    this.usersService.following(String(this.userData._id)).subscribe({
      next:()=>{
      }
    })
    console.log('follow')
  }

  private updateTotalPostsCount(postsData: Post[], curentPage: number, totalPages: number){
    const dataLength = Array.isArray(postsData) ? postsData.length : 0;
    console.log("postsData",postsData, 'currentpage', curentPage, 'totalPages', totalPages)
    if(totalPages <= 1){
      this.totalPages = dataLength;
      this.lastPageSize = dataLength;
      return;
    }

    if(this.pageSize === 0 && dataLength > 0 ){
      this.pageSize = dataLength;
    }


    if(curentPage === totalPages){
      this.lastPageSize = dataLength;
    }

    if(this.pageSize > 0 && this.lastPageSize !== null) {
      this.totalPosts = (totalPages -1) * this.pageSize + this.lastPageSize;
      return;
    }

    this.prefetchLastPageSize(totalPages);
  }


  private prefetchLastPageSize(totalPages: number){
    if(this.totalCountInFlight || this.lastPageSize !== null || !this.profileUserId || this.pageSize === 0){
      return;
    }

    this.totalCountInFlight = true;
    const userId = this.profileUserId;
    this.usersService.GetUserData(userId, totalPages)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next:(res)=>{
            const postsData = res?.posts?.data ?? res?.posts ?? res?.data ?? [];
            this.lastPageSize = Array.isArray(postsData) ? postsData.length : 0;
            if(this.pageSize > 0){
              this.totalPosts = (totalPages - 1) * this.pageSize + this.lastPageSize;
            } else {
              this.totalPosts = this.posts.length;
            }
            this.totalCountInFlight = false;
            this.cdr.markForCheck();
          },
          error:()=>{
            this.totalCountInFlight = false;
          }
        });
  }


  private scheduleAutoLoadMore(){
    if(this.autoLoadScheduled || this.isLoading || !this.hasMore){
      return;
    }
    this.autoLoadScheduled = true;
    requestAnimationFrame(()=>{
      this.autoLoadScheduled = false;
      if(this.isLoading || !this.hasMore){
        return;
      }
      if(this.isSentinelVissible()){
        console.log('check ooaing profile page', this.curentPage + 1)
        this.loadProfilePage(this.curentPage + 1)
      }
    })
  }

  private isSentinelVissible(): boolean {
    const sentinel = this.profileEnd?.nativeElement;
    if(!sentinel){
      return false;
    }
    const rect = sentinel.getBoundingClientRect();
    const viewHeight = window.innerHeight || document.documentElement.clientHeight;
    return rect.top <= viewHeight + 200;
  }
  ngOnDestroy(): void {
    this.observer?.disconnect();
    this.destroy$.next()
    this.destroy$.complete();
  }
}
