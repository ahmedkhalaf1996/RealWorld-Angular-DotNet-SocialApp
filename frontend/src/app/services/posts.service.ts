import { HttpClient, HttpHeaders } from '@angular/common/http';
import { EventEmitter, Injectable } from '@angular/core';
import { environment } from '../../environment/environment';
import { Post } from '../types/Post';
import { EMPTY, Observable, tap } from 'rxjs';

function httpOptions(){
  var opt = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${JSON.parse(localStorage.getItem('profile') as string)?.token}`
    })
  }
  return opt;
}


@Injectable({
  providedIn: 'root',
})
export class PostsService {

    userid : string = JSON.parse(localStorage.getItem('profile') as string)?.result?._id;

    private apiUrl = environment.APIUrl+'posts';

    PostsServiceData:Array<Post>=[]
    FeedUpdate: EventEmitter<Post[]> = new EventEmitter<Post[]>();
    curentPage = 0;
    totalPages = 1;

    constructor(private http:HttpClient){}

    // get one post by id
    fetchPost(id:string): Observable<any> {
      return this.http.get<any>(`${this.apiUrl}/${id}`)
    }

    // fetch posts users by search
    fetchPostsUsersBySearch(searchQuery:string): Observable<{user:[], posts:[]}> {
      return this.http.get<{user:[], posts:[]}>(`${this.apiUrl}/search?searchQuery=${searchQuery}`)
    }

    // fetch multi posts .. home
    fetchPosts(page:number, id:string, append:boolean =true): Observable<any> {
          this.userid = JSON.parse(localStorage.getItem('profile') as string)?.result?._id;
          if(this.userid) {
            const posts = this.http.get<any>(`${this.apiUrl}?page=${page}&id=${id}`).pipe(
              tap((res: {data:Post[]; curentPage?:number; numberOfPages?:number })=>{
                // console.log('API Response:', res);
                const {data, curentPage, numberOfPages} = res;
                if(!append){
                  this.PostsServiceData = [...data];
                } else {
                  this.PostsServiceData = [...this.PostsServiceData, ...data]
                }

                this.FeedUpdate.emit(this.PostsServiceData);
                if(curentPage !== undefined) {
                  this.curentPage = curentPage;
                }
                if(numberOfPages !== undefined){
                  this.totalPages = numberOfPages;
                }
              })
            );
            return posts;
          } else {
            return EMPTY;
          }

    }

    resetFeed(){
      this.PostsServiceData = [];
      this.curentPage = 0;
      this.totalPages = 1;
      this.FeedUpdate.emit(this.PostsServiceData);
    }


    // create post
    createPost(newPost:Post):Observable<Post> {
      return this.http.post<Post>(`${this.apiUrl}`, newPost, httpOptions())
    }

    // comment post
    comment(value:string, id:string): Observable<{ post:Post } | any> {
      return this.http.post<{post: Post}>(`${this.apiUrl}/${id}/commentPost`, { value }, httpOptions())
    }


    // delete comment
    deleteComment(postId:string, commentId:string): Observable< any> {
      return this.http.delete<any>(`${environment.APIUrl}comments/${postId}/comments/${commentId}`, httpOptions())
    }

    // like on post
    likePost(id: any): Observable<any>{
     return this.http.patch<any>(`${this.apiUrl}/${id}/likePost`, {}, httpOptions())
    }

    // update post
    udpatePost( id:string, updatedPost:Post,): Observable<Post> {
      return this.http.patch<Post>(`${this.apiUrl}/${id}`,updatedPost, httpOptions())
    }

    deletePost( id:string): Observable<any> {
      return this.http.delete<any>(`${this.apiUrl}/${id}`, httpOptions())
    }

  }
