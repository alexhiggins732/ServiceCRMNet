using Crm.Domain.Entities;
using Xunit;

namespace Crm.Tests.Unit.Domain;

public class PricingTests
{
    [Fact]
    public void EstimateItem_LineTotal_CalculatedCorrectly()
    {
        // Arrange
        var item = new EstimateItem
        {
            Description = "Labor",
            Quantity = 3,
            UnitPrice = 150m
        };

        // Act
        var total = item.LineTotal;

        // Assert
        Assert.Equal(450m, total);
    }

    [Fact]
    public void Invoice_TotalAmount_IsSumOfItems()
    {
        // Arrange
        var invoice = new Invoice
        {
            Title = "Final Bill",
            Items = new List<InvoiceItem>
            {
                new InvoiceItem { Description = "Parts", Quantity = 1, UnitPrice = 500m },
                new InvoiceItem { Description = "Labor", Quantity = 2, UnitPrice = 100m }
            }
        };

        // Act
        invoice.TotalAmount = invoice.Items.Sum(i => i.LineTotal);

        // Assert
        Assert.Equal(700m, invoice.TotalAmount);
    }
}
