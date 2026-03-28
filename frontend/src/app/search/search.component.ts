import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BehaviorSubject, combineLatest, defer, distinctUntilChanged, map, of, shareReplay, Subject, switchMap, takeUntil, tap } from 'rxjs';
import { PostsService } from '../services/posts.service';
import { SearchStateService } from '../services/search-state.service';
import { resolveImageSrc } from '../utils/image';
import { Post } from '../types/Post';
import { User } from '../types/User';

@Component({
  selector: 'app-search',
  standalone: false,
  templateUrl: './search.component.html',
  styleUrl: './search.component.css',
})
export class SearchComponent implements OnInit, OnDestroy {

  private destroy$ = new Subject<void>();
  private userToggleSubject = new BehaviorSubject<boolean | null>(null);
  readonly userToggle$ = this.userToggleSubject.asObservable();


  readonly query$ = defer(()=> this.searchState.query$).pipe(
    map((q)=> (q ?? '').trim()),
    distinctUntilChanged()
  )

  readonly results$ = this.query$.pipe(
    tap(()=> this.userToggleSubject.next(null)),
    switchMap((query)=>{
      if(!query){
        return of({query, posts:[] as Post[], users: [] as User[]})
      }
      return this.postsService.fetchPostsUsersBySearch(query).pipe(
        map((data)=>(
          {
          query,
          posts: (data?.posts ?? []) as Post[],
          users: (data?.user ?? []) as User[]
        }))
      )
    }),
    shareReplay(1)
  )

  readonly vm$ = combineLatest([this.results$, this.userToggle$]).pipe(
    map(([res, toggle])=>{
      const hasPosts = res.posts.length >0;
      const hasUsers = res.users.length > 0;
      const defultShowPosts = hasPosts ? true : false;
      const showPosts = toggle !== null ? toggle : defultShowPosts;
      return {...res, showPosts, hasPosts, hasUsers};
    }
    ))


  constructor(
    private route:ActivatedRoute,
    private postsService: PostsService,
    private searchState: SearchStateService,
  ){}

  resolveImage = resolveImageSrc;

  ngOnInit() {
    this.route.queryParamMap
        .pipe(
          map((params)=> (params?.get('data') as string) ?? ''),
          distinctUntilChanged(),
          takeUntil(this.destroy$)
        )
        .subscribe((query)=>{
          this.searchState.setQuery(query);
        })
  }

  changeSearch(event?: { checked: boolean }) {
    if(event && typeof event.checked === 'boolean') {
      this.userToggleSubject.next(event.checked);
      return;
    }
    const current = this.userToggleSubject.value ?? false;
    this.userToggleSubject.next(!current);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
