using AngularJSAuthentication.BusinessLayer.Ecollection;
using System;
using System.Transactions;

namespace AngularJSAuthentication.API.App_Code.ECollection
{
    public class MisRepository: IDisposable
    {
       
        private bool disposed = false;
        private AuthContext Context;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public MisRepository(AuthContext Context)
        {
            this.Context = Context;
        }
        public Response PushMisData(MISData objMISData)
        {
            try
            {
                Response objResponse;
                using (TransactionScope scope=new TransactionScope())
                {
                    if (checkDataExists(objMISData.UniqueID, objMISData))
                    {
                        objResponse = new Response
                        {
                            Status = "1",
                            Reason = "Already Processed",
                            UniqueID = objMISData.UniqueID
                        };

                       
                    }
                    else

                    {
                        objMISData.Status = 0;
                        objMISData.Reason = "Success";
                        Context.MISData.Add(objMISData);
                        if (Context.Commit() > 0)
                        {
                            objResponse = new Response
                            {
                                Status = "0",
                                Reason = "Success",
                                UniqueID = objMISData.UniqueID
                            };
                        }
                        else
                        {
                            objResponse = new Response
                            {
                                Status = "2",
                                Reason = "Re-push",
                                UniqueID = objMISData.UniqueID
                            };
                        }
                    }
                    scope.Complete();
                }
                return objResponse;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                Dispose();
            }
        }

        private bool checkDataExists(string UniqueId, MISData mISData)
        {
            bool Result = false;

             mISData = Context.MISData.Find(UniqueId);
            if (mISData!=null)
            {
                Result = true;
            }
            return Result;
        }
        
    }
}