using WebBanHang.Components;
using WebBanHang.UseCases.PluginInterfaces.DataStore;
using WebBanHang.Plugins.DataStore.SQL;
using WebBanHang.UseCases.Invoices;
using WebBanHang.UseCases.Customers;
using WebBanHang.UseCases.Products;
using WebBanHang.Services;
using WebBanHang.UseCases.Payments;
using WebBanHang.UseCases.Reports;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using WebBanHang.UseCases.AI;
using WebBanHang.UseCases.Dashboard;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// State Service
builder.Services.AddScoped<DraftInvoiceStateService>();

// Repositories
builder.Services.AddTransient<ICustomerRepository, CustomerRepository>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddTransient<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddTransient<IPaymentRepository, PaymentRepository>();

// UseCases
builder.Services.AddTransient<ICreateInvoiceUseCase, CreateInvoiceUseCase>();
builder.Services.AddTransient<IViewCustomerListUseCase, ViewCustomerListUseCase>();
builder.Services.AddTransient<IViewProductCatalogUseCase, ViewProductCatalogUseCase>();
builder.Services.AddTransient<IManageCustomerUseCase, ManageCustomerUseCase>();
builder.Services.AddTransient<IRecordPaymentAndAllocateUseCase, RecordPaymentAndAllocateUseCase>();
builder.Services.AddTransient<IGetCustomerAgingReportUseCase, GetCustomerAgingReportUseCase>();
builder.Services.AddTransient<IAiDebtReminderService, MockAiDebtReminderService>();
builder.Services.AddTransient<IDraftDebtReminderEmailUseCase, DraftDebtReminderEmailUseCase>();
builder.Services.AddTransient<IGetDashboardDataUseCase, GetDashboardDataUseCase>();
builder.Services.AddTransient<IViewInvoiceListUseCase, ViewInvoiceListUseCase>();
builder.Services.AddTransient<IViewInvoiceDetailUseCase, ViewInvoiceDetailUseCase>();
builder.Services.AddTransient<IGetUnpaidInvoicesUseCase, GetUnpaidInvoicesUseCase>();
builder.Services.AddTransient<IManageProductUseCase, ManageProductUseCase>();
builder.Services.AddTransient<IViewPaymentHistoryUseCase, ViewPaymentHistoryUseCase>();
builder.Services.AddTransient<IViewCustomerLedgerUseCase, ViewCustomerLedgerUseCase>();
builder.Services.AddTransient<IAiCreditRiskService, MockAiCreditRiskService>();
builder.Services.AddTransient<IAssessCreditRiskUseCase, AssessCreditRiskUseCase>();
builder.Services.AddTransient<IGetCustomerDebtUseCase, GetCustomerDebtUseCase>();
builder.Services.AddTransient<IEmailSenderService, EmailSenderService>();

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.Cookie.Name = "WebBanHang.AuthCookie";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/api/auth/login", async (HttpContext httpContext, IFormCollection form) => {
    string username = form["username"]!;
    string password = form["password"]!;
    if ((username == "sales" && password == "123456") || (username == "accountant" && password == "123456"))
    {
        string role = username == "accountant" ? "Accountant" : "Sales";
        string fullName = username == "accountant" ? "Kế Toán Nguyễn Văn A" : "Nhân Viên Trần Văn B";
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.NameIdentifier, username),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "Cookies");
        await httpContext.SignInAsync("Cookies", new ClaimsPrincipal(identity));
        return Results.Redirect(role == "Accountant" ? "/aging-report" : "/create-invoice");
    }
    return Results.Redirect("/login?error=true");
}).DisableAntiforgery();

app.MapGet("/api/auth/logout", async (HttpContext httpContext) => {
    await httpContext.SignOutAsync("Cookies");
    return Results.Redirect("/login");
});

app.Run();
