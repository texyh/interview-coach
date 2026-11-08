#!/bin/bash

# Test Runner Script for AIPA Backend
# This script runs unit tests, integration tests, and generates coverage reports

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_test() {
    echo -e "${PURPLE}[TEST]${NC} $1"
}

# Function to show usage
show_usage() {
    echo "Usage: $0 [COMMAND] [OPTIONS]"
    echo ""
    echo "Commands:"
    echo "  all                 Run all tests (default)"
    echo "  unit                Run unit tests only"
    echo "  integration         Run integration tests only"
    echo "  domain              Run domain tests only"
    echo "  application         Run application tests only"
    echo "  infrastructure      Run infrastructure tests only"
    echo "  webapi              Run web API tests only"
    echo "  watch               Run tests in watch mode"
    echo ""
    echo "Options:"
    echo "  --coverage, -c      Generate code coverage report"
    echo "  --verbose, -v       Verbose test output"
    echo "  --parallel, -p      Run tests in parallel (default)"
    echo "  --no-parallel       Run tests sequentially"
    echo "  --filter <pattern>  Run tests matching pattern"
    echo "  --build, -b         Build before running tests"
    echo "  --no-build          Skip build step"
    echo "  --help, -h          Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                      # Run all tests"
    echo "  $0 unit -c              # Run unit tests with coverage"
    echo "  $0 --filter \"User*\"    # Run tests matching User*"
    echo "  $0 domain -bv           # Build and run domain tests verbosely"
    echo "  $0 watch                # Run tests in watch mode"
}

# Check if dotnet is installed
check_dotnet() {
    if ! command -v dotnet &> /dev/null; then
        print_error "dotnet CLI is not installed or not in PATH"
        exit 1
    fi
}

# Get project directory
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOLUTION_FILE="$PROJECT_DIR/AIPA.sln"
TEST_RESULTS_DIR="$PROJECT_DIR/TestResults"
COVERAGE_DIR="$PROJECT_DIR/Coverage"

# Test project paths
DOMAIN_TESTS="$PROJECT_DIR/tests/AIPA.Domain.Tests"
APPLICATION_TESTS="$PROJECT_DIR/tests/AIPA.Application.Tests"
INFRASTRUCTURE_TESTS="$PROJECT_DIR/tests/AIPA.Infrastructure.Tests"
WEBAPI_TESTS="$PROJECT_DIR/tests/AIPA.WebApi.Tests"

# Check if projects exist
check_projects() {
    if [ ! -f "$SOLUTION_FILE" ]; then
        print_error "Solution file not found at: $SOLUTION_FILE"
        exit 1
    fi

    local test_projects=("$DOMAIN_TESTS" "$APPLICATION_TESTS" "$INFRASTRUCTURE_TESTS" "$WEBAPI_TESTS")
    local found_tests=false

    for project in "${test_projects[@]}"; do
        if [ -d "$project" ]; then
            found_tests=true
            break
        fi
    done

    if [ "$found_tests" = false ]; then
        print_error "No test projects found"
        exit 1
    fi
}

# Build solution
build_solution() {
    print_info "Building solution..."

    cd "$PROJECT_DIR"

    if dotnet build "$SOLUTION_FILE" --configuration Debug --no-restore; then
        print_success "Solution built successfully"
    else
        print_error "Failed to build solution"
        exit 1
    fi
}

# Restore packages
restore_packages() {
    print_info "Restoring NuGet packages..."

    cd "$PROJECT_DIR"

    if dotnet restore "$SOLUTION_FILE"; then
        print_success "Packages restored successfully"
    else
        print_error "Failed to restore packages"
        exit 1
    fi
}

# Create directories
create_directories() {
    mkdir -p "$TEST_RESULTS_DIR"
    mkdir -p "$COVERAGE_DIR"
}

