using AngularJSAuthentication.API.Controllers.External.Gamification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class GamificationHelper
    {

        public void UpdateCutomerRewardStatusTwoMinJob()
        {
            var obj = new GamificationController();
            var dailyDamageMovementReport = obj.UpdateCutomerRewardStatus();
        }
    }
}