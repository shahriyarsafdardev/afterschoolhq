using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;

namespace AfterSchoolLibrary
{
	
    public class ContactList : BasicLookUpObject
    {
		#region "ClassGenerator"

		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		

        public ContactList()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ContactList).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ContactList).Name;
                _dp = BasicLookUpObject.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        protected static ContactList LoadObject(System.Data.DataRow row)
        {
            ContactList obj = new ContactList();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ContactList).Name);
				
                return dq;
            }
        }

        public static ContactList Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ContactList> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ContactList.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ContactList.BaseGetCount(criterias, ObjectMap, dq);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ContactList).Name);
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ContactList).Name);
        }

        public static SystemMessages Create(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return BaseObject._Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();
            BasicLibrary.DataAccess.DatabaseParameters dps;

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ContactList Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ContactList.Load(this.ID.Value, dc);
		}
		
		public static List<ContactList> LoadByAccountId(int AccountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public static List<ContactList> LoadByAccountId(int AccountId, int? start, int? length, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add(ObjectMap["Name"], OrderByDirectionEnum.Ascending);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static ContactList CreateNew(string listname, int accountId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var contactList = new ContactList();
            contactList.Name = listname;
            contactList.CreateDate = contactList.LastModifiedDate = DateTime.UtcNow;
            contactList.CreatedBy = contactList.LastModifiedBy = loggedInUserId;
            contactList.AccountId = accountId;
            msgs.AddRange(contactList.Save(dc));

            return contactList;
        }

        public SystemMessages AddContact(int registrantId, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var clr = ContactListRegistrant.LoadByContactListIdByRegistrantId(this.ID.Value, registrantId, dc);
            if (clr.IsEmpty())
            {
                clr = new ContactListRegistrant();
                clr.ContactListId = this.ID;
                clr.RegistrantId = registrantId;
                clr.CreateDate = DateTime.UtcNow;
                clr.CreatedBy = loggedInUserId;
                msgs.AddRange(clr.Save(dc));
            }

            return msgs;
        }

        public SystemMessages RemoveContact(int registrantId, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var clr = ContactListRegistrant.LoadByContactListIdByRegistrantId(this.ID.Value, registrantId, dc);
            if (clr.IsNotEmpty())
                msgs.AddRange(clr.Delete(dc));

            return msgs;
        }

        public int GetContactCount(DatabaseConnection dc = null)
        {
            return ContactListRegistrant.GetCount(criterias: new Hashtable() { { "ContactListId", this.ID.Value } }, dc: dc);
        }

        public List<Registrant> GetContacts(DatabaseConnection dc = null)
        {
            return ContactListRegistrant.LoadByContactListId(this.ID.Value, dc).Select(x => x.ToRegistrant()).ToList();
        }

        public bool HasContact(Registrant contact, DatabaseConnection dc = null)
        {
            return ContactListRegistrant.GetCount(
                criterias: new Hashtable(){
                    {"RegistrantId", contact.ID.Value},
                    {"ContactListId", this.ID.Value}
                },
                dc: dc
                ) > 0;
        }

        #endregion
    }
}
