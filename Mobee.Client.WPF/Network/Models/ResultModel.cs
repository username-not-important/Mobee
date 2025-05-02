using System.ComponentModel.DataAnnotations.Schema;

namespace Mobee.Client.WPF.Network.Models
{
    public class ResultModel
    {
        public bool Success { get; set; }

        public string Error { get; set; }

        public static ResultModel ErrorResult(string error)
        {
            return new ResultModel()
            {
                Success = false,
                Error = error
            };
        }
        
        public static ResultModel DbContextErrorResult()
        {
            return new ResultModel()
            {
                Success = false,
                Error = "Error Prevented Saving DataContext"
            };
        }
    }
    
    public class ResultModel<T>
    {
        public bool Success { get; set; }

        public string Error { get; set; }

        public T Result { get; set; }

        public static ResultModel<T> ErrorResult(string error)
        {
            return new ResultModel<T>()
            {
                Success = false,
                Error = error
            };
        }

        public static ResultModel<T> DbContextErrorResult()
        {
            return new ResultModel<T>()
            {
                Success = false,
                Error = "Error Prevented Saving DataContext."
            };
        }



    }
}
