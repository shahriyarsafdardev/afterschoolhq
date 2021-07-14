using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class ActivityFormWebUserFactory
    {
        public static ActivityForm Create(Account account, Activity activity, Form form, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityForm() { AccountId = account.ID, ActivityId = activity.ID, FormId = form.ID  };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static ActivityForm CreateActiveForm(Account account, Activity activity, Form form, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new ActivityForm() { AccountId = account.ID, ActivityId = activity.ID, FormId = form.ID, IsActive = true, IsPublished =  true, Name="Test activity form", Description="Test Activity form" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
