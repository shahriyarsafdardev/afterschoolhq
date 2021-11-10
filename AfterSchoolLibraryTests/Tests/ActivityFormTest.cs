using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using AfterSchoolLibraryTests.Factories;

namespace AfterSchoolLibraryTests.Tests
{
    public class ActivityFormTest : BaseTest
    {
        private WebUser user;
        private Account account;
        private Activity activity;
        private Form form;
        private ActivityForm activityForm;

        public ActivityFormTest(ITestOutputHelper output) : base(output)
        {
            user = Factories.WebUserFactory.Create(msgs, dc);
            account = Factories.AccountFactory.Create(user, msgs, dc);
            activity = Factories.ActivityFactory.Create(account, msgs, dc);
            form = Factories.FormFactory.Create(account, msgs, dc);
            activityForm = Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc);
        }

        /// <summary>
        /// loads an ActivityForm from the database, in order to properly create an activity form
        /// it is dependant on having an account ID, ab activity ID and a form ID
        /// </summary>
        [Fact]
        public void LoadTest()
        {
            Assert.NotNull(Activity.Load(activity.ID.Value, dc: dc));
        }
        [Fact]
        public void CopyFormTest()
        {
            var copyForm = form.Copy(user.ID.Value, account.ID.Value, FormType.Survey, "Test Survey Form", msgs, dc);

            Assert.True(copyForm.FormTypeId == FormType.Survey);
            Assert.True(copyForm.Name == "Test Survey Form");
        }
        /// <summary>
        /// Gets the number of ActivityForms in the DB
        /// </summary>
        [Fact]
        public void GetCountTest()
        {
            var startCount = ActivityForm.GetCount(msgs: msgs, dc: dc);
            // should equal to 1
            Assert.Equal(1, startCount);
      
            Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc);
            Factories.ActivityFormWebUserFactory.Create(account, activity, form, msgs, dc);
            // should equal to 3
            Assert.Equal(3, ActivityForm.GetCount(msgs: msgs, dc: dc));
        }
        /// <summary>
        /// Saves the ActivityForm in the DB, returns messages that will send errors if this is not recorded 
        /// in the DB properly
        /// </summary>
        [Fact]
        public void SaveTest()
        {
            Assert.True(activityForm.Save(dc: dc).HasNoErrors());
        }
        /// <summary>
        /// Removes the ActivityForm from the DB, it should not be able to be loaded from the DB afterwards
        /// </summary>
        [Fact]
        public void DeleteTest()
        {
            activityForm.Delete(dc: dc);

            Assert.Null(ActivityForm.Load(activityForm.ID.Value, dc: dc));
        }

        /// <summary>
        /// Loads the activity form from the DB (using it's ID) and returns it.
        /// </summary>
        [Fact]
        public void ReloadTest()
        {
            Assert.NotNull(activityForm.Reload(dc: dc));
        }

        /// <summary>
        /// Loads an activity form by account id
        /// </summary>
        [Fact]
        public void LoadByAccountIdTest()
        {
            Assert.Single(ActivityForm.LoadByAccountId(account.ID.Value, dc: dc));
        }

        /// <summary>
        /// Loads an activity form by form id
        /// </summary>
        [Fact]
        public void LoadByFormIdTest()
        {
            Assert.Single(ActivityForm.LoadByFormId(form.ID.Value, msgs, dc: dc));
        }

        /// <summary>
        /// Loads an activity form by activity id
        /// </summary>
        [Fact]
        public void LoadByActivityIdTest()
        {
            Assert.Single(ActivityForm.LoadByActivityId(activity.ID.Value, dc: dc));
        }

        /// <summary>
        /// Loads an activity form by UID
        /// </summary>
        [Fact]
        public void LoadByUidTest()
        {
            Assert.NotNull(ActivityForm.LoadByUid(activityForm.Uid, dc: dc));
        }

        /// <summary>
        /// Loads an activity form by activity id and form id
        /// </summary>
        [Fact]
        public void LoadByActivityIdByFormIdTest()
        {
            Assert.NotNull(ActivityForm.LoadByActivityIdByFormId(activity.ID.Value, form.ID.Value, msgs, dc: dc));
        }

        /// <summary>
        /// Loads an active activity form by activity id
        /// </summary>
        [Fact]
        public void LoadByActivityIdByIsActiveTest()
        {
            activityForm.IsActive = true;
            activityForm.Save(dc: dc);

            Assert.NotNull(ActivityForm.LoadByActivityIdByIsActive(activity.ID.Value, true, msgs, dc: dc));
        }

        /// <summary>
        /// Loads a published activity form by UID
        /// </summary>
        [Fact]
        public void LoadByUidByIsPublishedTest()
        {
            activityForm.IsPublished = true;
            activityForm.Save(dc: dc);

            Assert.NotNull(ActivityForm.LoadByUidByIsPublished(activityForm.Uid, true, msgs, dc: dc));
        }
    }
}
