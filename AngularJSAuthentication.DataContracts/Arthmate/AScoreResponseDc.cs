using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Arthmate
{

    public class AScoreResponseDc
    {
        public string requestID { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
        public ScoreData data { get; set; }
    }

    public class ScoreData
    {
        public string product { get; set; }
        public string request_id { get; set; }
        public Score score { get; set; }
    }
    public class Score
    {
        public string CINSRate { get; set; }
        public int DPD24C2 { get; set; }
        public int EQ3C1 { get; set; }
        public int HL12 { get; set; }
        public int HL24C3 { get; set; }
        public int HL36CC { get; set; }
        public int LLPL { get; set; }
        public int NCVL_AScore { get; set; }
        public int PSMA24 { get; set; }
        public double RSKPremium { get; set; }
        public double VYC3 { get; set; }
    }
}
