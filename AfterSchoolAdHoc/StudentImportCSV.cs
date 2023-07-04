using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Data;

namespace AfterSchoolAdHoc;

internal class StudentImportCSV
{
    public static void Run(SystemMessages msgs, DatabaseConnection dc)
    {
        string sql =
            @"select s.ID,s.first_name,s.last_name,s.email, s.DOB, s.Gender,s.Student_street, s.Student_zip, r.ID as RegistrantId from Registrant as r 
              inner join StudentImportCSV as s on r.FirstName = s.first_name and r.LastName = s.last_name";

        var dq = new DatabaseQuery();
        dq.Connection = dc;
        var dt = dq.ExecuteSelect(sql, msgs);

        var studentList = (from DataRow row in dt.Rows
            select new StudentImportDto
            {
                ID = Convert.ToInt32(row["ID"]),
                FirstName = row["first_name"].ToString(),
                LastName = row["last_name"].ToString(),
                Email = row["email"].ToString(),
                DOB = StudentImportDto.GetDate(row["DOB"].ToString()),
                Gender = (int) StudentImportDto.GetGenderType(row["Gender"].ToString().ToLower()),
                Address = row["Student_street"].ToString(),
                ZipCode = row["Student_zip"].ToString(),
                RegistrantId = (int) row["RegistrantId"],
            }).ToList();

        studentList.ForEach(student =>
            {
                sql = $"select * from StudentRegistrant where RegistrantId = {student.RegistrantId} and StudentId = {student.ID}";
                var dt = dq.ExecuteSelect(sql, msgs);
                
                if (student.RegistrantId.IsNotEmpty() && dt.Rows.Count < 1)
                {
                    var matchedColumns = $"FirstName: {student.FirstName}, LastName: {student.LastName}";

                    var answerByDob = FormQuestionAnswer.Load(
                        new Hashtable()
                        {
                            {"RegistrantId", student.RegistrantId},
                            {"QuestionText", "Date of Birth"},
                            {"QuestionDataTypeId", (int) QuestionDataType.RegistrantDateOfBirth},
                            {"Text", student.DOB},
                        },
                        msgs: msgs,
                        dc: dc
                    ).FirstOrDefault();

                    var registrantByGender = Registrant.Load(
                        new Hashtable()
                        {
                            {"ID", student.RegistrantId},
                            {"GenderId", student.Gender},
                        },
                        msgs: msgs,
                        dc: dc
                    ).FirstOrDefault();

                    var answerByPostalCode = FormQuestionAnswer.Load(
                        new Hashtable()
                        {
                            {"RegistrantId", student.RegistrantId},
                            {"QuestionText", "Zip Code"},
                            {"QuestionDataTypeId", (int) QuestionDataType.RegistrantPostalCode},
                            {"Text", student.ZipCode},
                        },
                        msgs: msgs,
                        dc: dc
                    ).FirstOrDefault();

                    var answerByAddress = FormQuestionAnswer.Load(
                        new Hashtable()
                        {
                            {"RegistrantId", student.RegistrantId},
                            {"QuestionText", "Street Address"},
                            {"QuestionDataTypeId", (int) QuestionDataType.RegistrantAddress1},
                            {"Text", student.Address},
                        },
                        msgs: msgs,
                        dc: dc
                    ).FirstOrDefault();

                    if (answerByDob.IsNotEmpty())
                        matchedColumns = string.Concat(matchedColumns, $", Date of Birth: {student.DOB}");

                    if (registrantByGender.IsNotEmpty())
                        matchedColumns = string.Concat(matchedColumns, $", Gender: {(GenderType) student.Gender}");

                    if (answerByPostalCode.IsNotEmpty())
                        matchedColumns = string.Concat(matchedColumns, $", ZipCode: {student.ZipCode}");

                    if (answerByAddress.IsNotEmpty())
                        matchedColumns = string.Concat(matchedColumns, $", Address: {student.Address}");

                    matchedColumns = matchedColumns.Replace("'","''"); // for avoiding single quotes in matchedColumns during insertion
                    
                    var sqlInsert = $"insert into StudentRegistrant(RegistrantId,StudentId,MatchedColumns) " +
                                    $"values('{student.RegistrantId}','{student.ID}','{matchedColumns}')";

                    if(answerByDob.IsNotEmpty())
                        msgs.AddRange(dq.ExecuteSql(sqlInsert));
                }
            }
        );
    }
}