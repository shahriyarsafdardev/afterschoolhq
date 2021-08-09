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
    public class AccountRelationshipFactory
    {
        public static AccountRelationship Create(Account hostAccount, Account visitorAccount, ApprovalStatus approvalStatus, bool requiresPermissionToPublish, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new AccountRelationship()
            {
                HostAccountId = hostAccount.ID,
                VisitorAccountId = visitorAccount.ID,
                ApprovalStatusId = approvalStatus,
                HostPermissionTypeId = HostPermissionType.ApproveOnly,
                RequiresPublishPermission = requiresPermissionToPublish
            };
            msgs.AddRange(obj.Save(dc));

            return obj;
        }
    }
}
