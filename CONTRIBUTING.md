# Contributing

Thanks for helping improve this sample. It's a small .NET 10 console app with four samples that call GPT-6.1 Sol on Microsoft Foundry through `Microsoft.Extensions.AI` and the Responses API, with Entra ID sign-in. Contributions that make it clearer, more correct, or more useful to other .NET developers are welcome.

## Ways to contribute

- **Report a bug.** A sample fails, or the README doesn't match what the code does.
- **Fix the docs.** Typos, unclear steps, or a missing troubleshooting case.
- **Improve the sample.** For example, a streaming version of Case 1, or a clearer way to show the approval step in Case 2.
- **Share results.** If you compared reasoning effort (Case 4) on your own CI output, an issue with the token counts helps others.

For anything larger than a small fix, open an issue first so we can agree on the approach before you write code.

## Set up

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and the Azure CLI.
2. Clone the repo and build it:

   ```bash
   git clone https://github.com/taswar/Gpt-6.1-Sol-CSharp-Demo.git
   cd Gpt-6.1-Sol-CSharp-Demo
   dotnet build
   ```

3. To run it, deploy GPT-6.1 Sol on a Microsoft Foundry resource, set `AZURE_AI_ENDPOINT` and `AZURE_OPENAI_DEPLOYMENT` with user secrets, and sign in with `az login`, as described in the [README](README.md#run-it). Your account needs the **Cognitive Services OpenAI User** role on the resource. The sample uses Entra ID only, with no API key.

## Guidelines

- **Never commit secrets.** No keys, endpoints, tenant IDs or tokens in code, config files or screenshots. Settings belong in user secrets.
- **Keep the Responses API.** GPT-6.1 Sol rejects function tools with reasoning on Chat Completions. Don't switch the client back to `/chat/completions`.
- **Keep humans in charge.** Case 1 never approves or merges, and Case 2's `LookUpAccount` tool is read-only and returns a proposal for approval. Don't add tools that write, merge or send.
- **Keep the samples simulated.** The account, CI result and vendor documents are hard-coded. Don't connect a sample to a real system.
- **Keep the usage line.** Every call prints its token usage and time. Keep `Report` for any new sample, so readers can see the cost.
- **Keep the timeouts.** One five-minute budget covers the run, and each request has a three-minute network timeout. Keep them, or explain why you changed them.
- **Keep it a sample.** The code should stay short and readable in one `Program.cs`. Prefer clarity over abstraction, and add a dependency only when it clearly helps.
- **Match the existing style.** Target `net10.0`, keep nullable reference types on, and follow the layout already in `Program.cs`.
- **Keep the README in sync.** If you add a sample, change settings or change output, update the README in the same pull request, including the "What's inside" table.
- **One change per pull request.** Small, focused pull requests are easier to review and merge.

## Pull request checklist

- [ ] `dotnet build` succeeds with no new warnings.
- [ ] Any sample you changed runs against your own GPT-6.1 Sol deployment, or you explain why you couldn't run it.
- [ ] No secrets, endpoints or personal data are in the diff.
- [ ] The README reflects any change in samples, settings or output.
- [ ] The pull request description says what changed and why.

The build workflow runs on every pull request and has to pass before a merge.

## Reporting a bug

Open an issue and include:

- Which case failed and what you expected.
- The full error message, with any endpoints removed.
- Your .NET SDK version (`dotnet --version`) and operating system.
- Your deployment name, and whether the error was a timeout.

## Security issues

Please don't report security problems in a public issue. Use **Security → Report a vulnerability** on the repo to send a private report.

## License

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE) that covers this project.
