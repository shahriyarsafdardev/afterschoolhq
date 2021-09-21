using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateFormQuestionAnswers_20210918
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    //motive: updating FormQuestionAnswer's Text(Answer) column with standard json format for all list question types
                    //logic: get all FormQuestion's with list types,
                    //get it's FormQuestionAnswer, if not (null, empty, json) then proceed and convert to json like this [{"key":"key","value":"desc"}]

                    var formQuestions = FormQuestion.Load(
                        criterias: new Hashtable() { 
                            { "QuestionTypeId", new List<int>() { (int)QuestionType.RadioList, (int)QuestionType.CheckboxList, (int)QuestionType.DropDownList } }
                        },
                        start: null,
                        length: null,
                        msgs: amsgs,
                        dc: adc)
                    .ToList();

                    foreach (var item in formQuestions)
                    {
                        var formQuestionAnswers = FormQuestionAnswer.LoadByFormQuestionId(FormQuestionId: item.ID.Value, msgs: amsgs, dc: adc).ToList();

                        foreach (var formQuestionAnswer in formQuestionAnswers)
                        {
                            if (formQuestionAnswer.Text.IsNotEmpty())
                            {                               
                                var isJson = IsAlreadyJsonString(formQuestionAnswer.Text);
                                if (isJson)
                                    continue;

                                List<KeyValuePair> keyValuePair = new List<KeyValuePair>();

                                if (item.QuestionTypeId.In(new List<QuestionType>() { QuestionType.RadioList, QuestionType.DropDownList }))
                                    keyValuePair.Add(new KeyValuePair() { key = formQuestionAnswer.Text, value = formQuestionAnswer.Text });                                    
                                else if (item.QuestionTypeId.In(new List<QuestionType>() { QuestionType.CheckboxList }))
                                {
                                    var splitedItems = formQuestionAnswer.Text.Split(',');
                                    
                                    foreach (var answer in splitedItems)
                                        keyValuePair.Add(new KeyValuePair() { key = answer, value = answer });
                                }

                                formQuestionAnswer.Text = keyValuePair.ToJSON();
                                amsgs.AddRange(formQuestionAnswer.Save(adc));
                            }
                        }
                    }
                },
                uniqueIdenfier: "UpdateFormQuestionAnswers_20210918",
                msgs: msgs,
                dc: dc
            );
        }

        public static bool IsAlreadyJsonString(string s)
        {
            try
            {
                var stringInput = s.Trim();
                if ((stringInput.StartsWith("{") && stringInput.EndsWith("}")))
                    return true; //For json object
                else if (stringInput.StartsWith("[") && stringInput.EndsWith("]"))
                    return true; //For json array

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

    public class KeyValuePair
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}
