import {ActionResult} from '../models/action-result';
import {ExecuteResponseType} from '../models/response/execute-response';

// @dynamic
export class ActionHelper {
  public static result<X, Y>(completeFn: (result: X) => void, notifyFn: (notification: Y) => void) {
    return (result: ActionResult<X, Y>) => {
      if (result.type === ExecuteResponseType.End) {
        completeFn(result.result);
      } else {
        notifyFn(result.notification);
      }
    };
  }
}
