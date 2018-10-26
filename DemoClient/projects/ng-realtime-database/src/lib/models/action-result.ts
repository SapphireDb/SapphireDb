import {ExecuteResponse, ExecuteResponseType} from './response/execute-response';

export class ActionResult<X, Y> {
  type: ExecuteResponseType;
  result: X;
  notification: Y;

  constructor (response: ExecuteResponse) {
    this.type = response.type;

    if (response.type === ExecuteResponseType.End) {
      this.result = response.result;
    } else {
      this.notification = response.result;
    }
  }
}
