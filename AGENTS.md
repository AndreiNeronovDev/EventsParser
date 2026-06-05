# Agent Instructions

- Treat `EventsParser` as a read-only reference console app. Do not modify it unless the user explicitly asks.
- Make implementation changes only in `EventsIngestion.Service` or in new services/projects that are explicitly added for the ingestion solution.
- Keep service abstractions in `EventsIngestion.Service/Abstraction` and use the `EventsIngestion.Service.Abstraction` namespace.
- Keep shared message DTOs in the `EventsIngestion.Contracts` project so they can later be published as a GitHub NuGet package.
- Keep service-local runtime options in `EventsIngestion.Service/Options` unless they are part of the cross-repo message contract.
- Keep implementation classes in `Logic` until a more specific feature/source folder is introduced.
- Keep large source implementations in their own `EventsIngestion.Source.*` projects. The service should register and orchestrate them, not contain their parser internals.
- Add XML documentation comments to all abstractions, public classes/records without interfaces, public methods, and methods with non-trivial logic.
- Keep comments useful and concise. Avoid comments that merely restate the code.
- Never throw exceptions from constructors. Use explicit validation or readiness methods for runtime configuration and external dependency checks.
- The service should remain a Fargate-friendly one-shot worker task triggered by EventBridge: start, run the selected source extraction, publish/return results when implemented, and exit.
