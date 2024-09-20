using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.ServeyModule
{
    public class SurveyModuleDC
    {
        public int SurveyId { get; set; }
        public string SurveyName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int QuestionCount { get; set; }
        public int AnsweredCount { get; set; }
        public int PointsEarned { get; set; }
        public int CorrectAnswerCount { get; set; }
        public List<SurveyQuestionAnswerDc> SQA { get; set; }

    }

    public class SurveyQuestionAnswerDc
    {
        public long QuestionId { get; set; }
        public string QueName { get; set; }
        public long RightAnsId { get; set; }
        public bool isRequired { get; set; }
        public int Point { get; set; }
        public int Sequence { get; set; }
        public int OptionCount { get; set; }
        public bool IsAnswerd { get; set; }
        public List<SurveyQADList> AnswerList { get; set; }
    }

    public class SurveyQADList
    {
        public long AnswerId { get; set; }
        public string Answer { get; set; }
        public int Sequence { get; set; }
    }

    public class GetSurveyDc
    {
        public int WarehouseId { get; set; }
        public int CustomerId { get; set; }
        public string Language { get; set; }
    }



    public class SurveyQuestAnswDC
    {
        public long surveyId { get; set; }
        public string SurveyName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string QueName { get; set; }
        public long QuestionId { get; set; }
        public bool isRequired { get; set; }
        public int Point { get; set; }
        public int QuestionSequence { get; set; }
        public long answerId { get; set; }
        public bool IsAnswerd { get; set; }
        public string Answer { get; set; }
        public int answerSequence { get; set; }
        public long RightAnsId { get; set; }
        public int PointsEarned { get; set; }
        public bool isRight { get; set; }
        public bool IsComplete { get; set; }

    }
    public class SaveAnswerDc
    {
        public int CustomerId { get; set; }
        public int SurveyId { get; set; }
        public int QueId { get; set; }// SurveyQuestion Id
        public int AnswerId { get; set; }//   SurveyQuestionAnswer Id   
        public bool isRight { get; set; }
        public bool isComplete { get; set; }
    }
}

