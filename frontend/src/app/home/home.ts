import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Post } from '../types/Post';
import { filter, finalize, Subject, takeUntil } from 'rxjs';
import { PostsService } from '../services/posts.service';
import { UsersService } from '../services/users.service';
import { SoundService } from '../services/sound.service';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home implements OnInit, AfterViewInit, OnDestroy {

  arr:Array<Post> = []
  posts: Post[] = [];
  isLoading = false;
  hasMore = true;

  @ViewChild('feedEnd') feedEnd?: ElementRef<HTMLDivElement>;

  private observer?: IntersectionObserver;
  private destroy$ = new Subject<void>();

  constructor(public postsHomeService:PostsService,
    public userHomeService: UsersService,
    private cdr: ChangeDetectorRef
    ){
    this.arr = this.postsHomeService.PostsServiceData
  }


  async ngOnInit(): Promise<void> {
    this.postsHomeService.resetFeed();

    this.userHomeService.user$.pipe(
      filter((user) => !!user && !!user._id),
      takeUntil(this.destroy$)
    ).subscribe(async () =>{
      this.postsHomeService.resetFeed();
      await this.loadNextPage();
    });

    this.postsHomeService.FeedUpdate
        .pipe(takeUntil(this.destroy$))
        .subscribe((posts)=>{
          this.posts = [...posts];
          this.cdr.markForCheck();
        })
  }

  ngAfterViewInit(): void {
    if (this.feedEnd) {
      this.observer = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting) {
            this.loadNextPage();
          }
        },
        { rootMargin: '200px' }
      )
      this.observer.observe(this.feedEnd.nativeElement);
    }
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
    this.destroy$.next();
    this.destroy$.complete();
  }

  async loadNextPage(){
    if(this.isLoading || !this.hasMore || !this.userHomeService.isAuthenticated) {
      return;
    }

    const nextPage = this.postsHomeService.curentPage + 1;
    this.isLoading = true;
    this.postsHomeService.fetchPosts(nextPage, this.userHomeService.UserServiceData._id as string, true)
        .pipe(finalize(()=> this.isLoading =false))
        .subscribe({
          next:({curentPage, numberOfPages}) =>{
             if(curentPage !== undefined){
               this.postsHomeService.curentPage = curentPage;
             }
             if(numberOfPages !== undefined) {
              this.postsHomeService.totalPages = numberOfPages;
              this.hasMore = curentPage < numberOfPages;
             }
             this.posts = [...this.postsHomeService.PostsServiceData];
             this.cdr.markForCheck();
            //  console.log('Loaded page:', currentPage, 'Total pages:', numberOfPages);
          }

        })
  }

}
