using Crm.Shared.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Crm.Application.Common.Interfaces;

namespace Crm.Modules.CoreCrm;

public class CoreCrmModule : ICrmModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAutomationEngine, AutomationEngine>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/core").WithTags("Core CRM").RequireAuthorization();

        // Customers
        group.MapGet("/customers", async (CrmDbContext db) => Results.Ok(await db.Customers.ToListAsync()));
        group.MapGet("/customers/{id:guid}", async (Guid id, CrmDbContext db) =>
            await db.Customers.FindAsync(id) is Customer customer ? Results.Ok(customer) : Results.NotFound());
        group.MapPost("/customers", async (Customer customer, CrmDbContext db) => {
            db.Customers.Add(customer);
            await db.SaveChangesAsync();
            return Results.Created($"/api/core/customers/{customer.Id}", customer);
        });
        group.MapPut("/customers/{id:guid}", async (Guid id, Customer input, CrmDbContext db) => {
            var customer = await db.Customers.FindAsync(id);
            if (customer == null) return Results.NotFound();
            customer.Name = input.Name;
            customer.Email = input.Email;
            customer.Phone = input.Phone;
            customer.Address = input.Address;
            await db.SaveChangesAsync();
            return Results.Ok(customer);
        });

        // Leads
        group.MapGet("/leads", async (CrmDbContext db) => Results.Ok(await db.Leads.ToListAsync()));
        group.MapPost("/leads", async (Lead lead, CrmDbContext db, IAutomationEngine ae, ITenantService ts) => {
            db.Leads.Add(lead);
            await db.SaveChangesAsync();

            await ae.EvaluateAsync(new AutomationEvent {
                EventType = "lead_created",
                TenantId = ts.GetCurrentTenantId()!.Value,
                PayloadJson = "{}"
            });

            return Results.Created($"/api/core/leads/{lead.Id}", lead);
        });
        group.MapPut("/leads/{id:guid}", async (Guid id, Lead input, CrmDbContext db) => {
            var lead = await db.Leads.FindAsync(id);
            if (lead == null) return Results.NotFound();
            lead.Status = input.Status;
            lead.Source = input.Source;
            lead.CustomerId = input.CustomerId;
            await db.SaveChangesAsync();
            return Results.Ok(lead);
        });

        // Jobs
        group.MapGet("/jobs", async (CrmDbContext db) => Results.Ok(await db.Jobs.ToListAsync()));
        group.MapPost("/jobs", async (Job job, CrmDbContext db) => {
            db.Jobs.Add(job);
            await db.SaveChangesAsync();
            return Results.Created($"/api/core/jobs/{job.Id}", job);
        });
        group.MapPut("/jobs/{id:guid}", async (Guid id, Job input, CrmDbContext db) => {
            var job = await db.Jobs.FindAsync(id);
            if (job == null) return Results.NotFound();
            job.Status = input.Status;
            job.LeadId = input.LeadId;
            job.AssignedUserId = input.AssignedUserId;
            job.ScheduledStart = input.ScheduledStart;
            job.ScheduledEnd = input.ScheduledEnd;
            job.Notes = input.Notes;
            await db.SaveChangesAsync();
            return Results.Ok(job);
        });

        // Messages
        group.MapGet("/messages", async (CrmDbContext db) => Results.Ok(await db.Messages.OrderByDescending(m => m.CreatedAt).ToListAsync()));

        // Appointments
        group.MapGet("/appointments", async (CrmDbContext db) => Results.Ok(await db.Appointments.ToListAsync()));
        group.MapPost("/appointments", async (Appointment appt, CrmDbContext db) => {
            db.Appointments.Add(appt);
            await db.SaveChangesAsync();
            return Results.Created($"/api/core/appointments/{appt.Id}", appt);
        });
        group.MapPut("/appointments/{id:guid}", async (Guid id, Appointment input, CrmDbContext db) => {
            var appt = await db.Appointments.FindAsync(id);
            if (appt == null) return Results.NotFound();
            appt.JobId = input.JobId;
            appt.Start = input.Start;
            appt.End = input.End;
            appt.Location = input.Location;
            appt.Notes = input.Notes;
            await db.SaveChangesAsync();
            return Results.Ok(appt);
        });

        // Automations
        group.MapGet("/automations", async (CrmDbContext db) => Results.Ok(await db.Automations.ToListAsync()));
        group.MapPost("/automations", async (Automation auto, CrmDbContext db) => {
            db.Automations.Add(auto);
            await db.SaveChangesAsync();
            return Results.Created($"/api/core/automations/{auto.Id}", auto);
        });
        group.MapPut("/automations/{id:guid}", async (Guid id, Automation input, CrmDbContext db) => {
            var auto = await db.Automations.FindAsync(id);
            if (auto == null) return Results.NotFound();
            auto.Name = input.Name;
            auto.IsEnabled = input.IsEnabled;
            auto.TriggerType = input.TriggerType;
            auto.TriggerConfigJson = input.TriggerConfigJson;
            auto.ActionType = input.ActionType;
            auto.ActionConfigJson = input.ActionConfigJson;
            await db.SaveChangesAsync();
            return Results.Ok(auto);
        });
        group.MapDelete("/automations/{id:guid}", async (Guid id, CrmDbContext db) => {
            var auto = await db.Automations.FindAsync(id);
            if (auto != null)
            {
                db.Automations.Remove(auto);
                await db.SaveChangesAsync();
            }
            return Results.NoContent();
        });
    }
}
