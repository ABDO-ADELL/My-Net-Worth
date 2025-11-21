using System.ComponentModel.DataAnnotations.Schema;

public class Supplier
{
    [Key]
    public int SupplierId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(100)]
    public string Email { get; set; }

    [MaxLength(20)]
    public string Phone { get; set; }

    public bool IsDeleted { get; set; } = false;

    public List<SupplierItem> SupplierItems { get; set; } = new List<SupplierItem>();
    [Required]
    public int BusinessId { get; set; }

    [ForeignKey("BusinessId")]
    public Business? Business { get; set; }

}
