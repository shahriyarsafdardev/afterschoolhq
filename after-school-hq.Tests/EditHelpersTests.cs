using System.Collections.Generic;
using AfterSchool.Areas.Org.Controllers;
using ApprovalTests.Combinations;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace after_school_hq.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class EditHelpersTests
    {
        [Test]
        public void GetNextScreenForActivityTypeTest()
        {
            CombinationApprovals.VerifyAllCombinations(EditHelpers.GetNextScreenForActivityType,
                new List<ActivityType>
                {
                    ActivityType.Activity,
                    ActivityType.Team,
                    ActivityType.FieldTrip,
                    ActivityType.PermissionSlip,
                    ActivityType.LearningPod,
                    ActivityType.VolunteerOpportunity,
                },
                new List<string>
                {
                    EditHelpers.EditSetup,
                    EditHelpers.EditRestrictions,
                    EditHelpers.EditSchedule,
                    EditHelpers.EditDetails,
                    EditHelpers.EditContact,
                    EditHelpers.EditLocation,
                    EditHelpers.EditCost,
                });
        }
    }
}