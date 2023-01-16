using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20230116_FixAlWootenImportData
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var sql = @"
                                select fqa.*
                                from 
	                                FormQuestionAnswer fqa inner join
	                                Registrant r on fqa.RegistrantId = r.ID
                                where 
	                                fqa.CreateDate between '2023-01-07 02:55:00.757' and '2023-01-09 06:45:47.677' and
                                    (
                                        ((fqa.text like '%[[][[]%' or fqa.Text like '%[[]%') and fqa.Text not like '%{%') or 
	                                    (fqa.QuestionDataTypeId = 45 and fqa.Text not like '%[%' and fqa.text not like '%{%' and LEN(fqa.Text) > 0) or
	                                    (fqa.QuestionDataTypeId in (12,24,36) and fqa.Text not like '%[%' and fqa.text not like '%{%' and LEN(fqa.Text) > 0) or
	                                    (fqa.QuestionDataTypeId = 44 and fqa.Text not like '%[%' and fqa.text not like '%{%' and LEN(fqa.Text) > 0)
                                    )";

                    var dq = new DatabaseQuery();
                    dq.Connection = dc;
                    
                    var answers = dq.ExecuteSelect(sql).AsEnumerable().ToList().ConvertAll(FormQuestionAnswer.LoadObject);

                    foreach(var answer in answers)
                    {
                        answer.Text = Helpers.Utilities.GetKeyValuePairs(answer.Text).ToJSON();
                        amsgs.AddRange(answer.Save(adc));
                    }

                },
                uniqueIdenfier: "Migration_20230116_FixAlWootenImportData",
                msgs: msgs,
                dc: dc
            );
        }
    }
}