# Run specific test project
run_test_project() {
    local project_path="$1"
    local project_name="$2"
    local verbose="$3"
    local parallel="$4"
    local filter="$5"
    local coverage="$6"
    local no_build="$7"

    if [ ! -d "$project_path" ]; then
        print_warning "Test project not found: $project_name"
        return 0
    fi

    print_test "Running $project_name tests..."

    cd "$project_path"

    # Build test command
    local test_cmd="dotnet test"

    if [ "$no_build" = "true" ]; then
        test_cmd="$test_cmd --no-build"
    fi

    if [ "$verbose" = "true" ]; then
        test_cmd="$test_cmd --verbosity normal"
    else
        test_cmd="$test_cmd --verbosity minimal"
    fi

    if [ "$parallel" = "false" ]; then
        test_cmd="$test_cmd --parallel false"
    fi

    if [ ! -z "$filter" ]; then
        test_cmd="$test_cmd --filter \"$filter\""
    fi

    # Add logger for test results
    test_cmd="$test_cmd --logger \"trx;LogFileName=${project_name}_results.trx\""
    test_cmd="$test_cmd --results-directory \"$TEST_RESULTS_DIR\""

    # Add coverage if requested
    if [ "$coverage" = "true" ]; then
        test_cmd="$test_cmd --collect:\"XPlat Code Coverage\""
        test_cmd="$test_cmd --settings \"$PROJECT_DIR/coverlet.runsettings\""
    fi

    print_info "Executing: $test_cmd"

    # Run the tests
    if eval $test_cmd; then
        print_success "$project_name tests passed"
        return 0
    else
        print_error "$project_name tests failed"
        return 1
    fi
}

# Generate coverage report
generate_coverage_report() {
    print_info "Generating coverage report..."

    if ! command -v reportgenerator &> /dev/null; then
        print_info "Installing ReportGenerator tool..."
        dotnet tool install --global dotnet-reportgenerator-globaltool || true
    fi

    # Find coverage files
    local coverage_files=$(find "$TEST_RESULTS_DIR" -name "coverage.cobertura.xml" | tr '\n' ';')

    if [ -z "$coverage_files" ]; then
        print_warning "No coverage files found"
        return 1
    fi

    # Generate HTML report
    if reportgenerator \
        "-reports:$coverage_files" \
        "-targetdir:$COVERAGE_DIR" \
        "-reporttypes:Html;HtmlSummary;Badges;TextSummary" \
        "-historydir:$COVERAGE_DIR/history"; then

        print_success "Coverage report generated at: $COVERAGE_DIR/index.html"

        # Show summary
        if [ -f "$COVERAGE_DIR/Summary.txt" ]; then
            echo ""
            print_info "Coverage Summary:"
            cat "$COVERAGE_DIR/Summary.txt"
        fi

        return 0
    else
        print_error "Failed to generate coverage report"
        return 1
    fi
}

# Run tests in watch mode
run_watch_mode() {
    local filter="$1"

    print_info "Starting test watch mode..."
    print_info "Press Ctrl+C to stop watching"

    cd "$PROJECT_DIR"

    local watch_cmd="dotnet watch test"

    if [ ! -z "$filter" ]; then
        watch_cmd="$watch_cmd --filter \"$filter\""
    fi

    exec $watch_cmd
}

