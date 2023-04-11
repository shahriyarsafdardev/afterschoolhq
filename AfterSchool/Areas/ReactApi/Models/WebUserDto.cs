using AfterSchoolLibrary.Models;

namespace AfterSchool.Areas.ReactApi.Models
{
    public class WebUserDto
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string userName { get; set; }

        public WebUserDto(WebUser currentUser)
        {
            this.firstName = currentUser.FirstName;
            this.lastName = currentUser.LastName;
            this.email = currentUser.Email;
            this.phone = currentUser.Phone;
            this.userName = currentUser.Username;
        }

    }
}