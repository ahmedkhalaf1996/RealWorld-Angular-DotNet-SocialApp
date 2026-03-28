import { CommentWithUser } from "./Comment";

export interface PostUserInfo {
  name?: string;
  imageUrl?: string;
}

export interface Post {
  _id?: string;
  title?: string;
  message?: string;
  name?: string;
  creator?: string;
  selectedFile?: string;
  likes?: Array<string>;
  comments?: Array<CommentWithUser>;
  createdAt?: Date | string;
  user?: PostUserInfo;
}
