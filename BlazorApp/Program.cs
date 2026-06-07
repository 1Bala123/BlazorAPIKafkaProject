using BlazorApp.Components;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError() // 5xx, 408, network failures
    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10); 

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,   // fail 5 times
        durationOfBreak: TimeSpan.FromSeconds(30) // then stop for 30s
    );


builder.Services.AddHttpClient<TokenService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5240/");
}).AddPolicyHandler(retryPolicy)
  .AddPolicyHandler(timeoutPolicy)
  .AddPolicyHandler(circuitBreakerPolicy);

builder.Services.AddTransient<AuthMessageHandler>();
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("http://localhost:5240/");
}).AddHttpMessageHandler<AuthMessageHandler>()
  .AddPolicyHandler(retryPolicy)
  .AddPolicyHandler(timeoutPolicy)
  .AddPolicyHandler(circuitBreakerPolicy);


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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
