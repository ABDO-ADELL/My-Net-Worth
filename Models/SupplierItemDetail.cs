using PRISM;
using System.ComponentModel.DataAnnotations.Schema;

public class SupplierItem
{
    [Key]
    public int SupplierItemId { get; set; }

    [ForeignKey("Supplier")]
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; }

    [ForeignKey("Items")]
    public int ItemId { get; set; }
    public Items Item { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PurchasePrice { get; set; }

    [Required, MaxLength(50)]
    public string PaymentMethod { get; set; } 
}
