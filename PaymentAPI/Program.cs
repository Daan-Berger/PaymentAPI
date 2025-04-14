using PaymentAPI.Models;
using PaymentAPI.Services;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add environment variable configuration source
builder.Configuration.AddEnvironmentVariables();

// Override settings with environment variables
var stripeSettings = builder.Configuration.GetSection("Stripe").Get<StripeSettings>() ?? new StripeSettings();
stripeSettings.SecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY") ?? stripeSettings.SecretKey;

var mollieSettings = builder.Configuration.GetSection("Mollie").Get<MollieSettings>() ?? new MollieSettings();
mollieSettings.ApiKey = Environment.GetEnvironmentVariable("MOLLIE_API_KEY") ?? mollieSettings.ApiKey;

var adyenSettings = builder.Configuration.GetSection("Adyen").Get<AdyenSettings>() ?? new AdyenSettings();
adyenSettings.ApiKey = Environment.GetEnvironmentVariable("ADYEN_API_KEY") ?? adyenSettings.ApiKey;
adyenSettings.MerchantAccount = Environment.GetEnvironmentVariable("ADYEN_MERCHANT_ACCOUNT") ?? adyenSettings.MerchantAccount;

var appSettings = builder.Configuration.GetSection("App").Get<AppSettings>() ?? new AppSettings();
appSettings.FrontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? appSettings.FrontendUrl;

// Register settings with DI container
builder.Services.Configure<StripeSettings>(options => {
    options.SecretKey = stripeSettings.SecretKey;
});

builder.Services.Configure<MollieSettings>(options => {
    options.ApiKey = mollieSettings.ApiKey;
});

builder.Services.Configure<AdyenSettings>(options => {
    options.ApiKey = adyenSettings.ApiKey;
    options.MerchantAccount = adyenSettings.MerchantAccount;
    options.HmacSecret = adyenSettings.HmacSecret;
});

builder.Services.Configure<AppSettings>(options => {
    options.FrontendUrl = appSettings.FrontendUrl;
});

// Add existing services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<MolliePaymentService>();
builder.Services.AddScoped<StripePaymentService>();
builder.Services.AddScoped<AdyenPaymentService>();
builder.Services.AddScoped<PaymentServiceFactory>();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", builder =>
    {
        builder.WithOrigins("http://localhost:8082")  // Update port to 8082
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowVueApp");
app.UseAuthorization();

app.MapControllers();

app.Run();