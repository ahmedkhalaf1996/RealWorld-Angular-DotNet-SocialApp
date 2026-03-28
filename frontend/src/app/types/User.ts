export interface User {
  _id?: string,
  name?: string,
  email?: string,
  password?: string,
  id?: string,
  imageUrl?: string,
  bio?: string,
  followers?: Array<any>,
  following?: Array<any>,
}

export interface SiginUpUser {
  firstName?: string,
  lastName?: string,
  email?: string,
  password?: string,
}


export interface UserInChat {
  _id?: string,
  id?: string,
  name?: string,
  imageUrl?: string,
  unReadedMessage: number
}
