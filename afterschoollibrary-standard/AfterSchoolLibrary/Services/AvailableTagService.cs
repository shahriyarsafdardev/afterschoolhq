using System.Collections.Generic;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class AvailableTagService
    {
        public static List<AvailableTag> GetByObjectType(int objectType, Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AvailableTag.LoadByAccountIdByObjectType((int)account.ID ,objectType, msgs);
        }

        public static bool IsUniqueTag(string tag, int objectType, Account account, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return AvailableTag.ExistsByTag(tag, objectType, (int)account.ID, msgs);
        }
        
        
    }
}