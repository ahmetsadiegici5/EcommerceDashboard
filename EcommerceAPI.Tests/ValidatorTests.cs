using EcommerceAPI.DTOs;
using EcommerceAPI.Validators;
using Xunit;

namespace EcommerceAPI.Tests
{
    public class ValidatorTests
    {
        private readonly UpdateOrderStatusDtoValidator _statusValidator = new();

        [Theory]
        [InlineData("Pending")]
        [InlineData("Shipped")]
        [InlineData("Delivered")]
        public void UpdateOrderStatus_Accepts_Known_Statuses(string status)
        {
            var result = _statusValidator.Validate(new UpdateOrderStatusDto { Status = status });

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("shipped")]
        [InlineData("Lost")]
        public void UpdateOrderStatus_Rejects_Unknown_Statuses(string status)
        {
            var result = _statusValidator.Validate(new UpdateOrderStatusDto { Status = status });

            Assert.False(result.IsValid);
        }
    }
}
