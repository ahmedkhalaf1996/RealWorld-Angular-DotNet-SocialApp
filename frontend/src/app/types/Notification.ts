export interface Notification {
  _id?:string,
  deatils?:string,
    mainuid?:string,
    targetid?:string,
    isreded?:boolean,
    createdAt?: Date ,
    user?:{
      name?:string,
      avatar?:string,
      imageUrl?:string,
    }
}
