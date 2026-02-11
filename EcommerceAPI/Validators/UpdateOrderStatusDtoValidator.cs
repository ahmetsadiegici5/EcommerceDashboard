using FluentValidation;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Validators
{
    public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        public static readonly string[] AllowedStatuses =
        {
            "Pending", "Processing", "Shipped", "Delivered", "Cancelled"
        };

        public UpdateOrderStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Sipariş durumu boş olamaz.")
                .Must(status => AllowedStatuses.Contains(status))
                .WithMessage($"Geçerli durumlar: {string.Join(", ", AllowedStatuses)}");
        }
    }
}
