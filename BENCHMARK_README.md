# LogicGP Benchmark Suite

Cross-platform benchmarking tool to compare Python and C# LogicGP implementations.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│ Python Benchmark Harness                                    │
│ (scoredrulesets.benchmarking.csharp_wrapper)               │
└────────────┬────────────────────────────────────────────────┘
             │
             ├─→ Runs Python LogicGP benchmarks
             │
             └─→ Calls C# CLI Tool (subprocess)
                     ↓
        ┌─────────────────────────────┐
        │ C# CLI Benchmark Tool       │
        │ (Italbytz.EA.Benchmark.Cli) │
        └─────────────────────────────┘
             │
             ├─→ `run` command: Trains LogicGP model,
             │  exports JSON metrics
             │
             └─→ `compare` command: Compares two JSON results
                     ↓
        ┌──────────────────────────┐
        │ JSON Output (Metrics)    │
        │ - f1_score               │
        │ - train_time_ms          │
        │ - num_rules              │
        │ - num_atoms              │
        └──────────────────────────┘
```

## Setup & Build

### C# Side

```bash
cd /path/to/nuget-adapters-algorithms-ea/Italbytz.Adapters.Algorithms.EA

# Build the CLI tool
dotnet build

# Or build for Release (optimized):
dotnet build -c Release
```

The CLI tool will be available at:
- Debug: `Italbytz.Adapters.Algorithms.EA.Benchmark.Cli/bin/Debug/net9.0/Italbytz.Adapters.Algorithms.EA.Benchmark.Cli`
- Release: `Italbytz.Adapters.Algorithms.EA.Benchmark.Cli/bin/Release/net9.0/Italbytz.Adapters.Algorithms.EA.Benchmark.Cli`

### Python Side

```bash
cd /path/to/pypi-scoredrulesets

# Install with benchmark extras
pip install -e ".[dev,benchmark]"
```

## Usage Examples

### Run C# Benchmark via CLI

```bash
# Run a single benchmark
./Italbytz.Adapters.Algorithms.EA.Benchmark.Cli run \
  --trainer flcw-macro \
  --dataset iris \
  --generations 100 \
  --population 1000 \
  --seed 42 \
  --output result.json

# Output: JSON file with metrics
```

### Run from Python

```python
from scoredrulesets.benchmarking.csharp_wrapper import CSharBenchmarkRunner, BenchmarkComparator

# Initialize runner
runner = CSharBenchmarkRunner()

# Run a benchmark
result = runner.run_benchmark(
    trainer="flcw-macro",
    dataset="iris",
    generations=100,
    population=1000,
    seed=42
)

print(f"F1 Score: {result.f1_score}")
print(f"Time: {result.total_time_ms}ms")
```

### Compare Python vs C# Results

```python
from scoredrulesets.benchmarking.csharp_wrapper import BenchmarkComparator

comparison = BenchmarkComparator.compare_results(
    python_result=python_bench,
    csharp_result=csharp_bench
)

report = BenchmarkComparator.generate_comparison_report([comparison])
print(report)
```

## Key Design Decisions

### Feature Discretization

Both implementations use **categorical/discrete features**:

- **C#**: Uses `FeatureBinningAndCustomLabelMapping` in the ML.NET pipeline
- **Python**: Uses Quantile binning (sklearn's KBinsDiscretizer)

This ensure consistency across implementations.

### JSON Output Format

Both implementations produce JSON with:
```json
{
  "trainer": "flcw-macro",
  "dataset": "iris",
  "f1_score": 0.9667,
  "train_time_ms": 152,
  "total_time_ms": 200,
  "num_rules": 3,
  "num_atoms": 12,
  "status": "ok",
  "timestamp": "2026-03-28T10:15:30Z"
}
```

### Error Handling

- Divergence > 5%: Warning in report
- Divergence > 10%: Critical (possible implementation differences)
- Timeout > 5 minutes: Aborted with error status

## Benchmarks to Run

Recommended test matrix:

| Trainer | Dataset | Generations | Population | Notes |
|---------|---------|-------------|------------|-------|
| flcw-macro | iris | 100 | 1000 | Baseline, fast |
| flcw-micro | iris | 100 | 1000 | Label weighted |
| rlcw-macro | iris | 50 | 500 | New trainer |
| flcw-macro | npha | 20 | 1000 | Larger dataset |

## Troubleshooting

### CLI Tool Not Found

```python
runner = CSharBenchmarkRunner(cli_path=Path("/explicit/path/to/cli"))
```

### Feature Mismatch Between Implementations

Check that both use the same discretization method:
- Python: quantile binning with n_bins=5 (default)
- C#: ML.NET FeatureBinning (default bins)

If F1 divergence > 5%, examine bin boundaries.

### Build Issues

```bash
# Clean and rebuild
dotnet clean
dotnet build -c Release
```

## Next Steps

1. ✅ CLI scaffold created with `run` and `compare` commands
2. ✅ Python wrapper created
3. ⏳ **TODO**: Implement rule/atom extraction in C#
4. ⏳ **TODO**: Integrate into CI pipeline
5. ⏳ **TODO**: Generate automated comparison reports

