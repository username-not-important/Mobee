using Flurl.Http;

namespace Mobee.Client.WPF.Network
{

    public static class ClientHelper
    {

#if DEBUG
        private static string _baseUrl = "https://localhost:7016";
#else
    private static string _baseUrl = "https://mobees.ir";
#endif

        private static string _apiUrl = "/{0}Api/{1}";
        
        private static string Action(string controller, string action) => string.Format(_baseUrl + _apiUrl, controller, action);

        private static IFlurlRequest DefaultHeaders(this string url) => url.WithHeader("X-Requested-With", "XMLHttpRequest").WithTimeout(10);
        private static IFlurlRequest AuthorizedHeaders(this string url, string t) => url.WithHeader("AppAuthorization", t).WithHeader("X-Requested-With", "XMLHttpRequest").WithTimeout(10);

        //public static IFlurlRequest LoginRequest => Action("Account", "Login").DefaultHeaders();

        //public static Func<string, IFlurlRequest> SyncPingRequest => t => Action("Synchronization", "Ping").AuthorizedHeaders(t);
        //public static Func<string, IFlurlRequest> PatientRecordsByPhoneNumberRequest => t => Action("Records", "MemberRecordsByPhoneNumber").AuthorizedHeaders(t);
        //public static Func<string, IFlurlRequest> PatientRecordsByIdRequest => t => Action("Records", "MemberRecordsById").AuthorizedHeaders(t);
    }
}
