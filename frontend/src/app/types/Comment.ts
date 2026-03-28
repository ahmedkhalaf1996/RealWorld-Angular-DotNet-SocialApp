export interface CommentUser {
  name?: string;
  imageUrl?: string;
}

export interface CommentWithUser {
 _id?: string;
 postId?: string;
 value?: string;
 createdAt?: Date | string;
 userId?: string;
 user?: CommentUser;
}
