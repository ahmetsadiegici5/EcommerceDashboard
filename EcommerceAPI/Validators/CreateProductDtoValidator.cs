using FluentValidation;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Validators
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ürün adı boş olamaz.")
                .MaximumLength(200).WithMessage("Ürün adı 200 karakterden uzun olamaz.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stok 0'dan küçük olamaz.");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Kategori boş olamaz.");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU boş olamaz.");
        }
    }
}
