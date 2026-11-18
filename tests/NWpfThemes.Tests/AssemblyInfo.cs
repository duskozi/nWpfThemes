using Xunit;

// Every test runs on the single WPF thread, so running them in parallel gains nothing.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
