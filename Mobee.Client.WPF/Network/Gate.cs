using System;
using System.Threading.Tasks;
using Flurl.Http;
using Mobee.Client.WPF.Logs;
using Mobee.Client.WPF.Network.Models;
using Polly;

namespace Mobee.Client.WPF.Network
{

    public static class Gate
    {
        private static string __commError = "Comm Error";

        public static event Action CommunicationFailed;
        public static event Action CommunicationSuccess;

        internal static void onCommunicationFailed()
        {
            CommunicationFailed?.Invoke();
        }
        internal static void onCommunicationSuccess()
        {
            CommunicationSuccess?.Invoke();
        }

        public static ResultModel Use(Func<Task<IFlurlResponse>> call, Action onFailure = null)
        {
            var policy = Policy.Handle<Exception>(onException).Retry(1);

            return policy.ExecuteAndCapture(() => use(call, onFailure)).Result;
        }

        public static ResultModel<T> Use<T>(Func<Task<IFlurlResponse>> call, Action onFailure = null)
        {
            var policy = Policy<ResultModel<T>>.Handle<Exception>(onException).Retry(1);

            return policy.ExecuteAndCapture(() => use<T>(call, onFailure)).Result;
        }

        private static bool onException(Exception exception)
        {
            Logger.Instance.Log("Gate Error:\r\n" + exception, Logger.CH_COMMS);

            //TODO: more exception kinds
            if (exception is TimeoutException ||
                exception is AggregateException)
            {
                Gate.onCommunicationFailed();
                return true;
            }

            return false;
        }

        private static ResultModel use(Func<Task<IFlurlResponse>> call, Action onFailure = null)
        {
            bool success = false;
            try
            {
                var task = call();
                Task.WaitAll(task);

                var result = task.Result;
                if (result.StatusCode == 200)
                {
                    success = true;
                    onCommunicationSuccess();

                    return new ResultModel() { Success = true };
                }
            }
            catch (Exception e)
            {
                //Logger.Instance.Log("Gate Internal Error:\r\n" + e, Logger.CH_NETWORK);

                success = false;

                return ResultModel.ErrorResult(__commError);
            }
            finally
            {
                if (!success)
                {
                    onFailure?.Invoke();
                    onCommunicationFailed();
                }
            }

            return ResultModel.ErrorResult("");
        }

        private static ResultModel<T> use<T>(Func<Task<IFlurlResponse>> call, Action onFailure = null)
        {
            bool success = false;
            try
            {
                var task = call();
                Task.WaitAll(task);

                var result = task.Result;
                if (result.StatusCode == 200)
                {
                    success = true;
                    onCommunicationSuccess();

                    var json = result.GetJsonAsync<ResultModel<T>>();
                    json.Wait();

                    return json.Result;
                }
            }
            catch (Exception e)
            {
                //Logger.Instance.Log("Gate Internal Error:\r\n" + e, Logger.CH_NETWORK);

                success = false;

                return ResultModel<T>.ErrorResult(__commError);
            }
            finally
            {
                if (!success)
                {
                    onFailure?.Invoke();
                    onCommunicationFailed();
                }
            }

            return ResultModel<T>.ErrorResult(__commError);
        }

    }
}
