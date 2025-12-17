using FluentValidation;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Validators
{
    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {
            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("Müşteri adı boş olamaz.")
                .MaximumLength(100).WithMessage("Müşteri adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.CustomerEmail)
                .NotEmpty().WithMessage("E-posta adresi boş olamaz.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(255).WithMessage("E-posta adresi en fazla 255 karakter olabilir.");

            RuleFor(x => x.CustomerPhone)
                .Matches(@"^[0-9]{10,15}$").When(x => !string.IsNullOrEmpty(x.CustomerPhone))
                .WithMessage("Telefon numarası 10-15 rakam içermelidir.");

            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("Teslimat adresi boş olamaz.")
                .MaximumLength(500).WithMessage("Teslimat adresi en fazla 500 karakter olabilir.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Sipariş en az bir ürün içermelidir.");

            RuleForEach(x => x.Items).SetValidator(new OrderItemDtoValidator());
        }
    }

    public class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
    {
        public OrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Ürün ID boş olamaz.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Adet 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(1000).WithMessage("Tek seferde en fazla 1000 adet sipariş verilebilir.");
        }
    }
}
