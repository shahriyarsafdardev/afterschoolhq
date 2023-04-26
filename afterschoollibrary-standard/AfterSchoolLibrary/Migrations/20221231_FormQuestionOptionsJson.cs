using System.Linq;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Helpers;
using System;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20221231_FormQuestionOptionsJson
    {

        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var sql = "select * from formquestion where options is not null";
                    var dq = new DatabaseQuery();
                    dq.Connection = adc;
                    dq.UseParameterizedQuery = false;

                    var questions = dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(FormQuestion.LoadObject);

                    foreach (var question in questions)
                    {
                        var options = question.Options.Split(',');
                        question.OptionsJson = null;

                        if (question.Options.Contains("|"))
                            try
                            {
                                question.OptionsJson = options.Select(option => new KeyValuePair() { key = option.Split('|')[0], value = option.Split('|')[1] }).ToList().ToStringDictionary().ToJSON();
                            }
                            catch (Exception ex) { }
                        
                        if (question.OptionsJson.IsBlank())
                            question.OptionsJson = options.ToJSON();

                        amsgs.AddRange(question.Save(adc));
                    }
                },
                uniqueIdenfier: "Migration_20221231_FormQuestionOptionsJson",
                msgs: msgs,
                dc: dc
            );
        }
    }
}