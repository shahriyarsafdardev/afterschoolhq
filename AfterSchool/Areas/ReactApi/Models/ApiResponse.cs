using BasicLibrary;

namespace AfterSchool.Areas.ReactApi.Models
{
    public class ApiResponse
    {
        public SystemMessages msgs { get; set; }
        public dynamic obj { get; set; }

        public ApiResponse(SystemMessages msgs, dynamic obj)
        {
            this.msgs = msgs;
            this.obj = obj;
        }
    }
}