# Main test runner function
run_tests() {
    local test_type="$1"
    local verbose="$2"
    local parallel="$3"
    local filter="$4"
    local coverage="$5"
    local build_first="$6"
    local no_build="$7"

    create_directories

    if [ "$build_first" = "true" ]; then
        restore_packages
        build_solution
        no_build="true"
    fi

    local failed_tests=0

    case "$test_type" in
        "domain")
            run_test_project "$DOMAIN_TESTS" "Domain" "$verbose" "$parallel" "$filter" "$coverage" "$no_build" || ((failed_tests++))
            ;;
        "application")
            run_test_project "$APPLICATION_TESTS" "Application" "$verbose" "$parallel" "$filter" "$coverage" "$no_build" || ((failed_tests++))
            ;;
        "infrastructure")
            run_test_project "$INFRASTRUCTURE_TESTS" "Infrastructure" "$verbose" "$parallel" "$filter" "$coverage" "$no_build" || ((failed_tests++))
            ;;
        "webapi")
            run_test_project "$WEBAPI_TESTS" "WebApi" "$verbose" "$parallel" "$filter" "$coverage" "$no_build" || ((failed_tests++))
            ;;
        "unit")
            run_test_project "$DOMAIN_TESTS" "Domain" "$verbose" "$parallel" "$filter" "$coverage" "$no_build" || ((failed_tests++))
            run_test_project "$APPLICATION_TESTS" "Application" "$verbose" "$parallel" "$filter" "$coverage" "$no_build" || ((failed_tests++))
            ;;
        "integration")
            run_test_project "$INFRASTRUCTURE_TESTS" "Infrastructure" "$verbose" "$parallel" "$filter" "$coverage" "$no_build" || ((failed_tests++))
            run_test_project "$WEBAPI_TESTS" "WebApi" "$verbose" "$parallel" "$filter" "$coverage" "$no_build" || ((failed_tests++))
            ;;
        "all"|*)
            run_test_project "$DOMAIN_TESTS" "Domain" "$verbose" "$parallel" "$filter" "$coverage" "$no_build" || ((failed_tests++))
            run_test_project "$APPLICATION_TESTS" "Application" "$verbose" "$parallel" "$filter" "$coverage" "$no_build" || ((failed_tests++))
            run_test_project "$INFRASTRUCTURE_TESTS" "Infrastructure" "$verbose" "$parallel" "$filter" "$coverage" "$no_build" || ((failed_tests++))
            run_test_project "$WEBAPI_TESTS" "WebApi" "$verbose" "$parallel" "$filter" "$coverage" "$no_build" || ((failed_tests++))
            ;;
    esac

    # Generate coverage report if requested
    if [ "$coverage" = "true" ]; then
        generate_coverage_report
    fi

    # Summary
    echo ""
    if [ $failed_tests -eq 0 ]; then
        print_success "All tests completed successfully!"
    else
        print_error "$failed_tests test project(s) failed"
        exit 1
    fi
}

# Parse arguments
parse_arguments() {
    local test_type="all"
    local verbose="false"
    local parallel="true"
    local filter=""
    local coverage="false"
    local build_first="false"
    local no_build="false"
    local watch_mode="false"

    while [[ $# -gt 0 ]]; do
        case $1 in
            all|unit|integration|domain|application|infrastructure|webapi)
                test_type="$1"
                shift
                ;;
            watch)
                watch_mode="true"
                shift
                ;;
            --coverage|-c)
                coverage="true"
                shift
                ;;
            --verbose|-v)
                verbose="true"
                shift
                ;;
            --parallel|-p)
                parallel="true"
                shift
                ;;
            --no-parallel)
                parallel="false"
                shift
                ;;
            --filter)
                filter="$2"
                shift 2
                ;;
            --build|-b)
                build_first="true"
                shift
                ;;
            --no-build)
                no_build="true"
                shift
                ;;
            --help|-h)
                show_usage
                exit 0
                ;;
            -*|--*)
                # Handle combined flags like -bv
                if [[ $1 =~ ^-[bcvp]+$ ]]; then
                    for ((i=1; i<${#1}; i++)); do
                        case ${1:$i:1} in
                            b) build_first="true" ;;
                            c) coverage="true" ;;
                            v) verbose="true" ;;
                            p) parallel="true" ;;
                            *) print_error "Unknown option: -${1:$i:1}"; exit 1 ;;
                        esac
                    done
                else
                    print_error "Unknown option $1"
                    show_usage
                    exit 1
                fi
                shift
                ;;
            *)
                print_error "Unknown argument: $1"
                show_usage
                exit 1
                ;;
        esac
    done

    # Handle watch mode
    if [ "$watch_mode" = "true" ]; then
        run_watch_mode "$filter"
        return
    fi

    # Run tests
    run_tests "$test_type" "$verbose" "$parallel" "$filter" "$coverage" "$build_first" "$no_build"
}

# Main script logic
main() {
    print_info "AIPA Backend Test Runner"
    echo ""

    check_dotnet
    check_projects

    if [ $# -eq 0 ]; then
        # Default: run all tests
        parse_arguments "all"
    else
        parse_arguments "$@"
    fi
}

# Handle Ctrl+C gracefully
trap 'echo -e "\n${YELLOW}[INFO]${NC} Tests stopped by user"; exit 0' INT

# Run main function with all arguments
main "$@"
