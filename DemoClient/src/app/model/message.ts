export interface Message {
  id?: number;
  createdOn?: string;
  updatedOn?: string;
  userId?: string;
  toId: string;
  content: string;
}
