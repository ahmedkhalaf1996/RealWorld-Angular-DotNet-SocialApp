import { NgModule } from "@angular/core";
import { RouterModule, Routes} from "@angular/router";
import { Home } from "./home/home";
import { AuthComponent } from "./auth.component/auth.component";
import { EditShowPostComponent } from "./posts/edit-show-post/edit-show-post.component";
import { ProfileComponent } from "./profile/profile/profile.component";
import { NotificationComponent } from "./notification/notification.component";
import { ChatComponent } from "./chat/chat.component";
import { SearchComponent } from "./search/search.component";
import { HomeGard } from "./auth.component/auth.gard";
import { AuthGuard } from "./home/home.gard";

const routes: Routes = [
  { path: '', component: Home },
  { path:'auth', component:AuthComponent, canActivate:[HomeGard]},
  { path: 'search', component: SearchComponent },
  { path:'EditShowPost/:id', component:EditShowPostComponent, canActivate:[AuthGuard]},
  { path:'profile/:id', component: ProfileComponent,  canActivate:[AuthGuard] },
  { path: 'notifications', component: NotificationComponent,  canActivate:[AuthGuard] },
  { path: 'chat', component: ChatComponent ,  canActivate:[AuthGuard]}
  // { path: '**', redirectTo: '' }
 ]

 @NgModule({
  imports: [RouterModule.forRoot(routes, {scrollPositionRestoration: 'enabled'})],
  exports: [RouterModule]
 })
 export class AppRoutingModule { }
