using System.Collections.Generic;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class AvailableTagService
    {
        public static List<AvailableTag> GetByObjectType(int objectType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AvailableTag.LoadByObjectType(objectType, msgs);
        }
        
        public static AvailableTag GetById(int? Id, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AvailableTag.LoadById(Id, msgs);
        }
        
        public static SystemMessages Save(int objectType, string tag, Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var availableTag = new AvailableTag();
            
            availableTag.Tag = tag;
            availableTag.AccountId = (int)account.ID;
            availableTag.ObjectType = (ObjectType)objectType;
            availableTag.IsDeleted = false;
            
            msgs.AddRange(availableTag.Save(dc));

            return msgs;
        }
        
        public static SystemMessages Delete(AvailableTag availableTag, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            availableTag.IsDeleted = true;
            
            msgs.AddRange(availableTag.Save(dc));

            return msgs;
        }
        
        public static bool UniqueTag(string tag, int objectType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AvailableTag.UniqueTag(tag, objectType, msgs);
        }
        
    }
}