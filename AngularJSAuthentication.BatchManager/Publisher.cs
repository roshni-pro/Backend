using AngularJSAuthentication.BatchManager.Constants;
using AngularJSAuthentication.BatchManager.Helpers;
using AngularJSAuthentication.DataContracts.BatchCode;
using System;

namespace AngularJSAuthentication.BatchManager
{
    public class Publisher
    {
        public void PublishInBatchCode(BatchCodeSubject subject, string queueName = "")
        {
            try
            {
                string thisQueueName = "";
                if (string.IsNullOrEmpty(queueName))
                {
                    thisQueueName = Queues.BatchCodeQueue;
                }
                else
                {
                    thisQueueName = queueName;
                }
                subject.QueueName = thisQueueName;

                FileWriter fileWriter = new FileWriter();
                fileWriter.WriteToFile($"Publishing Subject ObjectDetailId: {subject.ObjectDetailId.ToString()}, ObjectId: {subject.ObjectId.ToString()}, TransactionType: {subject.TransactionType}, Qty: {subject.Quantity.ToString()}, WarehouseId: {subject.WarehouseId}");
                BatchCodeHelper batchCodeHelper = new BatchCodeHelper();
                
                if(thisQueueName == Queues.BatchCodeQueue)
                {
                    string mongoId = batchCodeHelper.Insert(subject);
                    subject.HashCode = mongoId;
                    if (string.IsNullOrEmpty(subject.HashCode))
                    {
                        subject.HashCode = Guid.NewGuid().ToString();
                    }
                }
                else
                {
                    subject.HashCode = Guid.NewGuid().ToString();
                }

                MongoDbHelper<BatchCodeSubjectMongoQueue> mongoDbHelper = new MongoDbHelper<BatchCodeSubjectMongoQueue>();
                mongoDbHelper.Insert(new BatchCodeSubjectMongoQueue
                {
                    HashCode = subject.HashCode,
                    IsProcess = false,
                    ItemMultiMrpId = subject.ItemMultiMrpId,
                    ObjectDetailId = subject.ObjectDetailId,
                    ObjectId = subject.ObjectId,
                    Quantity = subject.Quantity,
                    TransactionDate = subject.TransactionDate,
                    TransactionType = subject.TransactionType,
                    WarehouseId = subject.WarehouseId,
                    IsPublishErrorOccurs = false,
                    IsSubscriberErrorOccurs = false,
                    PublishError = "",
                    SubscriberError = "",
                    QueueName = thisQueueName
                });
               
                RabbitMqHelperNew rabbitMqHelper = new RabbitMqHelperNew();
                rabbitMqHelper.Publish(thisQueueName, subject);
            }
            catch (Exception ex)
            {
                string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                FileWriter fileWriter = new FileWriter();
                fileWriter.WriteToFile("Error occurs : " + error);
                MongoDbHelper<BatchCodeSubjectMongoQueue> mongoDbHelper = new MongoDbHelper<BatchCodeSubjectMongoQueue>();
                mongoDbHelper.UpdateWhenPublisherErrorOccurs(subject.HashCode, false, error);

            }

        }
    }
}
