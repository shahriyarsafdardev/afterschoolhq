namespace AfterSchoolLibrary.Models
{
    public class Permissions
    {
        public static bool HasPermission(UserType userType, PermissionType permissionType)
        {
            switch(userType)
            {
                case UserType.Owner:
                case UserType.Administrator:
                    return true;
                case UserType.Instructor:
                    switch (permissionType)
                    {
                        case PermissionType.ContactsEdit:
                        case PermissionType.ContactsManage:
                        case PermissionType.ContactsView:
                        case PermissionType.ActivitiesEdit:
                        case PermissionType.ActivitiesManage:
                        case PermissionType.ActivitiesView:
                        case PermissionType.StudentsEdit:
                        case PermissionType.StudentsManage:
                        case PermissionType.StudentsView:
                        case PermissionType.LeaguesEdit:
                        case PermissionType.LeaguesManage:
                        case PermissionType.LeaguesView:
                            return true;
                            
                    }
                    return false;
                case UserType.ProgramDirector:
                    switch(permissionType)
                    {
                        case PermissionType.AccountingEdit:
                        case PermissionType.AccountingManage:
                        case PermissionType.AccountingView:
                        case PermissionType.SessionsEdit:
                        case PermissionType.SessionsManage:
                        case PermissionType.SessionsView:
                        case PermissionType.FormsEdit:
                        case PermissionType.FormsManage:
                        case PermissionType.FormsView:
                        case PermissionType.TeamEdit:
                        case PermissionType.TeamManage:
                        case PermissionType.TeamView:
                        case PermissionType.ContactsEdit:
                        case PermissionType.ContactsManage:
                        case PermissionType.ContactsView:
                        case PermissionType.ActivitiesEdit:
                        case PermissionType.ActivitiesManage:
                        case PermissionType.ActivitiesView:
                        case PermissionType.StudentsEdit:
                        case PermissionType.StudentsManage:
                        case PermissionType.StudentsView:
                        case PermissionType.LeaguesEdit:
                        case PermissionType.LeaguesManage:
                        case PermissionType.LeaguesView:
                            return true;
                    }
                    return false;
            }

            return false;
        }
    }
}
