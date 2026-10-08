// ---------------------------------------
// Email: quickapp@ebenmonney.com
// Templates: www.ebenmonney.com/templates
// (c) 2024 www.ebenmonney.com/mit-license
// ---------------------------------------

using QuickApp.Server.Attributes;
using System.ComponentModel.DataAnnotations;

namespace QuickApp.Server.ViewModels.Shop
{
    public class OrderVM
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CashierId { get; set; }
        public decimal Discount { get; set; }
        public string? Comments { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public ICollection<OrderItemVM> Items { get; set; } = [];
    }

    public class OrderItemVM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
    }

    public class CreateOrderVM
    {
        [Range(1, int.MaxValue)]
        public int CustomerId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Discount { get; set; }

        [StringLength(500)]
        public string? Comments { get; set; }

        [MinimumCount(1)]
        public List<CreateOrderItemVM> Items { get; set; } = [];
    }

    public class CreateOrderItemVM
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Discount { get; set; }
    }

    public class UpdateOrderVM
    {
        [Range(0, double.MaxValue)]
        public decimal Discount { get; set; }

        [StringLength(500)]
        public string? Comments { get; set; }
    }
}
