using OfficeOpenXml;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services
{
    public class ExcelService
    {
        public ExcelService()
        {
            // EPPlus lisans ayarı (NonCommercial/Educational)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        #region Product Import/Export

        public async Task<List<Product>> ImportProductsFromExcel(Stream excelStream, string sellerId)
        {
            var products = new List<Product>();

            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                for (int row = 2; row <= rowCount; row++) // 1. satır başlık
                {
                    var product = new Product
                    {
                        Name = worksheet.Cells[row, 1].Value?.ToString() ?? "",
                        Description = worksheet.Cells[row, 2].Value?.ToString() ?? "",
                        Price = double.Parse(worksheet.Cells[row, 3].Value?.ToString() ?? "0"),
                        Stock = int.Parse(worksheet.Cells[row, 4].Value?.ToString() ?? "0"),
                        Category = worksheet.Cells[row, 5].Value?.ToString() ?? "",
                        SKU = worksheet.Cells[row, 6].Value?.ToString() ?? "",
                        ImageUrl = worksheet.Cells[row, 7].Value?.ToString(),
                        SellerId = sellerId,
                        IsActive = bool.Parse(worksheet.Cells[row, 8].Value?.ToString() ?? "true")
                    };

                    products.Add(product);
                }
            }

            return products;
        }

        public async Task<byte[]> ExportProductsToExcel(List<Product> products)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Ürünler");

                // Başlıklar
                worksheet.Cells[1, 1].Value = "Ürün Adı";
                worksheet.Cells[1, 2].Value = "Açıklama";
                worksheet.Cells[1, 3].Value = "Fiyat";
                worksheet.Cells[1, 4].Value = "Stok";
                worksheet.Cells[1, 5].Value = "Kategori";
                worksheet.Cells[1, 6].Value = "SKU";
                worksheet.Cells[1, 7].Value = "Resim URL";
                worksheet.Cells[1, 8].Value = "Aktif";
                worksheet.Cells[1, 9].Value = "ID";

                // Başlık formatı
                using (var range = worksheet.Cells[1, 1, 1, 9])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Veriler
                for (int i = 0; i < products.Count; i++)
                {
                    var row = i + 2;
                    worksheet.Cells[row, 1].Value = products[i].Name;
                    worksheet.Cells[row, 2].Value = products[i].Description;
                    worksheet.Cells[row, 3].Value = products[i].Price;
                    worksheet.Cells[row, 4].Value = products[i].Stock;
                    worksheet.Cells[row, 5].Value = products[i].Category;
                    worksheet.Cells[row, 6].Value = products[i].SKU;
                    worksheet.Cells[row, 7].Value = products[i].ImageUrl;
                    worksheet.Cells[row, 8].Value = products[i].IsActive;
                    worksheet.Cells[row, 9].Value = products[i].Id;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return await package.GetAsByteArrayAsync();
            }
        }

        public async Task<byte[]> GenerateProductTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Ürün Şablonu");

                // Başlıklar
                worksheet.Cells[1, 1].Value = "Ürün Adı *";
                worksheet.Cells[1, 2].Value = "Açıklama";
                worksheet.Cells[1, 3].Value = "Fiyat *";
                worksheet.Cells[1, 4].Value = "Stok *";
                worksheet.Cells[1, 5].Value = "Kategori *";
                worksheet.Cells[1, 6].Value = "SKU *";
                worksheet.Cells[1, 7].Value = "Resim URL";
                worksheet.Cells[1, 8].Value = "Aktif (true/false)";

                // Örnek veri
                worksheet.Cells[2, 1].Value = "Örnek Ürün";
                worksheet.Cells[2, 2].Value = "Bu bir örnek ürün açıklamasıdır";
                worksheet.Cells[2, 3].Value = 99.99;
                worksheet.Cells[2, 4].Value = 100;
                worksheet.Cells[2, 5].Value = "Elektronik";
                worksheet.Cells[2, 6].Value = "SKU-12345";
                worksheet.Cells[2, 7].Value = "https://example.com/image.jpg";
                worksheet.Cells[2, 8].Value = "true";

                // Başlık formatı
                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return await package.GetAsByteArrayAsync();
            }
        }

        #endregion

        #region Shipping Import/Export

        public async Task<List<Shipping>> ImportShippingFromExcel(Stream excelStream)
        {
            var shippingList = new List<Shipping>();

            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                for (int row = 2; row <= rowCount; row++)
                {
                    var shipping = new Shipping
                    {
                        OrderId = worksheet.Cells[row, 1].Value?.ToString() ?? "",
                        TrackingNumber = worksheet.Cells[row, 2].Value?.ToString() ?? "",
                        Carrier = worksheet.Cells[row, 3].Value?.ToString() ?? "",
                        Status = worksheet.Cells[row, 4].Value?.ToString() ?? "Preparing",
                        CurrentLocation = worksheet.Cells[row, 5].Value?.ToString()
                    };

                    shippingList.Add(shipping);
                }
            }

            return shippingList;
        }

        public async Task<byte[]> ExportShippingToExcel(List<Shipping> shippingList)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Kargo Takip");

                // Başlıklar
                worksheet.Cells[1, 1].Value = "Sipariş ID";
                worksheet.Cells[1, 2].Value = "Takip Numarası";
                worksheet.Cells[1, 3].Value = "Kargo Firması";
                worksheet.Cells[1, 4].Value = "Durum";
                worksheet.Cells[1, 5].Value = "Mevcut Konum";
                worksheet.Cells[1, 6].Value = "Oluşturma Tarihi";
                worksheet.Cells[1, 7].Value = "Tahmini Teslimat";

                // Başlık formatı
                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Veriler
                for (int i = 0; i < shippingList.Count; i++)
                {
                    var row = i + 2;
                    worksheet.Cells[row, 1].Value = shippingList[i].OrderId;
                    worksheet.Cells[row, 2].Value = shippingList[i].TrackingNumber;
                    worksheet.Cells[row, 3].Value = shippingList[i].Carrier;
                    worksheet.Cells[row, 4].Value = shippingList[i].Status;
                    worksheet.Cells[row, 5].Value = shippingList[i].CurrentLocation;
                    worksheet.Cells[row, 6].Value = shippingList[i].CreatedAt.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 7].Value = shippingList[i].EstimatedDeliveryDate?.ToString("dd/MM/yyyy");
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return await package.GetAsByteArrayAsync();
            }
        }

        public async Task<byte[]> GenerateShippingTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Kargo Şablonu");

                // Başlıklar
                worksheet.Cells[1, 1].Value = "Sipariş ID *";
                worksheet.Cells[1, 2].Value = "Takip Numarası *";
                worksheet.Cells[1, 3].Value = "Kargo Firması *";
                worksheet.Cells[1, 4].Value = "Durum";
                worksheet.Cells[1, 5].Value = "Mevcut Konum";

                // Örnek veri
                worksheet.Cells[2, 1].Value = "ORD-12345";
                worksheet.Cells[2, 2].Value = "TK123456789";
                worksheet.Cells[2, 3].Value = "Aras Kargo";
                worksheet.Cells[2, 4].Value = "Shipped";
                worksheet.Cells[2, 5].Value = "İstanbul";

                // Başlık formatı
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return await package.GetAsByteArrayAsync();
            }
        }

        #endregion
    }
}
