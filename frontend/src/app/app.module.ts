import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import { MaterialDesignModule } from '../material-design/material-design.module';
import { HttpClientModule } from '@angular/common/http';
import { Home } from './home/home';
import { AppComponent } from './app.components';
import { AppRoutingModule } from './app-routing.module';
import { AuthComponent } from './auth.component/auth.component';
import { ConfirmDialogComponent } from './confirm-dialog.component/confirm-dialog.component';
import { PostComponent } from './posts/post/post.component';
import { CreatePostComponent, showing } from './posts/create-post/create-post.component';
import { EditShowPostComponent } from './posts/edit-show-post/edit-show-post.component';
import { MatDialogContent } from "@angular/material/dialog";
import { ProfileComponent } from './profile/profile/profile.component';
import { UpdateComponent } from './profile/update/update.component';
import { NotificationComponent } from './notification/notification.component';
import { ChatComponent } from './chat/chat.component';
import { NavbarComponent } from './navbar/navbar.component';
import { SearchComponent } from './search/search.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { RightbarComponent } from './rightbar/rightbar.component';


@NgModule({
  imports: [
    FormsModule,
    ReactiveFormsModule,
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    BrowserAnimationsModule,
    MaterialDesignModule,
    MatDialogContent
],
  declarations: [
    Home,
    AuthComponent,
    AppComponent,
    ConfirmDialogComponent,
    PostComponent,
    ProfileComponent,
    UpdateComponent,
    CreatePostComponent,
    showing,
    CreatePostComponent,
    EditShowPostComponent,
    NotificationComponent,
    ChatComponent,
    NavbarComponent,
    SearchComponent,
    SidebarComponent,
    RightbarComponent,
  ],
  providers: [],
  bootstrap: [AppComponent]
})

export class AppModule { }
