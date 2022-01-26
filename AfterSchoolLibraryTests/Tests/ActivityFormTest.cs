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

        [Fact]
        public void SaveTest()
        {
            Assert.True(activityForm.Save(dc: dc).HasNoErrors());
        }

        [Fact]
        public void DeleteTest()
        {
            activityForm.Delete(dc: dc);

            Assert.Null(ActivityForm.Load(activityForm.ID.Value, dc: dc));
        }

        [Fact]
        public void ReloadTest()
        {
            Assert.NotNull(activityForm.Reload(dc: dc));
        }

        [Fact]
        public void LoadByAccountIdTest()
        {
            Assert.Single(ActivityForm.LoadByAccountId(account.ID.Value, dc: dc));
        }

        [Fact]
        public void LoadByFormIdTest()
        {
            Assert.Single(ActivityForm.LoadByFormId(form.ID.Value, msgs, dc: dc));
        }

        [Fact]
        public void LoadByActivityIdTest()
        {
            Assert.Single(ActivityForm.LoadByActivityId(activity.ID.Value, dc: dc));
        }

        [Fact]
        public void LoadByUidTest()
        {
            Assert.NotNull(ActivityForm.LoadByUid(activityForm.Uid, dc: dc));
        }

        [Fact]
        public void LoadByActivityIdByFormIdTest()
        {
            Assert.NotNull(ActivityForm.LoadByActivityIdByFormId(activity.ID.Value, form.ID.Value, msgs, dc: dc));
        }

        [Fact]
        public void LoadByActivityIdByIsActiveTest()
        {
            activityForm.IsActive = true;
            activityForm.Save(dc: dc);

            Assert.NotNull(ActivityForm.LoadByActivityIdByIsActive(activity.ID.Value, true, msgs, dc: dc));
        }

        [Fact]
        public void LoadByUidByIsPublishedTest()
        {
            activityForm.IsPublished = true;
            activityForm.Save(dc: dc);

            Assert.NotNull(ActivityForm.LoadByUidByIsPublished(activityForm.Uid, true, msgs, dc: dc));
        }
    }
}
