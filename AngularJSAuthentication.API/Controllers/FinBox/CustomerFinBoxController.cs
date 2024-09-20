using AngularJSAuthentication.API.App_Code.FinBox;
using AngularJSAuthentication.BusinessLayer.FinBox;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.FinBox
{
    [RoutePrefix("api/FinBox")]
    
    public class CustomerFinBoxController : ApiController
    {
        private FinBoxRepository FinBoxRepository;
        public CustomerFinBoxController()
        {
            this.FinBoxRepository = new FinBoxRepository(new AuthContext());
        }

        public CustomerFinBoxController(FinBoxRepository FinBoxRepository)
        {
            this.FinBoxRepository = FinBoxRepository;
        }

        [Route("CreateUser")]
        [HttpPost]
        public IHttpActionResult CreateUser(CreateUserDC createUserDC)
        {
            ResponseDC Result = FinBoxRepository.CreateUser(createUserDC);
            return Ok(Result);
        }

        [Route("Eligiblility")]
        [HttpGet]
        public IHttpActionResult Eligiblility(int CustomerId, bool isCreditLine)
        {
            EligibleResponseDC ObjeligibleResponseDC = FinBoxRepository.Eligiblity(CustomerId, isCreditLine);
            return Ok(ObjeligibleResponseDC);
        }

        [Route("GenerateToken")]
        [HttpPost]
        public IHttpActionResult GenerateToken(int CustomerId, bool isCreditLine)
        {
            TokenDC ObjTokenDC = FinBoxRepository.GenerateToken(CustomerId,isCreditLine);
            return Ok(ObjTokenDC);
        }

        [Route("IsFinBox")]
        [HttpGet]
        public IHttpActionResult IsFinBox(int CustomerId,bool isCreditLine)
        {
            bool Result = FinBoxRepository.IsFinBox(CustomerId, isCreditLine);
            return Ok(Result);
        }

        [Route("GenerateWebhook")]
        [HttpPost]
        public IHttpActionResult GenerateToken(WebhookDC webhookDC)
        {
            webhookDC.IsCreditLine = false;
            bool Result = FinBoxRepository.GenerateWebhook(webhookDC);
            return Ok(Result);
        }

        [Route("GenerateCreditLineWebhook")]
        [HttpPost]
        public IHttpActionResult GenerateCreditLineWebhook(WebhookDC webhookDC)
        {
            webhookDC.IsCreditLine = true;
            bool Result = FinBoxRepository.GenerateWebhook(webhookDC);
            return Ok(Result);
        }

        [Route("IsEligibilityAlreadycalculated")]
        [HttpGet]
        public IHttpActionResult IsEligibilityAlreadycalculated(int CustomerId, bool isCreditLine)
        {
            bool Result = FinBoxRepository.IsEligibilityAlreadycalculated(CustomerId, isCreditLine);
            return Ok(Result);
        }

        [Route("GetCustomerCreditLimit")]
        [HttpGet]
        public async Task<CreditLimitData> GetCustomerCreditLimit(int CustomerId, bool isCreditLine)
        {
            return await FinBoxRepository.GetCustomerCreditLimit(CustomerId, isCreditLine);
            
        }
    }
}
