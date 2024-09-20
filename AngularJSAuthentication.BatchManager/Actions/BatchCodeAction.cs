using AngularJSAuthentication.BatchManager.Constants;
using AngularJSAuthentication.BatchManager.Helpers;
using AngularJSAuthentication.DataContracts.BatchCode;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager.Actions
{
    public abstract class BatchCodeAction
    {
        private bool RemoveFromMongo(BatchCodeSubject subject)
        {
            BatchCodeHelper batchCodeHelper = new BatchCodeHelper();
            return batchCodeHelper.Delete(subject);
        }
        private bool ProcessInMongoQueue(BatchCodeSubject subject)
        {
            MongoDbHelper<BatchCodeSubjectMongoQueue> mongoDbHelper = new MongoDbHelper<BatchCodeSubjectMongoQueue>();
            return mongoDbHelper.UpdateByHashCode(subject.HashCode, true);
        }

        public async Task<bool> FinalRun(BatchCodeSubject subject)
        {
            bool result = await Run(subject);
            if (result)
            {
                if(string.IsNullOrEmpty(subject.QueueName) || subject.QueueName == Queues.BatchCodeQueue)
                {
                    result = RemoveFromMongo(subject);
                }
                ProcessInMongoQueue(subject);
                return result;
            }
            else
            {
                MongoDbHelper<BatchCodeSubjectMongoQueue> mongoDbHelper = new MongoDbHelper<BatchCodeSubjectMongoQueue>();
                mongoDbHelper.UpdateWhenSubscriberErrorOccurs(subject.HashCode, true, "Relative Action returs false");
                return result;
            }

        }

        protected abstract Task<bool> Run(BatchCodeSubject subject);

    }
}
