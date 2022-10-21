using System.Drawing;

namespace Test_InventoryPage
{
    public class ViewModels
    {
        public class InventoryViewModel
        {
            public string? itemId { get; set; }
            public string? itemName { get; set; }
            public string? department { get; set; }
            public double? departmentId { get; set; }
            public string? category { get; set; }
            public double? categoryId { get; set; }
            public double? inStock { get; set; }
            public double? inRepair { get; set; }
            public double? storeId { get; set; }
            public string? storeName { get; set; }
            public double? ebayPrice { get; set; }
            public double? amazonPrice { get; set; }
            public double? sellPrice { get; set; }
        }
        public class GetProductRequestViewModel
        {
            public string? itemId { get; set; }
            public string? itemName { get; set; }
            public double? sellPrice { get; set; }
        }
        public class InventoryBarcodeViewModel
        {
            public string? itemId { get; set; }
            public string? itemName { get; set; }
            public string? storeName { get; set; }
            public double? sellPrice { get; set; }
            public double? storeId { get; set; }
            public string? productBarcode { get; set; }
        }

        public class SaleInfoViewModel
        {
            public double? id { get; set; }
            public string? itemId { get; set; }
            public string? itemName { get; set; }
            public bool tradeIn { get; set; }
            public double? quantity { get; set; }
            public double? discount { get; set; }
            public double? sellPrice { get; set; }
            public double? grandTotal { get; set; }
            public double? storeId { get; set; }
            public bool toRepair { get; set; }
            public bool isDiscount { get; set; }
        }
        public class DepartmentViewModel
        {
            public int id { get; set; }
            public string? description { get; set; }
        }

        public class CategoryViewModel
        {
            public int id { get; set; }
            public string? description { get; set; }
        }

        public class WeeklyReportViewModel
        {
            public string? id { get; set; }
            public string? name { get; set; }
            public double? count { get; set; }
            public string? image { get; set; }
        }

        public class StoreViewModel
        {
            public int id { get; set; }
            public string? description { get; set; }
            public string? website { get; set; }
            public string? phoneNumber { get; set; }
            public int? storeGroupId { get; set; }
            public double? taxRate { get; set; }
        }

        public class MessageViewModel
        {
            public double? id { get; set; }
            //public string? fromStore { get; set; }
            //public string? toStore { get; set; }
            public string? message { get; set; }
        }

        public class CashBoxViewModel
        {
            public double? cashbox { get; set; }
            public double? envelope { get; set; }
            public double? dailyEarnings { get; set; }
        }

        public class ItemDetailViewModel
        {
            public double itemId { get; set; }
            public string? itemName { get; set; }
        }

        public class ItemInfoViewModel
        {
            public string? itemId { get; set; }
            public string? itemName { get; set; }
            public double? sellPrice { get; set; }
        }

        public class PersonnelInformationViewModel
        {
            public string? personnelCode { get; set; }
            public double? storeId { get; set; }
        }

        public class ReportStoreViewModel
        {
            public double? storeId { get; set; }
            public double? sellPrice { get; set; }
            public double? subTotal { get; set; }
            public double? taxTotal { get; set; }
            public double? grandTotal { get; set; }
            public DateTime? getDate { get; set; }
            public string? storeName { get; set; }
        }

        public class ReportPersonnelViewModel
        {
            public string? personnelCode { get; set; }
            public double? sellPrice { get; set; }
            public double? subTotal { get; set; }
            public double? taxTotal { get; set; }
            public double? grandTotal { get; set; }
            public DateTime? getDate { get; set; }
            public string? clockIn { get; set; }
            public string? clockOut { get; set; }
            public string? workingHour { get; set; }
            public string? personnelName { get; set; }
        }
        public class ReportInvoiceViewModel
        {
            public double? invoiceId { get; set; }
            public double? sellPrice { get; set; }
            public double? subTotal { get; set; }
            public double? taxTotal { get; set; }
            public double? grandTotal { get; set; }
            public DateTime? getDate { get; set; }
            public string? customerPhone { get; set; }
        }
        public class ReportInvoiceDetailViewModel
        {
            public double? invoiceId { get; set; }
            public double? productId { get; set; }
            public string? productName { get; set; }
            public double? quantity { get; set; }
            public string? personnelName { get; set; }
            public string? storeName { get; set; }
            public string? paymentMethod { get; set; }
            public double? sellPrice { get; set; }
            public double? subTotal { get; set; }
            public double? taxTotal { get; set; }
            public double? grandTotal { get; set; }
            public double? customerPhone { get; set; }
            public DateTime? getDate { get; set; }
        }

