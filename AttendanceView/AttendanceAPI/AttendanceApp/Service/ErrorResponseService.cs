using AttendanceApp.ViewModel;

namespace AttendanceApp.Service
{
    public class ErrorResponseService
    {
        public CustomErrorResponseViewModel CreateErrorResponse (int status, string error)
        {
            return new CustomErrorResponseViewModel
            {
                Status = status,
                Error = error,
                Data = null
            };
        }
    }
}
