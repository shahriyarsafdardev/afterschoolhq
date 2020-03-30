using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class SubscriberService
    {
        public static Subscriber CreateNew(Account accountToFollow, WebUser user, SubscriberStatus status, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var subscriber = new Subscriber()
            {
                AccountId = accountToFollow.ID,
                WebUserId = user.ID,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser.ID,
                LastModifiedBy = loggedInUser.ID,
                LastModifiedDate = DateTime.UtcNow,
                SubscriberStatusId = status,
            };

            msgs.AddRange(subscriber.Save(dc));

            return subscriber;
        }

        public static void UpdateSubscriber(this Subscriber subscriber, SubscriberStatus status, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            subscriber.SubscriberStatusId = status;
            subscriber.LastModifiedDate = DateTime.UtcNow;
            subscriber.LastModifiedBy = loggedInUser.ID;
            msgs.AddRange(subscriber.Save(dc));
        }
    }
}
