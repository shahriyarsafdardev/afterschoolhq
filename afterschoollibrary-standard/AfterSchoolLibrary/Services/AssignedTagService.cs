using System.Collections.Generic;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class AssignedTagService
    {
        public static List<AssignedTag> GetByIdByType(int objectId, int objectType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AssignedTag.LoadByObjectIdByObjectType(objectId, objectType, msgs);
        }
    }
}