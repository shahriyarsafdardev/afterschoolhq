using System;
using System.Collections.Generic;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class RegistrantFactory
    {
        public static Registrant Create(SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Registrant() { Email = "test@test.com" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Registrant Create(Account account, bool isMaster = true, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var obj = new Registrant() { Email = "test@test.com", AccountId = account.ID, IsMaster = isMaster };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Registrant CreateParent(Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var obj = new Registrant() { Email = "parent@test.com", AccountId = account.ID, IsMaster = true, IsParentOrGuardian = true };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Registrant CreateStudent(Account account, int parentRegistrantId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var obj = new Registrant() { Email = "student@test.com", AccountId = account.ID, IsMaster = false, ParentRegistrantId = parentRegistrantId, IsParentOrGuardian = false };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Registrant Create(WebUser user, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Registrant() { Email = "test@test.com", WebUserId = user.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Registrant Create(WebUser user, bool isMaster, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Registrant() { Email = "test@test.com", WebUserId = user.ID, IsMaster = isMaster };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Registrant CreateWithNames(Account account, bool isMaster = true, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var obj = new Registrant() { Email = "test@test.com", FirstName = "Test Name" , LastName = "Test Last Name", AccountId = account.ID, IsMaster = isMaster };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
