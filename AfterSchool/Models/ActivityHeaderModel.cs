using AfterSchoolLibrary.Models;

namespace AfterSchool.Models
{
    public class ActivityHeaderModel
    {
        public AccountPackage AccountPackage { get; set; }
        public Activity Activity { get; set; }
        public Account CurrentAccount { get; set; }
        public WebUser CurrentUser { get; set; }
        public bool? IsActivityLeader { get; set; }
        public bool? IsFullScreen { get; set; }
    }
}