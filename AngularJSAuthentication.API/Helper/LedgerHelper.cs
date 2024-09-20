using AngularJSAuthentication.Model.Account;
using System;
using System.Linq;

namespace AngularJSAuthentication.API.Helper
{
    public class LedgerHelper
    {
        public LadgerType GetOrCreateLadgerType(string code, int userid)
        {
            using (var authContext = new AuthContext())
            {
                LadgerType ladgerType = authContext.LadgerTypeDB.Where(x => !string.IsNullOrEmpty(x.code) && x.code.ToLower() == code.ToLower()).FirstOrDefault();

                if (ladgerType == null)
                {
                    ladgerType = new LadgerType();
                    ladgerType.Active = true;
                    ladgerType.code = code;
                    ladgerType.CreatedBy = userid;
                    ladgerType.CreatedDate = DateTime.Now;
                    ladgerType.Name = code;
                    ladgerType.Sequence = null;
                    ladgerType.UpdatedBy = userid;
                    ladgerType.UpdatedDate = DateTime.Now;
                    authContext.LadgerTypeDB.Add(ladgerType);
                    authContext.Commit();
                }
                return ladgerType;
            }
        }

        public Ladger GetOrCreateLadger(int ledgerTypeId, int objectId, string name, int userid, string objectType)
        {
            using (var context = new AuthContext())
            {
                Ladger ledger = context.LadgerDB.Where(x => x.LadgertypeID == ledgerTypeId && x.Name == name).FirstOrDefault();
                if (ledger == null)
                {
                    ledger = new Ladger
                    {
                        Active = true,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        LadgertypeID = ledgerTypeId,
                        ObjectID = objectId,
                        ObjectType = objectType,
                        UpdatedBy = userid,
                        UpdatedDate = DateTime.Now,
                        Name = name,
                        InventoryValuesAreAffected = false,
                        ProvidedBankDetails = false
                    };
                    context.LadgerDB.Add(ledger);
                    context.Commit();
                }

                return ledger;
            }
        }


    }
}