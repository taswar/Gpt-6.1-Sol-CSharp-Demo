#pragma warning disable OPENAI001 // Responses API types are marked experimental in the OpenAI SDK

using System.ClientModel.Primitives;
using System.ComponentModel;
using System.Diagnostics;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Responses;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var endpoint = config["AZURE_AI_ENDPOINT"]
    ?? throw new InvalidOperationException(
        "AZURE_AI_ENDPOINT is not set. Run: dotnet user-secrets set \"AZURE_AI_ENDPOINT\" \"<your-endpoint>\"");
var deployment = config["AZURE_OPENAI_DEPLOYMENT"]
    ?? throw new InvalidOperationException(
        "AZURE_OPENAI_DEPLOYMENT is not set. Run: dotnet user-secrets set \"AZURE_OPENAI_DEPLOYMENT\" \"<your-deployment>\"");

// gpt-6-sol rejects function tools + reasoning on /chat/completions, so use the
// Responses API through the Azure OpenAI v1 endpoint.
var clientOptions = new ResponsesClientOptions
{
    Endpoint = new Uri(new Uri(endpoint), "openai/v1/"),
    NetworkTimeout = TimeSpan.FromMinutes(3)
};

IChatClient chat = new ResponsesClient(
        new BearerTokenPolicy(new DefaultAzureCredential(),
            "https://cognitiveservices.azure.com/.default"),
        clientOptions)
    .AsIChatClient(deployment)
    .AsBuilder()
    .UseFunctionInvocation()
    .UseOpenTelemetry(sourceName: "Gpt-6.1-Sol-CSharp-Demo")
    .Build();

using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

static void Report(string label, ChatResponse response, Stopwatch timer) =>
    Console.WriteLine(
        $"[{label}] input={response.Usage?.InputTokenCount} " +
        $"cached={response.Usage?.CachedInputTokenCount} " +
        $"output={response.Usage?.OutputTokenCount} " +
        $"reasoning={response.Usage?.ReasoningTokenCount} " +
        $"elapsed={timer.Elapsed.TotalSeconds:F1}s");

var ci = args.Length > 0
    ? File.ReadAllText(args[0])
    : "PR #418: change to PromoCodeResolver; CI failed: " +
      "DiscountTests.SinglePromo_WhenExpired expected rejection, got success. " +
      "Other 203 tests passed.";

var reviewTimer = Stopwatch.StartNew();
var review = await chat.GetResponseAsync(
    [
        new(ChatRole.System, "You are a C# PR reviewer. Use only the supplied CI " +
            "evidence. Separate observed facts from hypotheses. Suggest one " +
            "reproducible next test. Do not approve or merge the PR."),
        new(ChatRole.User, ci)
    ], cancellationToken: cts.Token);

Console.WriteLine("###################### Case 1 ################################");
Console.WriteLine(review.Text);
Report("pr-triage", review, reviewTimer);
Console.WriteLine("##############################################################");

var options = new ChatOptions
{
    Tools = [AIFunctionFactory.Create(LookUpAccount)]
};

var changeTimer = Stopwatch.StartNew();
var result = await chat.GetResponseAsync<ProposedChange>(
    [
        new(ChatRole.System, "You assist a support agent. Look up the account, " +
            "then return a proposed billing-email change for human review. " +
            "You have no tool that can update the account."),
        new(ChatRole.User, "Account ACC-4471 requests changing its billing email " +
            "to finance@northwind-retail.com.")
    ], options, cancellationToken: cts.Token);

if (result.TryGetResult(out var proposal))
{
    // Hand the proposal to your approval workflow; nothing is written here.
    Console.WriteLine("###################### Case 2 ################################");
    Console.WriteLine($"{proposal.AccountId} {proposal.Field}: " +
        $"{proposal.CurrentValue} -> {proposal.ProposedValue} (pending approval)");
}
Report("account-change", result, changeTimer);
Console.WriteLine("##############################################################");

[Description("Read-only lookup of an account by ID.")]
static string LookUpAccount([Description("Account ID")] string accountId) =>
    accountId == "ACC-4471"
        ? "Northwind Retail; billing email: billing-old@northwind-retail.com"
        : "Account not found";


var documents = """
    [SLA §4] Credits require a claim within 30 days of the incident.
    [Incident INC-204] Availability dropped below the SLA target on 2026-09-16.
    [Incident INC-204] The incident note does not record whether a claim was filed.
    """;

var vendorTimer = Stopwatch.StartNew();
var summary = await chat.GetResponseAsync(
    [
        new(ChatRole.System, "Review only the supplied sources. For each finding, " +
            "quote its source label. Distinguish a missing fact from a negative fact. " +
            "Suggest a human follow-up; do not make a legal determination."),
        new(ChatRole.User, documents + "\nWhat should the weekly vendor review flag?")
    ], cancellationToken: cts.Token);

Console.WriteLine("###################### Case 3 ################################");
Console.WriteLine(summary.Text);
Report("vendor-review", summary, vendorTimer);
Console.WriteLine("##############################################################");

Console.WriteLine("###################### Reasoning Effort ################################");
foreach (var effort in new[] { ReasoningEffort.Low, ReasoningEffort.Medium, ReasoningEffort.High })
{
    var effortTimer = Stopwatch.StartNew();
    var attempt = await chat.GetResponseAsync(
        [new(ChatRole.User, ci)],
        new ChatOptions { Reasoning = new() { Effort = effort } },
        cts.Token);
    Report($"pr-triage-{effort}", attempt, effortTimer);
}
Console.WriteLine("##############################################################");

record ProposedChange(
    string AccountId,
    string Field,
    string CurrentValue,
    string ProposedValue,
    bool RequiresRequesterVerification);

