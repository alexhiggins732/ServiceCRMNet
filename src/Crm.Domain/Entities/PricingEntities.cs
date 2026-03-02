using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class ProductService : TenantEntity
{
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public required string UnitType { get; set; }
    public bool Taxable { get; set; }
    public bool Active { get; set; } = true;
}

public class PricebookTemplate : TenantEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<TemplateItem> Items { get; init; } = new List<TemplateItem>();
}

public class TemplateItem : TenantEntity
{
    public Guid PricebookTemplateId { get; set; }
    public Guid ProductServiceId { get; set; }
    public int Quantity { get; set; }
}

public class Estimate : TenantEntity
{
    public Guid CustomerId { get; set; }
    public required string Title { get; set; }
    public string Status { get; set; } = "Draft";
    public ICollection<EstimateItem> Items { get; init; } = new List<EstimateItem>();
    public decimal TotalAmount { get; set; }
}

public class EstimateItem : TenantEntity
{
    public Guid EstimateId { get; set; }
    public required string Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public class Invoice : TenantEntity
{
    public Guid CustomerId { get; set; }
    public Guid? EstimateId { get; set; }
    public required string Title { get; set; }
    public string Status { get; set; } = "Unpaid";
    public ICollection<InvoiceItem> Items { get; init; } = new List<InvoiceItem>();
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
}

public class InvoiceItem : TenantEntity
{
    public Guid InvoiceId { get; set; }
    public required string Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public class Payment : TenantEntity
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public required string Method { get; set; }
    public required string Status { get; set; }
    public string? ExternalId { get; set; }
}
