using EcommerceAPI.Models;

namespace EcommerceAPI.Services
{
    public interface IExcelService
    {
        Task<List<Product>> ImportProductsFromExcel(Stream excelStream, string sellerId);
        Task<byte[]> ExportProductsToExcel(List<Product> products);
        Task<byte[]> GenerateProductTemplate();
    }
}