        public class ReportCustomerViewModel
        {
            public double? id { get; set; }
            public string? firstName { get; set; }
            public string? lastName { get; set; }
            public string? email { get; set; }
            public string? phoneNumber { get; set; }
            public DateTime? getDate { get; set; }
        }

        public class ReportStockViewModel
        {
            public double? itemId { get; set; }
            public string? itemName { get; set; }
            public double? stock { get; set; }
            public double? quantity { get; set; }
            public double? currentStock { get; set; }
            public double? sellPrice { get; set; }
            public string? store { get; set; }
            public DateTime? getDate { get; set; }
            public string? getDateHour { get; set; }
        }

        public class ReportProductViewModel
        {
            public double? itemId { get; set; }
            public string? itemName { get; set; }
            public double? quantity { get; set; }
            public double? dateRange { get; set; }
        }

        public class ReportWeeklyOrderViewModel
        {
            public double? itemId { get; set; }
            public string? itemName { get; set; }
            public double? itemCount { get; set; }
            public double? inStock { get; set; }
            public double? itemOrder { get; set; }
            public string? store { get; set; }
            public DateTime? getDate { get; set; }
        }

        public class ReportUndercountViewModel
        {
            public string? itemId { get; set; }
            public string? itemName { get; set; }
            public double? undercount { get; set; }
            public string? store { get; set; }
            public DateTime? getDate { get; set; }
        }

        public class ReportProductRequestViewModel
        {
            public double? itemId { get; set; }
            public string? itemName { get; set; }
            public string? store { get; set; }
            public string? personnelName { get; set; }
            public DateTime? getDate { get; set; }
        }

        public class ReportAccessibilityViewModel
        {
            public string? personnelCode { get; set; }
            public string? storeName { get; set; }
            public string? permission { get; set; }
            public bool? store { get; set; }
            public bool? cashier { get; set; }
            public bool? invoice { get; set; }
            public bool? customer { get; set; }
            public bool? stock { get; set; }
            public bool? salesAnalysis { get; set; }
            public bool? inventoryOrder { get; set; }
            public bool? undercount { get; set; }
            public bool? productRequest { get; set; }
        }

        public class DefinitionAccessibilityViewModel
        {
            public string? personnelCode { get; set; }
            public string? storeName { get; set; }
            public string? permission { get; set; }
            public bool? document { get; set; }
            public bool? category { get; set; }
            public bool? personnel { get; set; }
            public bool? store { get; set; }
            public bool? product { get; set; }
        }

        public class SettingAccessibilityViewModel
        {
            public string? personnelCode { get; set; }
            public string? storeName { get; set; }
            public string? permission { get; set; }
            public bool? price { get; set; }
            public bool? inventoryOrder { get; set; }
            public bool? promotions { get; set; }
        }

        public class ReportPermissionViewModel
        {
            public string? personnelCode { get; set; }
            public string? storeName { get; set; }
            public bool? store { get; set; }
            public bool? cashier { get; set; }
            public bool? invoice { get; set; }
            public bool? customer { get; set; }
            public bool? stock { get; set; }
            public bool? salesAnalysis { get; set; }
            public bool? inventoryOrder { get; set; }
            public bool? undercount { get; set; }
            public bool? productRequest { get; set; }
        }

        public class DefinitionPermissionViewModel
        {
            public string? personnelCode { get; set; }
            public string? storeName { get; set; }
            public bool? documentary { get; set; }
            public bool? category { get; set; }
            public bool? personnel { get; set; }
            public bool? store { get; set; }
        }

