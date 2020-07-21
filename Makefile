NAME ?= ocelot

build:
	./build.sh

build_and_run_tests:
	./build.sh --target=RunTests

release:
	./build.sh --target=Release

run_acceptance_tests:
	./build.sh --target=RunAcceptanceTests

run_benchmarks:
	./build.sh --target=RunBenchmarkTests

run_unit_tests:
	./build.sh --target=RunUnitTests

release_notes:
	./build.sh --target=ReleaseNotes
	