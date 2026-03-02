using Crm.Shared.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Modules.Pricing;

public class PricingModule : ICrmModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/pricing").WithTags("Pricing").RequireAuthorization();

        // ProductServices
        group.MapGet("/products", async (CrmDbContext db) => Results.Ok(await db.ProductsServices.ToListAsync()));
        group.MapPost("/products", async (ProductService p, CrmDbContext db) => {
            db.ProductsServices.Add(p);
            await db.SaveChangesAsync();
            return Results.Ok(p);
        });
        group.MapPut("/products/{id:guid}", async (Guid id, ProductService input, CrmDbContext db) => {
            var p = await db.ProductsServices.FindAsync(id);
            if (p == null) return Results.NotFound();
            p.Sku = input.Sku;
            p.Name = input.Name;
            p.Description = input.Description;
            p.UnitPrice = input.UnitPrice;
            p.UnitType = input.UnitType;
            p.Taxable = input.Taxable;
            p.Active = input.Active;
            await db.SaveChangesAsync();
            return Results.Ok(p);
        });

        // Templates
        group.MapGet("/templates", async (CrmDbContext db) => Results.Ok(await db.PricebookTemplates.Include(t => t.Items).ToListAsync()));

        // Estimates
        group.MapGet("/estimates", async (CrmDbContext db) => Results.Ok(await db.Estimates.Include(e => e.Items).ToListAsync()));
        group.MapPost("/estimates", async (Estimate e, CrmDbContext db) => {
            db.Estimates.Add(e);
            await db.SaveChangesAsync();
            return Results.Ok(e);
        });
        group.MapPut("/estimates/{id:guid}", async (Guid id, Estimate input, CrmDbContext db) => {
            var e = await db.Estimates.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
            if (e == null) return Results.NotFound();
            e.Status = input.Status;
            e.CustomerId = input.CustomerId;
            e.Title = input.Title;
            e.TotalAmount = input.TotalAmount;

            db.EstimateItems.RemoveRange(e.Items);
            foreach (var item in input.Items)
            {
                item.Id = Guid.Empty;
                item.EstimateId = e.Id;
                e.Items.Add(item);
            }
            await db.SaveChangesAsync();
            return Results.Ok(e);
        });

        // Invoices
        group.MapGet("/invoices", async (CrmDbContext db) => Results.Ok(await db.Invoices.Include(i => i.Items).ToListAsync()));
        group.MapPost("/invoices", async (Invoice i, CrmDbContext db) => {
            db.Invoices.Add(i);
            await db.SaveChangesAsync();
            return Results.Ok(i);
        });
        group.MapPut("/invoices/{id:guid}", async (Guid id, Invoice input, CrmDbContext db) => {
            var i = await db.Invoices.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
            if (i == null) return Results.NotFound();
            i.Status = input.Status;
            i.CustomerId = input.CustomerId;
            i.EstimateId = input.EstimateId;
            i.Title = input.Title;
            i.TotalAmount = input.TotalAmount;
            i.AmountPaid = input.AmountPaid;

            db.InvoiceItems.RemoveRange(i.Items);
            foreach (var item in input.Items)
            {
                item.Id = Guid.Empty;
                item.InvoiceId = i.Id;
                i.Items.Add(item);
            }
            await db.SaveChangesAsync();
            return Results.Ok(i);
        });

        // Apply template workflows
        group.MapPost("/templates/{id:guid}/apply-to-estimate/{estimateId:guid}", async (Guid id, Guid estimateId, CrmDbContext db) =>
        {
            var estimate = await db.Estimates.Include(e => e.Items).FirstOrDefaultAsync(e => e.Id == estimateId);
            if (estimate == null) return Results.NotFound("Estimate not found");

            var templateItems = await db.TemplateItems
                .Where(ti => ti.PricebookTemplateId == id)
                .ToListAsync();

            if (templateItems.Count == 0) return Results.NotFound("Template not found or has no items");

            foreach (var item in templateItems)
            {
                var prod = await db.ProductsServices.FindAsync(item.ProductServiceId);
                var estItem = new EstimateItem
                {
                    EstimateId = estimateId,
                    Description = prod?.Name ?? "Item",
                    Quantity = item.Quantity,
                    UnitPrice = prod?.UnitPrice ?? 0
                };
                estimate.Items.Add(estItem);
            }

            estimate.TotalAmount = estimate.Items.Sum(i => i.LineTotal);

            await db.SaveChangesAsync();
            return Results.Ok(estimate);
        });

        group.MapPost("/templates/{id:guid}/apply-to-invoice/{invoiceId:guid}", async (Guid id, Guid invoiceId, CrmDbContext db) =>
        {
            var invoice = await db.Invoices.Include(i => i.Items).FirstOrDefaultAsync(i => i.Id == invoiceId);
            if (invoice == null) return Results.NotFound("Invoice not found");

            var templateItems = await db.TemplateItems
                .Where(ti => ti.PricebookTemplateId == id)
                .ToListAsync();

            if (templateItems.Count == 0) return Results.NotFound("Template not found or has no items");

            foreach (var item in templateItems)
            {
                var prod = await db.ProductsServices.FindAsync(item.ProductServiceId);
                var invItem = new InvoiceItem
                {
                    InvoiceId = invoiceId,
                    Description = prod?.Name ?? "Item",
                    Quantity = item.Quantity,
                    UnitPrice = prod?.UnitPrice ?? 0
                };
                invoice.Items.Add(invItem);
            }

            invoice.TotalAmount = invoice.Items.Sum(i => i.LineTotal);

            await db.SaveChangesAsync();
            return Results.Ok(invoice);
        });
    }
}
