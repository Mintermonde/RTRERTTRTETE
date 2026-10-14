using UavFleetManagement.Domain.Entities.Fleet;

namespace UavFleetManagement.Domain.Entities.Insurance;

public class BillingInvoice
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string ServiceType { get; set; } = "InsurancePolicy";
    public int PayerOwnerId { get; set; }
    public decimal AmountRub { get; set; }
    public string PaymentStatus { get; set; } = "Pending";
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime? PaidAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual UavOwner? PayerOwner { get; set; }
}
