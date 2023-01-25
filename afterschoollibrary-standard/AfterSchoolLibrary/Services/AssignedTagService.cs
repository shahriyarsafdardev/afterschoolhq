using System.Collections.Generic;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class AssignedTagService
    {
        public static SystemMessages Save(string tag, int objectID, int objectType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var assignedTag = new AssignedTag();
            
            assignedTag.Tag = tag;
            assignedTag.ObjectId = objectID;
            assignedTag.ObjectType = (ObjectType)objectType;
            
            msgs.AddRange(assignedTag.Save(dc));
            
            return msgs;
        }
        
        public static int Remove(string tag, int objectID, int objectType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AssignedTag.Delete(tag, objectID, objectType, msgs);
        }
        
        public static List<AssignedTag> GetByIdByType(int objectId, int objectType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AssignedTag.LoadByObjectTypeObjectID(objectId, objectType, msgs);
        }
        
    }
}