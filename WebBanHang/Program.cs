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
using WebBanHang.UseCases.PublicPortal;
using WebBanHang.UseCases.GlobalSearch;
using WebBanHang.UseCases.Notifications;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// State Service
builder.Services.AddScoped<DraftInvoiceStateService>();
builder.Services.AddScoped<LayoutStateService>();

// Repositories
builder.Services.AddTransient<ICustomerRepository, CustomerRepository>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddTransient<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddTransient<IPaymentRepository, PaymentRepository>();
builder.Services.AddTransient<IPublicStatisticsRepository, PublicStatisticsRepository>();
builder.Services.AddTransient<IGlobalSearchRepository, GlobalSearchRepository>();
builder.Services.AddTransient<INotificationRepository, NotificationRepository>();

// UseCases & Smart Assistant
builder.Services.AddTransient<IAvaSmartAssistantService, AvaSmartAssistantService>();
builder.Services.AddTransient<IGetSmartRemindersUseCase, GetSmartRemindersUseCase>();
builder.Services.AddTransient<IAnswerAccountingQuestionUseCase, AnswerAccountingQuestionUseCase>();
builder.Services.AddTransient<IGlobalSearchUseCase, GlobalSearchUseCase>();
builder.Services.AddTransient<IGetNotificationsUseCase, GetNotificationsUseCase>();
builder.Services.AddTransient<IGetPublicStatisticsUseCase, GetPublicStatisticsUseCase>();
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
builder.Services.AddTransient<IGetCustomerPortalInfoUseCase, GetCustomerPortalInfoUseCase>();
builder.Services.AddTransient<IGetCustomerMyInvoicesUseCase, GetCustomerMyInvoicesUseCase>();
builder.Services.AddTransient<ICreateCustomerPortalOrderUseCase, CreateCustomerPortalOrderUseCase>();

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

// Anti-caching middleware for authenticated pages & protected areas
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var path = context.Request.Path.Value ?? "";
        bool isProtectedPath = path.StartsWith("/dashboard", StringComparison.OrdinalIgnoreCase) ||
                               path.StartsWith("/portal", StringComparison.OrdinalIgnoreCase) ||
                               path.StartsWith("/invoices", StringComparison.OrdinalIgnoreCase) ||
                               path.StartsWith("/payments", StringComparison.OrdinalIgnoreCase) ||
                               path.StartsWith("/aging-report", StringComparison.OrdinalIgnoreCase) ||
                               path.StartsWith("/customer-ledger", StringComparison.OrdinalIgnoreCase) ||
                               path.StartsWith("/manage-customers", StringComparison.OrdinalIgnoreCase) ||
                               path.StartsWith("/manage-products", StringComparison.OrdinalIgnoreCase) ||
                               path.StartsWith("/create-invoice", StringComparison.OrdinalIgnoreCase) ||
                               path.StartsWith("/payment-history", StringComparison.OrdinalIgnoreCase);

        if (context.User.Identity?.IsAuthenticated == true || isProtectedPath)
        {
            context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";
        }
        return Task.CompletedTask;
    });

    await next();
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/api/auth/login", async (HttpContext httpContext, IFormCollection form) => {
    string username = form["username"].ToString().Trim().ToLowerInvariant();
    string password = form["password"].ToString().Trim();
    string returnUrl = form["returnUrl"].ToString().Trim();

    if ((username == "sales" && password == "123456") || 
        (username == "accountant" && password == "123456") ||
        ((username == "customer" || username == "baba") && password == "123456"))
    {
        string role = username == "accountant" ? "Accountant" : (username == "customer" || username == "baba") ? "Customer" : "Sales";
        string fullName = username == "accountant" ? "Kế Toán Nguyễn Văn A" : (username == "customer" || username == "baba") ? "Tạp Hóa Bà Ba - Chợ Đông Ba" : "Nhân Viên Trần Văn B";
        string nameId = (username == "customer" || username == "baba") ? "customer" : username;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.NameIdentifier, nameId),
            new(ClaimTypes.Role, role)
        };

        if (role == "Customer")
        {
            claims.Add(new Claim("CustomerId", "1"));
        }

        var identity = new ClaimsIdentity(claims, "Cookies");
        await httpContext.SignInAsync("Cookies", new ClaimsPrincipal(identity));

        // Return to requested URL if present and valid
        if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && !returnUrl.Contains("/login"))
        {
            return Results.Redirect(returnUrl);
        }

        if (role == "Customer")
        {
            return Results.Redirect("/portal/dashboard");
        }
        else if (role == "Accountant")
        {
            return Results.Redirect("/dashboard");
        }
        else
        {
            return Results.Redirect("/create-invoice");
        }
    }
    string errRedirect = "/login?error=true" + (!string.IsNullOrEmpty(returnUrl) ? $"&returnUrl={Uri.EscapeDataString(returnUrl)}" : "");
    return Results.Redirect(errRedirect);
}).DisableAntiforgery();

app.MapGet("/api/auth/switch", async (string role, HttpContext httpContext) => {
    string r = (role ?? "").Trim().ToLower();
    string targetRole = r == "customer" ? "Customer" : (r == "sales" ? "Sales" : "Accountant");
    string fullName = targetRole == "Accountant" ? "Kế Toán Nguyễn Văn A" : (targetRole == "Customer" ? "Tạp Hóa Bà Ba - Chợ Đông Ba" : "Nhân Viên Trần Văn B");
    string nameId = targetRole == "Customer" ? "customer" : (targetRole == "Accountant" ? "accountant" : "sales");

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, fullName),
        new(ClaimTypes.NameIdentifier, nameId),
        new(ClaimTypes.Role, targetRole)
    };

    if (targetRole == "Customer")
    {
        claims.Add(new Claim("CustomerId", "1"));
    }

    var identity = new ClaimsIdentity(claims, "Cookies");
    await httpContext.SignInAsync("Cookies", new ClaimsPrincipal(identity));

    if (targetRole == "Customer")
    {
        return Results.Redirect("/portal/dashboard");
    }
    return Results.Redirect("/dashboard");
});

app.MapGet("/api/auth/logout", async (HttpContext httpContext) => {
    await httpContext.SignOutAsync("Cookies");
    httpContext.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
    httpContext.Response.Headers["Pragma"] = "no-cache";
    httpContext.Response.Headers["Expires"] = "0";
    return Results.Redirect("/login");
});

app.Run();
