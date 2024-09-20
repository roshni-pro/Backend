using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    [BsonIgnoreExtraElements]
    public class GameConditionMastersMongo
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? Date { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class GameCustomerBucketHdr
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string SkCode { get; set; }
        public int CustomerId { get; set; }
        public int CRMBucketNo { get; set; }
        public int BucketNo { get; set; }
        public int NextBucketNo { get; set; }
        public int GameBucketNo { get; set; }
        public bool IsStreakCreated { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? LastOrderDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime BucketStartDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime BucketEndDate { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class GameCustomerBucketHdrOrderDetail
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public int BucketNo { get; set; }
        public int GameBucketNo { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? BucketStartDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? BucketEndDate { get; set; }

        public int RewardCredit { get; set; }
        public string RewardStatus { get; set; }
        public bool IsCompleted { get; set; }
        public long GameBucketRewardId { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class GameStreakCustomerTransaction
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int CustomerId { get; set; }
        public int BucketNo { get; set; }
        public int StreakId { get; set; }
        public bool StreakIsFulfill { get; set; }
        public bool IsRewardProvided { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime BucketStartDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
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

    }
    [BsonIgnoreExtraElements]
    public class GameStreakLevelRewardValue
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int CustomerId { get; set; }
        public int BucketNo { get; set; }
        public int StreakIdFrom { get; set; }
        public int StreakIdTo { get; set; }
        public long GameStreakLevelConfigMasterId { get; set; }
        public long GameStreakLevelConfigDetailId { get; set; }
        public double RewardValue { get; set; }
        //public double RewardValueCr { get; set; }
        //public double RewardValueDr { get; set; }
        //public double RemaningRewardAmount { get; set; }
        public long ReferGameStreakLevelConfigMasterId { get; set; }
        public string RewardStatus { get; set; }
        public bool IsCompleted { get; set; }
        //public bool IsCancelRewardSettled { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? BucketStartDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? BucketEndDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class GameCustomerLedger
    {
        [BsonId]
        public ObjectId Id { get; set; }
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
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? IsUpComingRewardDate { get; set; }

        public bool IsCompleted { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? IsCompletedDate { get; set; }

        public bool IsCanceled { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? IsCanceledDate { get; set; }

        public bool IsRedeemedReward { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? IsRedeemedRewardDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? BucketStartDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
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
}