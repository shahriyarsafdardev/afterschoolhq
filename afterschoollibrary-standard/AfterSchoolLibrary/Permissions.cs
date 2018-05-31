using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfterSchoolLibrary
{
    public class Permissions
    {
        public static bool HasAccessToFeature(FeatureType ft, UserType userTypeId, PermissionObjectType pot)
        {
            return true;
        }
    }
}
