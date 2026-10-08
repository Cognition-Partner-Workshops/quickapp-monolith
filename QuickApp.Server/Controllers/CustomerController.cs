// ---------------------------------------
// Email: quickapp@ebenmonney.com
// Templates: www.ebenmonney.com/templates
// (c) 2024 www.ebenmonney.com/mit-license
// ---------------------------------------

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using QuickApp.Core.Services;
using QuickApp.Core.Services.Shop;
using QuickApp.Server.Attributes;
using QuickApp.Server.Services.Email;
using QuickApp.Server.ViewModels.Shop;

namespace QuickApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [OrderServiceUnavailable]
    public class CustomerController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IEmailSender _emailSender;
        private readonly ICustomerService _customerService;
        private readonly IOrdersService _ordersService;

        public CustomerController(IMapper mapper, ILogger<CustomerController> logger, IEmailSender emailSender,
            ICustomerService customerService, IOrdersService ordersService)
        {
            _mapper = mapper;
            _logger = logger;
            _emailSender = emailSender;
            _customerService = customerService;
            _ordersService = ordersService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var customers = _mapper.Map<List<CustomerVM>>(_customerService.GetAllCustomersData());

            var ordersByCustomer = (await _ordersService.GetOrdersAsync(cancellationToken: cancellationToken))
                .ToLookup(o => o.CustomerId);

            foreach (var customer in customers)
                customer.Orders = _mapper.Map<List<OrderVM>>(ordersByCustomer[customer.Id]);

            return Ok(customers);
        }

        [HttpGet("throw")]
        public IEnumerable<CustomerVM> Throw()
        {
            throw new CustomerException($"This is a test exception: {DateTime.Now}");
        }

        [HttpGet("email")]
        public async Task<string> Email()
        {
            var recipientName = "QickApp Tester"; //         <===== Put the recipient's name here
            var recipientEmail = "test@ebenmonney.com"; //   <===== Put the recipient's email here

            var message = EmailTemplates.GetTestEmail(recipientName, DateTime.UtcNow);

            (var success, var errorMsg) = await _emailSender.SendEmailAsync(recipientName, recipientEmail,
                "Test Email from QuickApp", message);

            if (success)
                return "Success";

            return $"Error: {errorMsg}";
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return $"value: {id}";
        }

        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
