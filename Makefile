NAME ?= ocelot

build: tool_restore
	dotnet cake

build_and_run_tests: tool_restore
	dotnet cake --target=RunTests

release: tool_restore
	dotnet cake --target=Release

run_acceptance_tests: tool_restore
	dotnet cake --target=RunAcceptanceTests

run_benchmarks: tool_restore
	dotnet cake --target=RunBenchmarkTests

run_unit_tests: tool_restore
	dotnet cake --target=RunUnitTests

release_notes: tool_restore
	dotnet cake --target=ReleaseNotes

tool_restore:
	dotnet tool restore
