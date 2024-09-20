using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AngularJSAuthentication.DataContracts.BatchCode;
using LinqKit;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AngularJSAuthentication.BatchManager.Helpers
{
    public class MongoDbHelper<T>
    {
        //private static Logger logger = LogManager.GetCurrentClassLogger();
        private static string connStr => ConfigurationManager.AppSettings["mongoConnStr"];
        private static MongoUrl url = new MongoUrl(connStr);
        private MongoClient dbClient = null;
        public static IMongoDatabase db = null;

        //public MongoClient dbClient = new MongoClient(new MongoClientSettings()
        //{
        //    Server = url.Server,
        //    MaxConnectionPoolSize = 3000,
        //    WaitQueueSize = 2000
        //});

        public MongoDbHelper()
        {
            dbClient = MongoSingleton.MongoClientInstance;
            if (db == null)
                db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);

            //if (bidCollection == null)
            //    bidCollection = db.GetCollection<Bid>("Bid");
            //MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(connStr));
            ////settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            //settings.MaxConnectionIdleTime = TimeSpan.FromMinutes(10);
            //dbClient = new MongoClient(settings);
        }

        public IMongoDatabase mongoDatabase { get { return db; } }

        public IEnumerable<TProjected> GetWithProjection<TProjected>(Expression<Func<T, bool>> filter, Expression<Func<T, TProjected>> fields)
        {
            IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);

            var collectionName = typeof(T).Name;

            var collection = db.GetCollection<T>(collectionName);

            return collection.Find(filter).Project(fields).ToEnumerable();
        }


        public async Task<bool> InsertLog(T data)
        {
            try
            {
                //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
                var collection = db.GetCollection<BsonDocument>("BatchService" + typeof(T).Name + "_" + DateTime.Now.ToString(@"MMddyyyy"));
                var bsonDoc = data.ToBsonDocument();
                await collection.InsertOneAsync(bsonDoc);
            }
            catch (Exception ex)
            {
            }

            return true;
        }



        public async Task<bool> InsertAsync(T data)
        {
            try
            {
                //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
                var collection = db.GetCollection<BsonDocument>(typeof(T).Name);
                var bsonDoc = data.ToBsonDocument();
                await collection.InsertOneAsync(bsonDoc);
            }
            catch (Exception ex)
            {
            }

            return true;
        }

        public bool Insert(T data, string EntityName = "")
        {

            //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
            var collection = string.IsNullOrEmpty(EntityName) ? db.GetCollection<BsonDocument>(typeof(T).Name) : db.GetCollection<BsonDocument>(EntityName);
            var bsonDoc = data.ToBsonDocument();
            collection.InsertOne(bsonDoc);

            return true;
        }

        public string InsertWithID(T data, string EntityName = "")
        {

            //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
            var collection = string.IsNullOrEmpty(EntityName) ? db.GetCollection<BsonDocument>(typeof(T).Name) : db.GetCollection<BsonDocument>(EntityName);
            var bsonDoc = data.ToBsonDocument();
            collection.InsertOne(bsonDoc);
            BsonValue val = bsonDoc.GetValue("_id");
            return val.AsObjectId.ToString();
        }

        public bool InsertMany(List<T> data, string EntityName = "")
        {
            try
            {
                //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
                var collection = string.IsNullOrEmpty(EntityName) ? db.GetCollection<T>(typeof(T).Name) : db.GetCollection<T>(EntityName);
                collection.InsertMany(data);
            }
            catch (Exception ex)
            {
            }

            return true;
        }

        public async Task<bool> InsertManyAsync(List<T> data, string EntityName = "")
        {
            try
            {
                //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
                var collection = string.IsNullOrEmpty(EntityName) ? db.GetCollection<T>(typeof(T).Name) : db.GetCollection<T>(EntityName);
                await collection.InsertManyAsync(data);
            }
            catch (Exception ex)
            {
            }

            return true;
        }

        public async Task<bool> ReplaceAsync(ObjectId id, T data, string collectionName = "")
        {
            try
            {
                //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
                if (string.IsNullOrEmpty(collectionName))
                {
                    collectionName = typeof(T).Name;
                }

                var collection = db.GetCollection<BsonDocument>(collectionName);//.FindOneAndReplaceAsync(filter,data);

                var bsonDoc = data.ToBsonDocument();
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);

                var result = await collection.FindOneAndReplaceAsync(filter, bsonDoc);

                return true;

            }
            catch (Exception ex)
            {
            }

            return true;
        }

        public async Task<bool> DeleteAsync(ObjectId id, string collectionName = "")
        {
            try
            {
                //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
                if (string.IsNullOrEmpty(collectionName))
                {
                    collectionName = typeof(T).Name;
                }

                var collection = db.GetCollection<BsonDocument>(collectionName);//.FindOneAndReplaceAsync(filter,data);

                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);

                var result = await collection.DeleteOneAsync(filter);

                return true;

            }
            catch (Exception ex)
            {
            }

            return true;
        }


        public bool UpdateByHashCode(string hashcode, bool isProcessed, string collectionName = "")
        {
            //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = typeof(T).Name;
            }

            var collection = db.GetCollection<BatchCodeSubjectMongoQueue>(collectionName);//.FindOneAndReplaceAsync(filter,data);

            var filter = Builders<BatchCodeSubjectMongoQueue>.Filter.Eq("HashCode", hashcode);
            var updateDef = Builders<BatchCodeSubjectMongoQueue>.Update.Set(o => o.IsProcess, isProcessed);
            var result = collection.UpdateOne(filter, updateDef);

            return true;


        }

        public bool UpdateWhenSubscriberErrorOccurs(string hashcode, bool isProcessed, string subscriberError, string collectionName = "")
        {
            //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = typeof(T).Name;
            }

            var collection = db.GetCollection<BatchCodeSubjectMongoQueue>(collectionName);//.FindOneAndReplaceAsync(filter,data);

            var filter = Builders<BatchCodeSubjectMongoQueue>.Filter.Eq("HashCode", hashcode);
            var updateDef = Builders<BatchCodeSubjectMongoQueue>.Update
                .Set(o => o.IsProcess, isProcessed)
                .Set(o => o.IsSubscriberErrorOccurs, true)
                .Set(o => o.SubscriberError, subscriberError);
            var result = collection.UpdateOne(filter, updateDef);

            return true;


        }

        public bool UpdateWhenPublisherErrorOccurs(string hashcode, bool isProcessed, string publisherError, string collectionName = "")
        {
            //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = typeof(T).Name;
            }

            var collection = db.GetCollection<BatchCodeSubjectMongoQueue>(collectionName);//.FindOneAndReplaceAsync(filter,data);

            var filter = Builders<BatchCodeSubjectMongoQueue>.Filter.Eq("HashCode", hashcode);
            var updateDef = Builders<BatchCodeSubjectMongoQueue>.Update
                .Set(o => o.IsProcess, isProcessed)
                .Set(o => o.IsPublishErrorOccurs, true)
                .Set(o => o.PublishError, publisherError);
            var result = collection.UpdateOne(filter, updateDef);

            return true;


        }



        public bool Delete(ObjectId id, string collectionName = "")
        {
            //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = typeof(T).Name;
            }

            var collection = db.GetCollection<BsonDocument>(collectionName);//.FindOneAndReplaceAsync(filter,data);

            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);

            var result = collection.DeleteOne(filter);

            return true;


        }


        public bool Replace(ObjectId id, T data, string collectionName = "")
        {
            try
            {
                if (string.IsNullOrEmpty(collectionName))
                {
                    collectionName = typeof(T).Name;
                }

                //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);

                var collection = db.GetCollection<BsonDocument>(collectionName);//.FindOneAndReplaceAsync(filter,data);

                var bsonDoc = data.ToBsonDocument();
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);

                var result = collection.FindOneAndReplace(filter, bsonDoc);

                return true;

            }
            catch (Exception ex)
            {
            }

            return true;
        }

        public bool ReplaceWithoutFind(ObjectId id, T data, string collectionName = "")
        {
            try
            {
                if (string.IsNullOrEmpty(collectionName))
                {
                    collectionName = typeof(T).Name;
                }

                //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);

                var collection = db.GetCollection<BsonDocument>(collectionName);//.FindOneAndReplaceAsync(filter,data);

                var bsonDoc = data.ToBsonDocument();
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);

                var result = collection.ReplaceOne(filter, bsonDoc);

                return true;

            }
            catch (Exception ex)
            {
            }

            return true;
        }
        public async Task<bool> ReplaceWithoutFindAsync(ObjectId id, T data, string collectionName = "")
        {
            try
            {
                if (string.IsNullOrEmpty(collectionName))
                {
                    collectionName = typeof(T).Name;
                }

                //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);

                var collection = db.GetCollection<BsonDocument>(collectionName);//.FindOneAndReplaceAsync(filter,data);

                var bsonDoc = data.ToBsonDocument();
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);

                var result = await collection.ReplaceOneAsync(filter, bsonDoc);

                return true;

            }
            catch (Exception ex)
            {
            }

            return true;
        }

        public List<T> GetAll()
        {
            //IMongoDatabase db = dbClient.GetDatabase("SK");
            var collection = db.GetCollection<T>(typeof(T).Name);
            return collection.Find(x => true).ToList();
        }

        public List<BatchCodeSubjectMongoQueue> GetAllUnProcessed()
        {
            //IMongoDatabase db = dbClient.GetDatabase("SK");
            var collection = db.GetCollection<BatchCodeSubjectMongoQueue>(typeof(BatchCodeSubjectMongoQueue).Name);
            return collection.Find(x => x.IsProcess == false && x.TransactionDate < DateTime.Now.AddMinutes(-30) && x.HashCode != null && x.HashCode != "").ToList();
        }


        public IEnumerable<TProjected> GetWithProjection<TProjected>(Expression<Func<T, bool>> filter, Expression<Func<T, TProjected>> fields
           , Func<IQueryable<TProjected>, IOrderedQueryable<TProjected>> orderBy = null, int? skip = null, int? take = null)
        {
            //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);

            var collectionName = typeof(T).Name;
            var collection = db.GetCollection<T>(collectionName);

            var query = collection.AsQueryable().AsExpandable().Where(filter).Select(fields);

            if (!skip.HasValue && !take.HasValue && orderBy == null)
                return query.AsEnumerable();
            else
            {
                if (skip.HasValue && take.HasValue)
                {
                    if (orderBy != null)
                    {
                        query = orderBy(query).Skip(skip.Value).Take(take.Value);
                        return query.AsEnumerable();
                    }

                    return query.Skip(skip.Value).Take(take.Value).AsEnumerable();

                }
                else
                {
                    if (orderBy != null)
                        query = orderBy(query);

                    return query.AsEnumerable();


                }
            }

        }


        public List<T> Select(Expression<Func<T, bool>> condition, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int? skip = null, int? take = null, bool isLog = false, string dateformat = "", string collectionName = "")
        {
            //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);

            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = typeof(T).Name;

                if (isLog && !string.IsNullOrEmpty(dateformat))
                    collectionName = collectionName + "_" + dateformat;
            }

            var collection = db.GetCollection<T>(collectionName);

            var query = collection.AsQueryable().AsExpandable().Where(condition);

            if (skip.HasValue && take.HasValue)
            {
                if (orderBy != null)
                {
                    query = orderBy(query).Skip(skip.Value).Take(take.Value);
                    return query.ToList();
                }

                return query.Skip(skip.Value).Take(take.Value).ToList();

            }
            else
            {
                if (orderBy != null)
                    query = orderBy(query);

                return query.ToList();


            }
        }

        public async Task<List<T>> SelectAsync(Expression<Func<T, bool>> condition, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int? skip = null, int? take = null, bool isLog = false, string dateformat = "", string collectionName = "")
        {
            //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);

            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = typeof(T).Name;

                if (isLog && !string.IsNullOrEmpty(dateformat))
                    collectionName = collectionName + "_" + dateformat;
            }

            var collection = db.GetCollection<T>(collectionName);

            var query = collection.AsQueryable().AsExpandable().Where(condition);

            if (skip.HasValue && take.HasValue)
            {
                if (orderBy != null)
                {
                    query = orderBy(query).Skip(skip.Value).Take(take.Value);
                    return await query.ToListAsync();
                }

                return await query.Skip(skip.Value).Take(take.Value).ToListAsync();

            }
            else
            {
                if (orderBy != null)
                    query = orderBy(query);

                return await query.ToListAsync();


            }
        }

        public int Count(Expression<Func<T, bool>> condition, bool isLog = false, string dateformat = "", string collectionName = "")
        {
            //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);

            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = typeof(T).Name;

                if (isLog && !string.IsNullOrEmpty(dateformat))
                    collectionName = collectionName + "_" + dateformat;
            }

            var collection = db.GetCollection<T>(collectionName);

            if (typeof(T) == typeof(T))
            {
                return collection.AsQueryable().AsExpandable().Where(condition).Count();
            }
            return 0;
        }
        public string InsertPickerMongo(T data, string EntityName = "")
        {

            //IMongoDatabase db = dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
            var collection = string.IsNullOrEmpty(EntityName) ? db.GetCollection<BsonDocument>(typeof(T).Name) : db.GetCollection<BsonDocument>(EntityName);
            var bsonDoc = data.ToBsonDocument();
            bsonDoc["_id"] = ObjectId.Empty;
            collection.InsertOne(bsonDoc);
            object id = (object)bsonDoc.GetValue("_id");
            string objectid = Convert.ToString(id);
            return objectid;
        }


    }


    public sealed class MongoSingleton
    {
        private static string connStr => ConfigurationManager.AppSettings["mongoConnStr"];
        private static MongoClient dbClient = null;
        public static MongoClient MongoClientInstance
        {
            get
            {
                if (dbClient == null)
                {
                    MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(connStr));
                    //settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
                    settings.ConnectTimeout = TimeSpan.FromMinutes(1);
                    settings.MaxConnectionIdleTime = TimeSpan.FromMinutes(10);
                    dbClient = new MongoClient(settings);
                }
                return dbClient;
            }
        }
    }
}
