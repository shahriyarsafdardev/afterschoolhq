using System;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20221119_VolunteerTaskTable
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var webUsers = WebUser.LoadVolunteerWebUsers();
                    webUsers.ForEach(webUser =>
                    {
                        VolunteerTaskService.AddTask(
                                
                            webUser.ID,
                            TaskStatus.ReviewApplicant,
                            ApplicationStatus.New,
                            0,
                            null,
                            msgs
                                
                        );
                    });
                    
                },
                uniqueIdenfier: "Migration_20221119_VolunteerTaskTable",
                msgs: msgs,
                dc: dc
            );
        }
    }
}