using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.DataContracts.Mongo;
using LinqKit;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace AngularJSAuthentication.BatchManager.Helpers
{
    public class BatchCodeHelper
    {
        public string GetMongoCollectionName(int itemMultiMRPId, int warehouseId)
        {
            string collectionName = string.Format("Batchcode-{0}-{1}", itemMultiMRPId.ToString(), warehouseId.ToString());
            return collectionName;
        }

        public bool Delete(BatchCodeSubject subject)
        {
            ElasticBatchHelper elasticBatchHelper = new ElasticBatchHelper();
            bool result = elasticBatchHelper.Delete(subject.HashCode);
            return result;

        }

        public string Insert(BatchCodeSubject subject)
        {
            ElasticBatchHelper elasticBatchHelper = new ElasticBatchHelper();
            string id = elasticBatchHelper.Insert(new BatchCodeElastic { 
                createddate = DateTime.Now,
                itemmultimrpid = subject.ItemMultiMrpId,
                warehouseid = subject.WarehouseId
            });
            return id;
        }


        public bool IsAnyPendingDocExists(int itemMultiMRPId, int warehouseId)
        {
            ElasticBatchHelper elasticBatchHelper = new ElasticBatchHelper();
            return elasticBatchHelper.IsAnyPendingDocExists(itemMultiMRPId, warehouseId);
        }

        private BatchCodePendingTransaction GetTransaction(BatchCodeSubject subject)
        {
            BatchCodePendingTransaction transaction = new BatchCodePendingTransaction
            {
                //Timestamp = GetTimestmp(subject.TransactionDate),
                TransactionDate = subject.TransactionDate,
                ObjectId = subject.ObjectId,
                ObjectDetailId = subject.ObjectDetailId,
                TransactionType = subject.TransactionType
            };
            return transaction;
        }

        public long GetTimestmp(DateTime dateTime)
        {
            long timestamp = dateTime.Ticks;
            return timestamp;
        }


        public bool InsertSubjectInMongo(BatchCodeSubject subject)
        {
            MongoDbHelper<BatchCodeSubjectMongo> mongoDbHelper
                   = new MongoDbHelper<BatchCodeSubjectMongo>();
            return mongoDbHelper.Insert(new BatchCodeSubjectMongo
            {
                HashCode = subject.HashCode,
                ItemMultiMrpId = subject.ItemMultiMrpId,
                ObjectDetailId = subject.ObjectDetailId,
                ObjectId = subject.ObjectId,
                Quantity = subject.Quantity,
                TransactionDate = subject.TransactionDate,
                TransactionType = subject.TransactionType,
                WarehouseId = subject.WarehouseId
            });
        }

        public bool InsertSubjectInMongoError(BatchCodeSubject subject)
        {
            MongoDbHelper<BatchCodeSubjectMongoError> mongoDbHelper
                   = new MongoDbHelper<BatchCodeSubjectMongoError>();
            return mongoDbHelper.Insert(new BatchCodeSubjectMongoError
            {
                HashCode = subject.HashCode,
                ItemMultiMrpId = subject.ItemMultiMrpId,
                ObjectDetailId = subject.ObjectDetailId,
                ObjectId = subject.ObjectId,
                Quantity = subject.Quantity,
                TransactionDate = subject.TransactionDate,
                TransactionType = subject.TransactionType,
                WarehouseId = subject.WarehouseId
            });
        }

        public bool DeleteSubjectFromMongo(BatchCodeSubjectMongo subject)
        {
            MongoDbHelper<BatchCodeSubjectMongo> mongoDbHelper
                 = new MongoDbHelper<BatchCodeSubjectMongo>();
            return mongoDbHelper.Delete(subject.Id);
        }
    }
}
