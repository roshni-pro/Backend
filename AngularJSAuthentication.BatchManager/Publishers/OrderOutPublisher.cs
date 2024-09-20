using AngularJSAuthentication.BatchManager.Constants;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions.BatchCode;
using AngularJSAuthentication.DataContracts.BatchCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager.Publishers
{
    public class OrderOutPublisher
    {
        public bool PublishOrderOut(List<BatchCodeSubjectDc> subjectList)
        {
            BatchCodeSubjectManager manager = new BatchCodeSubjectManager();
            if (subjectList != null && subjectList.Count > 0)
            {
                Publisher publisher = new Publisher();
                List<BatchCodeSubject> batchCodeSubjectList = new List<BatchCodeSubject>();
                foreach (var item in subjectList)
                {
                    publisher.PublishInBatchCode(new BatchCodeSubject
                    {
                        ItemMultiMrpId = item.ItemMultiMrpId,
                        ObjectDetailId = item.ObjectDetailId,
                        ObjectId = item.ObjectId,
                        Quantity = item.Quantity,
                        TransactionDate = DateTime.Now,
                        TransactionType = manager.GetTransactionTypeOrderOut(item.StockType),
                        WarehouseId = item.WarehouseId
                    });
                }

            }

            return true;
        }
        public bool PlannedPublish(List<BatchCodeSubjectDc> subjectList)
        {
            BatchCodeSubjectManager manager = new BatchCodeSubjectManager();
            if (subjectList != null && subjectList.Count > 0)
            {
                Publisher publisher = new Publisher();
                List<BatchCodeSubject> batchCodeSubjectList = new List<BatchCodeSubject>();
                foreach (var item in subjectList)
                {
                    publisher.PublishInBatchCode(new BatchCodeSubject
                    {
                        ItemMultiMrpId = item.ItemMultiMrpId,
                        ObjectDetailId = item.ObjectDetailId,
                        ObjectId = item.ObjectId,
                        Quantity = item.Quantity,
                        TransactionDate = DateTime.Now,
                        TransactionType = manager.GetTransactionTypePlannedOut(item.StockType),
                        WarehouseId = item.WarehouseId
                    });
                }
            }
            return true;
        }
        public bool PlannedRejectPublish(List<BatchCodeSubjectDc> subjectList)
        {
            BatchCodeSubjectManager manager = new BatchCodeSubjectManager();
            if (subjectList != null && subjectList.Count > 0)
            {
                Publisher publisher = new Publisher();
                List<BatchCodeSubject> batchCodeSubjectList = new List<BatchCodeSubject>();
                foreach (var item in subjectList)
                {
                    publisher.PublishInBatchCode(new BatchCodeSubject
                    {
                        ItemMultiMrpId = item.ItemMultiMrpId,
                        ObjectDetailId = item.ObjectDetailId,
                        ObjectId = item.ObjectId,
                        Quantity = item.Quantity,
                        TransactionDate = DateTime.Now,
                        TransactionType = manager.GetTransactionTypePlannedReject(item.StockType),
                        WarehouseId = item.WarehouseId
                    });
                }
            }
            return true;
        }
        public bool PublishOrderIn(List<BatchCodeSubjectDc> subjectList)
        {
            BatchCodeSubjectManager manager = new BatchCodeSubjectManager();
            if (subjectList != null && subjectList.Count > 0)
            {
                Publisher publisher = new Publisher();
                List<BatchCodeSubject> batchCodeSubjectList = new List<BatchCodeSubject>();
                foreach (var item in subjectList)
                {
                    publisher.PublishInBatchCode(new BatchCodeSubject
                    {
                        ItemMultiMrpId = item.ItemMultiMrpId,
                        ObjectDetailId = item.ObjectDetailId,
                        ObjectId = item.ObjectId,
                        Quantity = item.Quantity,
                        TransactionDate = DateTime.Now,
                        TransactionType = manager.GetTransactionTypeOrderIn(item.StockType),
                        WarehouseId = item.WarehouseId
                    });
                }

            }

            return true;
        }
        public bool PublishOrderInvoiceQueue(List<BatchCodeSubjectDc> subjectList)
        {
            BatchCodeSubjectManager manager = new BatchCodeSubjectManager();
            if (subjectList != null && subjectList.Count > 0)
            {
                Publisher publisher = new Publisher();
                List<BatchCodeSubject> batchCodeSubjectList = new List<BatchCodeSubject>();
                foreach (var item in subjectList)
                {
                    publisher.PublishInBatchCode(new BatchCodeSubject
                    {
                        ItemMultiMrpId = item.ItemMultiMrpId,
                        ObjectDetailId = item.ObjectDetailId,
                        ObjectId = item.ObjectId,
                        Quantity = item.Quantity,
                        TransactionDate = DateTime.Now,
                        TransactionType = "OrderInvoiceQueue",
                        WarehouseId = item.WarehouseId
                    }, Queues.OrderInvoiceQueue);
                }
            }
            return true;
        }
        public bool PublishBackendOrderQueue(List<BatchCodeSubjectDc> subjectList)
        {
            BatchCodeSubjectManager manager = new BatchCodeSubjectManager();
            if (subjectList != null && subjectList.Count > 0)
            {
                Publisher publisher = new Publisher();
                List<BatchCodeSubject> batchCodeSubjectList = new List<BatchCodeSubject>();
                foreach (var item in subjectList)
                {
                    publisher.PublishInBatchCode(new BatchCodeSubject
                    {
                        ItemMultiMrpId = item.ItemMultiMrpId,
                        ObjectDetailId = item.ObjectDetailId,
                        ObjectId = item.ObjectId,
                        Quantity = item.Quantity,
                        TransactionDate = DateTime.Now,
                        TransactionType = "BackendOrderQueue",
                        WarehouseId = item.WarehouseId
                    }, Queues.BackendOrderQueue);
                }
            }
            return true;
        }
        public bool PublishZilaOrderQueue(List<BatchCodeSubjectDc> subjectList)
        {
            BatchCodeSubjectManager manager = new BatchCodeSubjectManager();
            if (subjectList != null && subjectList.Count > 0)
            {
                Publisher publisher = new Publisher();
                List<BatchCodeSubject> batchCodeSubjectList = new List<BatchCodeSubject>();
                foreach (var item in subjectList)
                {
                    publisher.PublishInBatchCode(new BatchCodeSubject
                    {
                        ItemMultiMrpId = item.ItemMultiMrpId,
                        ObjectDetailId = item.ObjectDetailId,
                        ObjectId = item.ObjectId,
                        Quantity = item.Quantity,
                        TransactionDate = DateTime.Now,
                        TransactionType = "ZilaOrderQueue",
                        WarehouseId = item.WarehouseId
                    }, Queues.ZilaOrderQueue);
                }
            }
            return true;
        }
    }
}