        public class DefinitionInventoryViewModel
        {
            public string? itemId { get; set; }
            public string? itemName { get; set; }
            public string? department { get; set; }
            public double? departmentId { get; set; }
            public string? category { get; set; }
            public double? categoryId { get; set; }
            public double? inStock { get; set; }
            public double? inRepair { get; set; }
            public double? storeId { get; set; }
            public string? storeName { get; set; }
            public double? ebayPrice { get; set; }
            public double? amazonPrice { get; set; }
            public double? sellPrice { get; set; }
        }

        public class SettingPermissionViewModel
        {
            public string? personnelCode { get; set; }
            public string? storeName { get; set; }
            public bool? promotions { get; set; }
            public bool? inventoryOrder { get; set; }
            public bool? price { get; set; }
            public bool? discount { get; set; }
        }

        public class DefinitionViewModel
        {
            public string? description { get; set; }
            public string? website { get; set; }
            public string? phoneNumber { get; set; }
            public double? storeGroupId { get; set; }
            public double? taxRate { get; set; }
        }
        public class PersonnelViewModel
        {
            public string? personnelCode { get; set; }
            public string? personnelName { get; set; }
            public string? storeName { get; set; }
            public DateTime hireDate { get; set; }
            public string? permission { get; set; }
            public string? factorId { get; set; }
        }


        public class InvoiceHeaderViewModel
        {
            public double? invoiceId { get; set; }
            public string? personnelId { get; set; }
            public double? storeId { get; set; }
            public string? paymentMethod { get; set; }
            public string? giftCode { get; set; }
            public double? subTotal { get; set; }
            public double? taxTotal { get; set; }
            public double? grandTotal { get; set; }
            public DateTime invoiceDate { get; set; }
            public double? storeGroupId { get; set; }
            public string? storeName { get; set; }
        }

        public class InvoiceBodyViewModel
        {
            public double? productId { get; set; }
            public string? productName { get; set; }
            public double? quantity { get; set; }
            public double? unitPrice { get; set; }
            public double? totalPrice { get; set; }
        }

        public class SettingUpdatePrice
        {
            public double? id { get; set; }
            public string? description { get; set; }
        }

        public class PermissionLookupViewModel
        {
            public double? id { get; set; }
            public string? description { get; set; }
        }
        public class StoreLookupViewModel
        {
            public double? id { get; set; }
            public string? description { get; set; }
        }
        public class StoreGroupLookupViewModel
        {
            public double? id { get; set; }
            public string? description { get; set; }
        }
        public class PersonnelLookupViewModel
        {
            public string? personnelCode { get; set; }
            public string? personnelName { get; set; }
        }

        public class SettingPriceViewModel
        {
            public double? id { get; set; }
            public string? category { get; set; }
            public double? sellPrice { get; set; }
            public double? ebayPrice { get; set; }
            public double? amazonPrice { get; set; }
            public DateTime createDate { get; set; }
            public DateTime updateDate { get; set; }
        }

        public class SettingWeeklyInventoryViewModel
        {
            public string? id { get; set; }
            public string? itemName { get; set; }
            public bool? isSelected { get; set; }
            public string? productGroup { get; set; }
            public double? itemOrder { get; set; }
            public DateTime createDate { get; set; }
            public string? storeName { get; set; }
            public string? category { get; set; }
        }

        public class SettingPromotionViewModel
        {
            public double? id { get; set; }
            public string? promotionName { get; set; }
            public DateTime? startDate { get; set; }
            public DateTime? endDate { get; set; }
            public DateTime createDate { get; set; }
        }
        public class SettingDiscountViewModel
        {
            public string? id { get; set; }
            public string? itemName { get; set; }
            public double? discountRate { get; set; }
        }


        public class UpdateWeeklyInventoryViewModel
        {
            public double? id { get; set; }
            public double? count { get; set; }
            public double? cashierId { get; set; }
            public DateTime createDate { get; set; }
        }

        public class ValidationViewModel
        {
            public double? id { get; set; }
            public string? description { get; set; }
            public string? permission { get; set; }
        }
    }
}
