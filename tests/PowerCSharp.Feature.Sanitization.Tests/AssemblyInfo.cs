// The static SanitizationEngine holds process-wide mutable state (the configuration provider set
// via SetConfigurationProvider, the security-event logger, and performance counters). Tests that
// exercise DI/module wiring mutate that shared state; running them concurrently with other tests
// in this assembly could make an unrelated test observe settings it never configured. Disabling
// parallelization keeps the suite deterministic — the tradeoff is acceptable given this project's
// size.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
