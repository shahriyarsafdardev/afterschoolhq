using System;
using System.Collections;
using System.Collections.Generic;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220603_AddDistrictofColumbiaToStateOptions {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    using (var connection = new SqlConnection(adc.ConnectionString))
                    {
                        var stateQuestions = FormQuestion.LoadByQuestionDataTypeId(new List<QuestionDataType>
                        {
                            QuestionDataType.OrganizationState,
                            QuestionDataType.Parent1State,
                            QuestionDataType.Parent2State,
                            QuestionDataType.RegistrantState
                        },amsgs, adc);
                        
                        stateQuestions.ForEach(question =>
                        {
                            question.Options = Utilities.GetStateList();
                            amsgs.AddRange(question.Save(adc));
                        });
                    }
                },
                uniqueIdenfier: "Migration_20220603_AddDistrictofColumbiaToStateOptions",
                msgs: msgs,
                dc: dc
            );
        }
    }
}