export interface Message {
  _id?: string,
  content?: string,
  sender?: string,
  recever?: string,
  clientId?: string,
  status?: 'pending' | 'sent' | 'failed',
  localCreatedAt?: number,
}

export interface UnrededMsg {
  _id?: string,
  mainUserid?: string,
  otherUserid?: string,
  numOfUnreadedMessages: number,
  isReaded?: boolean,
}
