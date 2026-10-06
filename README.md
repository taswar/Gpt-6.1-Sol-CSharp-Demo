# GPT-6.1 Sol for C# Developers

> A runnable .NET 10 console app that calls **GPT-6.1 Sol** in **Microsoft Foundry** through `Microsoft.Extensions.AI`, covering PR triage, an approval-gated tool call, an evidence-bound vendor review and a reasoning-effort cost comparison.

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)
![License: MIT](https://img.shields.io/badge/license-MIT-green)

<!-- TODO: add a screenshot of a full `dotnet run`. -->

It's one `Program.cs` with no wrapper library. Each sample prints its token usage, so you can see what the work costs before you put it into a pipeline.

## What's inside

| # | Sample | What it shows |
|---|--------|---------------|
| 1 | **PR triage** | Reviews a CI failure, separates observed facts from hypotheses and suggests one reproducible next test. It never approves or merges. |
| 2 | **Approval-gated change** | Calls a read-only `LookUpAccount` tool, then returns a typed `ProposedChange` record for a human to approve. Nothing is written. |
| 3 | **Vendor review** | Reviews SLA and incident notes, cites the source label for every finding, and tells a *missing* fact apart from a *negative* one. |
| 4 | **Reasoning effort** | Runs the Case 1 prompt at `Low`, `Medium` and `High` effort and compares token usage and latency. |

Each sample prints a usage line like this one:

```
[pr-triage] input=77 cached=0 output=342 reasoning=222 elapsed=6.1s
```

## Prerequisites

- **.NET 10 SDK**
- An Azure subscription with a **Microsoft Foundry** resource and a **GPT-6.1 Sol** deployment
- The **Azure CLI**, signed in with `az login`
- The **Cognitive Services OpenAI User** role on the resource. Authentication goes through Entra ID (`DefaultAzureCredential`), so there's no API key.

## Run it

**1. Store your settings** (user secrets are kept outside the repo):

```bash
dotnet user-secrets set "AZURE_AI_ENDPOINT" "https://<your-resource>.cognitiveservices.azure.com/"
dotnet user-secrets set "AZURE_OPENAI_DEPLOYMENT" "<your-deployment-name>"
```

Use the resource's base endpoint. The app appends `openai/v1/` itself.

**2. Sign in and run:**

```bash
az login
dotnet run
```

**3. Optional: triage your own CI output.** Pass a file and it replaces the built-in CI summary in Cases 1 and 4:

```bash
dotnet run -- ./ci-failure.txt
```

## Why the Responses API?

The app uses the Responses API (`/openai/v1/responses`) instead of Chat Completions. GPT-6.1 Sol rejects function tools combined with reasoning on `/chat/completions`:

```
HTTP 400 ... Function tools with reasoning_effort are not supported for gpt-6-sol
in /v1/chat/completions. To use function tools, use /v1/responses or set
reasoning_effort to 'none'.
```

Case 2 needs both, so the client is built like this:

```csharp
IChatClient chat = new ResponsesClient(
        new BearerTokenPolicy(new DefaultAzureCredential(),
            "https://cognitiveservices.azure.com/.default"),
        new ResponsesClientOptions { Endpoint = new Uri(new Uri(endpoint), "openai/v1/") })
    .AsIChatClient(deployment)
    .AsBuilder()
    .UseFunctionInvocation()
    .UseOpenTelemetry(sourceName: "Gpt-6.1-Sol-CSharp-Demo")
    .Build();
```

The OpenAI SDK marks the Responses types as experimental, which is why `Program.cs` starts with `#pragma warning disable OPENAI001`.

## Good to know

- **Live calls cost money.** One run makes at least six model requests (Case 2 makes extra calls while it uses the tool), and Case 4 deliberately runs at `High` effort.
- **The samples are simulations.** The account lookup, the CI result and the vendor documents are all hard-coded in `Program.cs`. Nothing touches a real system.
- **Output varies.** Responses are model-generated and differ between runs. Compare token counts across several runs, not just one.
- **Every call shares one 5-minute timeout,** and each HTTP request has a 3-minute network timeout.

## Troubleshooting

| Error | Likely cause | Fix |
|-------|--------------|-----|
| `404 DeploymentNotFound` | `AZURE_OPENAI_DEPLOYMENT` doesn't match a deployment on the resource | Set it to the exact deployment name. A brand-new deployment can take about 5 minutes to become available. |
| `400 ... Function tools with reasoning_effort are not supported` | The client is calling Chat Completions | Use the `ResponsesClient` setup shown above |
| `401` / `403` | Missing role, or `az login` signed in to the wrong tenant | Assign **Cognitive Services OpenAI User**, then run `az login --tenant <id>` |
| `InvalidOperationException: AZURE_AI_ENDPOINT is not set` | User secrets are missing | Run the `dotnet user-secrets set` commands from inside the project folder |

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Extensions.AI` | 10.10.0 | `IChatClient`, function invocation, structured output, OpenTelemetry |
| `Microsoft.Extensions.AI.OpenAI` | 10.10.1 | `IChatClient` adapter for the OpenAI `ResponsesClient` (brings in `OpenAI` 2.14.0) |
| `Azure.Identity` | 1.21.0 | `DefaultAzureCredential` for Entra ID sign-in |
| `Microsoft.Extensions.Configuration.UserSecrets` | 10.0.12 | Local development secrets |
| `Azure.AI.OpenAI` | 2.1.0 | No longer used by `Program.cs`. Safe to remove. |

## Project layout

```
Gpt-6.1-Sol-CSharp-Demo.csproj   Target framework, NuGet references, UserSecretsId
Program.cs                       Client setup, the four samples, the usage reporter
```

## Learn more

<!-- TODO: confirm the final URL once the post is published. -->
- [GPT-6.1 Sol for C# Developers (blog post)](https://taswar.zeytinsoft.com/gpt-6-1-sol-csharp-developers-guide/)
- [GPT-6 Astra for C# Developers (blog post)](https://taswar.zeytinsoft.com/gpt-6-astra-csharp-developers-guide/)
- [Introducing GPT-6.1 Sol in Microsoft Foundry](https://techcommunity.microsoft.com/blog/azure-ai-foundry-blog/introducing-gpt-6-1-sol-in-microsoft-foundry-advanced-intelligence-optimized-for/4560811)
- [Microsoft.Extensions.AI documentation](https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai)
- [.NET Secret Manager](https://learn.microsoft.com/aspnet/core/security/app-secrets)

## License

MIT. <!-- Add a LICENSE file, or remove this section and the badge. -->

If this saved you time, a ⭐ helps other .NET developers find it.
