using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Models
{
    public class DatabaseActionHelper
    {
        public SystemMessages Messages { get; set; }
        public DatabaseConnection Connection { get; set; }
        public WebUser LoggedInUser { get; set; }

        public DatabaseActionHelper(WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc)
        {
            this.LoggedInUser = loggedInUser ?? new WebUser() { ID = 0 };
            this.Messages = msgs ?? new SystemMessages();
            this.Connection = dc;
        }

        public void AddMessages(SystemMessages msgs)
        {
            this.Messages.AddRange(msgs);
        }

        public void AddMessage(SystemMessage msg)
        {
            this.Messages.Add(msg);
        }
    }
}
