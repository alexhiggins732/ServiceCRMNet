using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Crm.Domain.Entities;

namespace Crm.Web.Services;

public class CrmApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public CrmApiClient(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    private async Task PrepareRequestAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/auth/login", new { Email = email, Password = password });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResult>();
            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                await _localStorage.SetItemAsync("authToken", result.Token);
                return result.Token;
            }
        }
        return null;
    }

    public async Task<List<Customer>> GetCustomersAsync()
    {
        await PrepareRequestAsync();
        return await _httpClient.GetFromJsonAsync<List<Customer>>("/api/core/customers") ?? new List<Customer>();
    }

    public async Task<Customer> SaveCustomerAsync(Customer customer)
    {
        await PrepareRequestAsync();
        if (customer.Id == Guid.Empty)
        {
            var res = await _httpClient.PostAsJsonAsync("/api/core/customers", customer);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Customer>() ?? customer;
        }
        else
        {
            var res = await _httpClient.PutAsJsonAsync($"/api/core/customers/{customer.Id}", customer);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Customer>() ?? customer;
        }
    }

    public async Task<List<Lead>> GetLeadsAsync()
    {
        await PrepareRequestAsync();
        return await _httpClient.GetFromJsonAsync<List<Lead>>("/api/core/leads") ?? new List<Lead>();
    }

    public async Task<List<Job>> GetJobsAsync()
    {
        await PrepareRequestAsync();
        return await _httpClient.GetFromJsonAsync<List<Job>>("/api/core/jobs") ?? new List<Job>();
    }

    public async Task<List<Message>> GetMessagesAsync()
    {
        await PrepareRequestAsync();
        return await _httpClient.GetFromJsonAsync<List<Message>>("/api/core/messages") ?? new List<Message>();
    }

    public async Task<List<Appointment>> GetAppointmentsAsync()
    {
        await PrepareRequestAsync();
        return await _httpClient.GetFromJsonAsync<List<Appointment>>("/api/core/appointments") ?? new List<Appointment>();
    }

    public async Task<List<ProductService>> GetProductsAsync()
    {
        await PrepareRequestAsync();
        return await _httpClient.GetFromJsonAsync<List<ProductService>>("/api/pricing/products") ?? new List<ProductService>();
    }

    public async Task<List<PricebookTemplate>> GetTemplatesAsync()
    {
        await PrepareRequestAsync();
        return await _httpClient.GetFromJsonAsync<List<PricebookTemplate>>("/api/pricing/templates") ?? new List<PricebookTemplate>();
    }

    public async Task<List<Estimate>> GetEstimatesAsync()
    {
        await PrepareRequestAsync();
        return await _httpClient.GetFromJsonAsync<List<Estimate>>("/api/pricing/estimates") ?? new List<Estimate>();
    }

    public async Task<List<Invoice>> GetInvoicesAsync()
    {
        await PrepareRequestAsync();
        return await _httpClient.GetFromJsonAsync<List<Invoice>>("/api/pricing/invoices") ?? new List<Invoice>();
    }

    public async Task<Estimate?> ApplyTemplateToEstimateAsync(Guid estimateId, Guid templateId)
    {
        await PrepareRequestAsync();
        var response = await _httpClient.PostAsync($"/api/pricing/templates/{templateId}/apply-to-estimate/{estimateId}", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Estimate>();
    }

    public async Task<List<Automation>> GetAutomationsAsync()
    {
        await PrepareRequestAsync();
        return await _httpClient.GetFromJsonAsync<List<Automation>>("/api/core/automations") ?? new List<Automation>();
    }

    public async Task<string> SendAiMessageAsync(string provider, string message)
    {
        await PrepareRequestAsync();
        var response = await _httpClient.PostAsJsonAsync("/api/ai/chat", new { Provider = provider, Model = "default", Context = "CRM context", Messages = message });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("response").GetString() ?? "No response";
    }
}

public class LoginResult
{
    public string Token { get; set; } = string.Empty;
}
