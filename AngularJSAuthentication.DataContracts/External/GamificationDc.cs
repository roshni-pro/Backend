using AngularJSAuthentication.Model.RetailerApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External
{
    public class GamificationDc
    {
        public class RewardTypeDc
        {
            public int RewardID { get; set; }
            public string RewardType { get; set; }
        }
        public class GameBucketRewards
        {
            public int TotalRecords { get; set; }
            public List<GameBucketRewardsDC> gameBucketRewards { get; set; }
        }
        public class PostGameBucketRewards
        {
            public long BucketRewardConditionsID { get; set; }
            public int BucketNo { get; set; }
            public bool IsFix { get; set; }
            public int RewardType { get; set; }
            public int value { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string RewardApproveStatus { get; set; }
            public List<GameConditionList> GameConditionLists { get; set; }
        }
        public class EditGameBucketRewards
        {
            public long BucketRewardConditionsID { get; set; }
            public int BucketNo { get; set; }
            public bool IsFix { get; set; }
            public int RewardType { get; set; }
            public int value { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string RewardApproveStatus { get; set; }
            //public long GameConditionMasterId { get; set; }
            //public int GameValue { get; set; }
        }
        public class GameConditionList
        {
            public long GameConditionMasterId { get; set; }
            public int value { get; set; }
        }
        public class ElasticCRMData
        {
            public string skcode { get; set; }
            public int bucketno { get; set; }
            public DateTime lastorderdate { get; set; }
            public DateTime startdate { get; set; }
            public DateTime enddate { get; set; }
        }

        public class GameBucketRewardsDC
        {
            public long BucketRewardConditionsID { get; set; }
            public long GameConditionMasterId { get; set; }

            //public DateTime CreatedDate { get; set; }
            //public DateTime? ModifiedDate { get; set; }
            //public bool IsActive { get; set; }
            //public bool? IsDeleted { get; set; }
            //public int CreatedBy { get; set; }
            //public int? ModifiedBy { get; set; }
            public int BucketNo { get; set; }
            public bool IsFix { get; set; } //1-Fix / 0-Dynamic
            public int RewardType { get; set; } //wallet-0/RI-1/offer-2
            public int BucketRewardValue { get; set; } //point/offerId
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string RewardApproveStatus { get; set; }
            public string RewardTypeName { get; set; }
            public string isFixTypeName { get; set; }
            public string AppDesc { get; set; }
            public int? GameValue { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeleted { get; set; }
        }
        public class GameConditionDC
        {
            public long Id { get; set; }
            public int BucketNo { get; set; }
            public string AppDesc { get; set; }
            public int AppValue { get; set; }
        }
        public class GameCondition
        {
            public int TotalRecords { get; set; }
            public List<GameConditionDC> gameCondition { get; set; }
        }

        public class GameConditionMastersDc
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public string AppDesc { get; set; }
            public string SqlQuery { get; set; }

        }

        public class MappingRersultDc
        {
            public int CustomerId { get; set; }
            public int BucketNo { get; set; }
            public int GameBucketNo { get; set; }
            public int BucketRerward { get; set; }
            public long ConditionId { get; set; }
            public string ApiDes { get; set; }
            public string MappingCondiName { get; set; }
            public double SqlOutPut { get; set; }
            public string SqlResultStatus { get; set; }
            public DateTime CreatedDate { get; set; }
            public bool IsActive { get; set; }
            public int CreatedBy { get; set; }
        }


        public class RetailerBucketGame
        {

            public int CurrentBucket { get; set; }
            public int LevelUpBucket { get; set; }
            public int NextOrderDay { get; set; }
            public int CurrentStreakBucket { get; set; }
            public List<GameBucket> GameBucket { get; set; }

            //public List<GameStreakDc> CustomerStreakDataList { get; set; }
            //public List<GameStreakConfig> StreakConfigDataList { get; set; }

        }
        public class crmjobdc
        {
            public int customerid { get; set; }
            public int bucketno { get; set; }
            public string Skcode { get; set; }
            public DateTime lastorderdate { get; set; }
        }

        public class GameBucket
        {
            public int BucketNo { get; set; }
            public int CRMBucketNo { get; set; }
            public int TotalCustomers { get; set; }
            public string RewardType { get; set; }
            public int RewardValue { get; set; }
            public string Status { get; set; }
            public int RewardCredited { get; set; }
            public List<GameBucketRewardCondition> GameBucketRewardConditions { get; set; }
        }

        public class GameBucketRewardCondition
        {
            public long ConditionId { get; set; }
            public string AppDesc { get; set; }
            public int Value { get; set; }
            public int AchiveValue { get; set; }
        }

        public class GameBucketConditionData
        {
            public long GameBucketRewardId { get; set; }
            public int bucketNo { get; set; }
            public bool IsFix { get; set; } //1-Fix / 0-Dynamic
            public int RewardType { get; set; }
            public int Value { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string RewardApproveStatus { get; set; }
            public long ConditionId { get; set; }
            public string Condition { get; set; }
            public int ConditionValue { get; set; }
            public string AppDesc { get; set; }
            public string SqlQuery { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class CustomerAchiveLevelDc
        {
            public string CustomerName { get; set; }
            public string ShopName { get; set; }
            public int Level { get; set; }
            public int Reward { get; set; }
            public string Image { get; set; }
        }
        public class GameCustomerLevesDC
        {
            public int CustomerId { get; set; }
            public int BucketNo { get; set; }
            public int GameBucketNo { get; set; }
            public int RewardCredit { get; set; }
            public string RewardStatus { get; set; }
            public bool IsCompleted { get; set; }
            public int RequiredAcheiveValue { get; set; }
            public int AcheiveValue { get; set; }
            public string AppDesc { get; set; }
        }
        public class GetlevelcustomerwiseDC
        {
            public int RequiredAcheiveValue { get; set; }
            public int AcheiveValue { get; set; }
            public string AppDesc { get; set; }
        }
        public class GameBucketConditionDC
        {
            public long BucketRewardConditionsID { get; set; }
            public long GameConditionMasterId { get; set; }

            //public DateTime CreatedDate { get; set; }
            //public DateTime? ModifiedDate { get; set; }
            //public bool IsActive { get; set; }
            //public bool? IsDeleted { get; set; }
            //public int CreatedBy { get; set; }
            //public int? ModifiedBy { get; set; }
            public int BucketNo { get; set; }
            public bool IsFix { get; set; } //1-Fix / 0-Dynamic
            public int RewardType { get; set; } //wallet-0/RI-1/offer-2
            public int BucketRewardValue { get; set; } //point/offerId
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string RewardApproveStatus { get; set; }
            public string RewardTypeName { get; set; }
            public string isFixTypeName { get; set; }
            public string AppDesc { get; set; }
            public long BucketRewardId { get; set; }
        }
        public class GameBucketCondition
        {
            public int TotalRecords { get; set; }
            public List<GameBucketConditionDC> gamebucketcondition { get; set; }
        }

        public class GameDashboard
        {
            public int CityId { get; set; }
            public List<int?> Warehouseid { get; set; }
            public string Skcode { get; set; }
            public int FromBucketNo { get; set; }
            public int EndBucketNo { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; }
        }
        public class GameSkcodeDC
        {
            public string Skcode { get; set; }
            public int CustomerId { get; set; }
            public string ShopName { get; set; }
        }
        public class GameCustomerBucketHdrOrderDetailDC
        {
            public string Skcode { get; set; }
            public int CustomerId { get; set; }
            public string ShopName { get; set; }
            public string OrderId { get; set; }
            public int BucketNo { get; set; }
            public int GameBucketNo { get; set; }
            public bool IsCompleted { get; set; }
            public string RewardStatus { get; set; }
            public int RewardCredit { get; set; }
            public int CurrentBucket { get; set; }
            public int LevelUpBucket { get; set; }
            public DateTime? BucketStartDate { get; set; }
            public DateTime? BucketEndDate { get; set; }
            public string gameBucketRewardCondition { get; set; }
            //public List<GameBucketRewardCondition> gameBucketRewardCondition { get; set; }
            public long GameBucketRewardId { get; set; }
        }

        public class GameDashboardHeaderDC
        {
            public string SkCode { get; set; }
            public int CustomerId { get; set; }
            public string ShopName { get; set; }
            public int CrmBucketNo { get; set; }
            public int BucketNo { get; set; }
            public int NextBucketNo { get; set; }
            public int GameBucketNo { get; set; }
            public DateTime? LastOrderDate { get; set; }
            public DateTime? BucketStartDate { get; set; }
            public DateTime? BucketEndDate { get; set; }
            public int CurrentBucket { get; set; }
            public int LevelUpBucket { get; set; }

        }
        public class GameDashboardHeader
        {
            public List<GameDashboardHeaderDC> DashboardHeader { get; set; }
            public int TotalRecords { get; set; }
        }


        public class GameBucketNoConditionDC
        {
            public long BucketRewardConditionsID { get; set; }
            public long GameConditionMasterId { get; set; }
            public int BucketNo { get; set; }
            public string AppDesc { get; set; }
            public int GameValue { get; set; }
        }


        public class StreakCustomerDc
        {
            public int CustomerId { get; set; }
            public int NoOfPeriod { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int BucketNo { get; set; }
        }

        public class GameStreakLevelConfigMasterDc
        {
            public int BucketNoFrom { get; set; }
            public int BucketNoTo { get; set; }
            public bool IsActiveCurrent { get; set; }
            public DateTime CreatedDate { get; set; }

        }

        public class GameStreakLevelConfigDetailDc
        {
            public long Id { get; set; }
            public int BucketNoFrom { get; set; }
            public int BucketNoTo { get; set; }
            public int StreakConditionType { get; set; } // 1-Level-Individual, 2-Streak, 3-Outof 
            public string StreakConditionTypeName { get; set; }
            public string Type { get; set; }
            public int RewardType { get; set; } //wallet-0/RI-1/offer-2
            public string RewardTypeName { get; set; }
            public double RewardValue { get; set; }
            public int Streak_StreakCount { get; set; }
            public int Streak_ChooseReward { get; set; } //1-Multiplier / 2-Fixed
            public string Streak_ChooseRewardName { get; set; }
            public int OutOf_TotalBucket { get; set; }
            public int OutOf_OutOfBucket { get; set; }
            public int LevelNo { get; set; }
            public int LevelValue { get; set; }
            public string Condition { get; set; }
            public string Reward { get; set; }
            public string StreakDescr { get; set; }
            public long GameStreakLevelConfigDetailId { get; set; }
        }

        public class GameStreakLevelConfigDetailListDc
        {
            public List<GameStreakLevelConfigDetailDc> StreakLevelConfig { get; set; }
        }


        public class GameStreakDc
        {
            public int BucketNo { get; set; }
            public int StreakId { get; set; }
            //public DateTime StreakBucketStartDate { get; set; }
            //public DateTime StreakBucketEndDate { get; set; }
            public int DaysLeft { get; set; }
            public string Status { get; set; }
            public string IndivitualRewardValue { get; set; }
            public double RewardCredited { get; set; }
            public int StreakIdFrom { get; set; }
            public int StreakIdTo { get; set; }
            //public long GameStreakLevelConfigMasterId { get; set; }

            public List<GameStreakConfig> GameStreakConfigData { get; set; }
        }

        public class GameMainListDc
        {
            public List<GameStreakDc> CustomerStreakDataList { get; set; }
            public List<GameStreakConfig> StreakConfigDataList { get; set; }
        }

        public class GameStreakConfig
        {
            public long Id { get; set; }
            public string Type { get; set; }
            public string Condition { get; set; }
            public string RewardTypeName { get; set; }
            public string Reward { get; set; }
            public string StreakDescr { get; set; }
            public int DayLeft { get; set; }

            public int StreakLevelValueFirst { get; set; }
            public int StreakLevelValueLast { get; set; } //total values
            public int StreakAchivedCount { get; set; }

            //public int LevelNo { get; set; }
            //public int BucketNoFrom { get; set; }
            //public int BucketNoTo { get; set; }
        }

        public class AchiveConfigDc
        {
            public int StreakAchivedCount { get; set; }
            public string type { get; set; }
            public long Id { get; set; }
            public string Condition { get; set; }
            // public int LevelNo { get; set; }
        }
        public class GameStreakLevelRewardValueDc
        {
            public int CustomerId { get; set; }
            public int BucketNo { get; set; }
            public int StreakIdFrom { get; set; }
            public int StreakIdTo { get; set; }
            public long GameStreakLevelConfigMasterId { get; set; }
            public double RewardValue { get; set; }
        }

        public class GameStreakLevelRewardValueDC
        {
            public int CustomerId { get; set; }
            public int BucketNo { get; set; }
            public int StreakIdFrom { get; set; }
            public int StreakIdTo { get; set; }
            public long GameStreakLevelConfigMasterId { get; set; }
            public long GameStreakLevelConfigDetailId { get; set; }
            public double RewardValueCr { get; set; }
            public double RewardValueDr { get; set; }
            public double RemaningRewardAmount { get; set; }
            public long ReferGameStreakLevelConfigMasterId { get; set; }
            public string RewardStatus { get; set; }
            public bool IsCompleted { get; set; }
            public bool IsCancelRewardSettled { get; set; }
            public DateTime? BucketStartDate { get; set; }
            public DateTime? BucketEndDate { get; set; }
        }

        public class CreateCustomerLedgerDc
        {
            public int CustomerId { get; set; }
            public int GameBucketNo { get; set; }
            public int BucketNo { get; set; }
            public int ForRewardStrack { get; set; } //"Reward=1  / Strack=2
            public int StreakIdFrom { get; set; }
            public int StreakIdTo { get; set; }
            public long GameBucketRewardId { get; set; }
            public long GameStreakLevelConfigMasterId { get; set; }
            public long GameStreakLevelConfigDetailId { get; set; }
            public double RewardValue { get; set; }

            public bool IsUpComingReward { get; set; }
            public DateTime? IsUpComingRewardDate { get; set; }

            public bool IsCompleted { get; set; }
            public DateTime? IsCompletedDate { get; set; }

            public bool IsCanceled { get; set; }
            public DateTime? IsCanceledDate { get; set; }


            public DateTime? BucketStartDate { get; set; }
            public DateTime? BucketEndDate { get; set; }
            public List<int> OrderIdList { get; set; }
            public List<long> GameStreakLevelConfigMasterIdList { get; set; }
            public bool IsActive { get; set; }
            public bool? IsDeleted { get; set; }
            public DateTime CreatedDate { get; set; }
            public int CreatedBy { get; set; }
            public DateTime? ModifiedDate { get; set; }
            public int? ModifiedBy { get; set; }
        }

        public class UpdateCustomerLedgerStatusDc
        {
            public int CustomerId { get; set; }
            public int GameBucketNo { get; set; }
            public int BucketNo { get; set; }
            public int ForRewardStrack { get; set; } //"Reward=1  / Strack=2
            public int StreakIdFrom { get; set; }
            public int StreakIdTo { get; set; }
            public long GameBucketRewardId { get; set; }
            public long GameStreakLevelConfigMasterId { get; set; }
            public long GameStreakLevelConfigDetailId { get; set; }

            public bool IsUpComingReward { get; set; }
            public DateTime? IsUpComingRewardDate { get; set; }

            public bool IsCompleted { get; set; }
            public DateTime? IsCompletedDate { get; set; }

            public bool IsCanceled { get; set; }
            public DateTime? IsCanceledDate { get; set; }
        }

        public class CreateCustomerHistoryDc
        {
            public int CustomerId { get; set; }
            public int GameBucketNo { get; set; }
            public int BucketNo { get; set; }
            public string ForRewardStrack { get; set; } //"Reward=1  / Strack=2
            public long GameBucketRewardId { get; set; }
            public long GameStreakLevelConfigMasterId { get; set; }
            public long GameStreakLevelConfigDetailId { get; set; }
            public int StreakIdFrom { get; set; }
            public int StreakIdTo { get; set; }
            public double RewardValue { get; set; }
            public string RewardStatus { get; set; }
            public DateTime? RewardStatusDate { get; set; }
            public DateTime? BucketStartDate { get; set; }
            public DateTime? BucketEndDate { get; set; }
            public string GameType { get; set; }
            public string GameCondition { get; set; }


            //public bool IsUpComingReward { get; set; }
            //public DateTime? IsUpComingRewardDate { get; set; }
            //public bool IsCompleted { get; set; }
            //public DateTime? IsCompletedDate { get; set; }
            //public bool IsCanceled { get; set; }
            //public DateTime? IsCanceledDate { get; set; }


            //public List<int> OrderIdList { get; set; }
            //public List<long> GameStreakLevelConfigMasterIdList { get; set; }
            //public bool IsActive { get; set; }
            //public bool? IsDeleted { get; set; }
            //public DateTime CreatedDate { get; set; }
            //public int CreatedBy { get; set; }
            //public DateTime? ModifiedDate { get; set; }
            //public int? ModifiedBy { get; set; }
        }

        public class RewardEarningHistoryDc
        {
            public double TotalEarningPoint { get; set; }
            public double UpCommingPoint { get; set; }
            public List<CreateCustomerHistoryDc> CreateCustomerHistoryDcs { get; set; }
        }

        public class CustomerUpcomingRewardsDc
        {
            public double IndividualRewardValue { get; set; }
            public double OutofRewardValue { get; set; }
            public double StreakRewardValue { get; set; }
            public double WalletRewardValue { get; set; }
            public int IndividualDaysLeft { get; set; }
            public bool IndividualOrderPunch { get; set; }
        }

        public class ResultQueryDc
        {
            public double OrderCount { get; set; }
            public double TotalOrderAmount { get; set; }
            public double OrderDeliveryAmount { get; set; }
            public double AvgLineItem { get; set; }
        }

        public class GetStreakDashboardDC
        {
            public int StreakId { get; set; }   
            public DateTime StartDate { get; set; } 
            public DateTime EndDate { get; set; } 
            public bool Status { get; set; }
           
        }

        public class SingleMapview
        {
            public List<GetStreakDashboardDC> getStreakDashboardDCs { get; set; }
            public List<GameStreakLevelConfigDetailDc> gameStreakLevelConfigDetailDcs { get; set; }
        }



        public class GameStreakCustomerTransactionDc
        {
            //public ObjectId Id { get; set; }
            public int CustomerId { get; set; }
            public int BucketNo { get; set; }
            public int StreakId { get; set; }
            public bool StreakIsFulfill { get; set; }
            public bool IsRewardProvided { get; set; }

            public DateTime BucketStartDate { get; set; }

            public DateTime BucketEndDate { get; set; }
            public bool IsExpired { get; set; }
            public bool IsActive { get; set; }
            public bool? IsDeleted { get; set; }
            public DateTime CreatedDate { get; set; }
            public int CreatedBy { get; set; }
            public DateTime? ModifiedDate { get; set; }
            public int? ModifiedBy { get; set; }
            public List<int> OrderId { get; set; }
            public List<long> GameStreakLevelConfigMasterId { get; set; }
            public bool? IsOrderCancel { get; set; }
            public int IsStreakContinue { get; set; }
        }
    }
}
