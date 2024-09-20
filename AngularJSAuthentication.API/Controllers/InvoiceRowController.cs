using AngularJSAuthentication.Model;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/InvoiceDetails")]
    public class InvoiceRowController : ApiController
    {        

        //[Authorize]
        //[Route("")]
        //public IEnumerable<AllInvoice> Get()
        //{
        //    //return Helper.CreateProjects().AsEnumerable();

        //    return context.AllInvoice;
        //}



        [ResponseType(typeof(InvoiceRow))]
        [Route("")]
        [AcceptVerbs("POST")]
        public IEnumerable<InvoiceRow> Post([FromBody]IEnumerable<InvoiceRow> pList)
        {
            using (var context = new AuthContext())//PZ
            {
                int j = pList.Count();
                j = j - 1;
                var id = pList.Last();
                pList = pList.Take(pList.Count() - 1);
                foreach (InvoiceRow model in pList)
                {

                    InvoiceRow e = new InvoiceRow();

                    e.invoicedetail_id = model.invoicedetail_id;
                    e.invoice_id = id.invoice_id;
                    e.Quantity = model.Quantity;
                    e.unit = model.unit;
                    e.Amount = model.Amount;
                    e.Desc = model.Desc;
                    e.product = model.product;



                    context.AddInvoiceDetail(e);


                }
                return pList;
            }
        }



    }


}



