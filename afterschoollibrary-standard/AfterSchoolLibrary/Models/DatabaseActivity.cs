using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Models
{
    public class DatabaseActivity
    {
        public SystemMessages Messages { get; set; }
        public DatabaseConnection Connection { get; set; }
        public WebUser LoggedInUser { get; set; }

        public DatabaseActivity(WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc)
        {
            this.LoggedInUser = loggedInUser;
            this.Messages = msgs;
            this.Connection = dc;
        }
    }
}
