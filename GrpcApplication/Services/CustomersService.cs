using Grpc.Core;

namespace GrpcApplication.Services
{
    public class CustomersService : Customer.CustomerBase
    {
        private List<CustomerModel> _customers = new List<CustomerModel>()
        {
            new CustomerModel()
            {
                Id = "1",
                FirstName = "Alex",
                LastName = "Newman",
                EmailAddress = "newman.alex@gmail.ru",
                Age= 24
            },
            new CustomerModel()
            {
                Id = "2",
                FirstName = "Nikola",
                LastName = "Tesla",
                EmailAddress = "nikola@gmail.ru",
                Age= 25
            },
            new CustomerModel()
            {
                Id = "3",
                FirstName = "Marisha",
                LastName = "Anishkina",
                EmailAddress = "amar@gmail.ru",
                Age= 19
            },
            new CustomerModel()
            {
                Id = "4",
                FirstName = "Nicole",
                LastName = "Newman",
                EmailAddress = "newman.nicole@gmail.ru",
                Age= 22
            },
        };

        public override Task<CustomerModel> GetCustomerInfo(CustomerLookupModel request, ServerCallContext context)
        {
            var foundCustomer = _customers.FirstOrDefault(c => c.Id.Equals(request.UserId.ToString()));

            if (foundCustomer == null)
            {
                return Task.FromResult(new CustomerModel());
            }

            return Task.FromResult(foundCustomer);
        }
    }
}
