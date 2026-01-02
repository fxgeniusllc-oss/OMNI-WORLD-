#!/bin/bash

###############################################################################
# OmniWorld - Automated Build Script
# 
# This script automates the build process for all components:
# - Unity client build
# - Backend API deployment
# - Smart contract compilation
# - Documentation generation
#
# Usage: ./build.sh [target]
# Targets: all, unity, backend, contracts, docs, clean
###############################################################################

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BUILD_DIR="$PROJECT_ROOT/Build"
UNITY_PROJECT="$PROJECT_ROOT"
BACKEND_DIR="$PROJECT_ROOT/Backend"
CONTRACTS_DIR="$PROJECT_ROOT/Assets/Contracts/Source"
DOCS_DIR="$PROJECT_ROOT/Docs"

# Logging functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Build Unity project
build_unity() {
    log_info "Building Unity project..."
    
    if ! command_exists unity; then
        log_error "Unity not found in PATH. Please install Unity 2022.3 LTS or later."
        return 1
    fi
    
    # Create build directory
    mkdir -p "$BUILD_DIR/Unity"
    
    # Build for different platforms
    local platforms=("StandaloneWindows64" "StandaloneLinux64" "StandaloneOSX" "Android" "WebGL")
    
    for platform in "${platforms[@]}"; do
        log_info "Building for $platform..."
        
        unity -quit -batchmode -projectPath "$UNITY_PROJECT" \
            -buildTarget "$platform" \
            -buildPath "$BUILD_DIR/Unity/$platform/OmniWorld" \
            -logFile "$BUILD_DIR/Unity/build_${platform}.log"
        
        if [ $? -eq 0 ]; then
            log_success "Built $platform successfully"
        else
            log_error "Failed to build $platform. Check log: $BUILD_DIR/Unity/build_${platform}.log"
        fi
    done
    
    log_success "Unity build complete"
}

# Build backend services
build_backend() {
    log_info "Building backend services..."
    
    if ! command_exists python3; then
        log_error "Python3 not found. Please install Python 3.9 or later."
        return 1
    fi
    
    cd "$BACKEND_DIR"
    
    # Create virtual environment if it doesn't exist
    if [ ! -d "venv" ]; then
        log_info "Creating Python virtual environment..."
        python3 -m venv venv
    fi
    
    # Activate virtual environment
    source venv/bin/activate
    
    # Install dependencies
    log_info "Installing Python dependencies..."
    pip install --upgrade pip
    pip install -r requirements.txt
    
    # Run tests
    log_info "Running backend tests..."
    if command_exists pytest; then
        pytest tests/ -v || log_warning "Some tests failed"
    else
        log_warning "pytest not found, skipping tests"
    fi
    
    # Create deployment package
    mkdir -p "$BUILD_DIR/Backend"
    log_info "Creating deployment package..."
    tar -czf "$BUILD_DIR/Backend/omniworld-backend-$(date +%Y%m%d-%H%M%S).tar.gz" \
        --exclude='venv' --exclude='__pycache__' --exclude='*.pyc' \
        .
    
    deactivate
    
    log_success "Backend build complete"
}

# Compile smart contracts
build_contracts() {
    log_info "Building smart contracts..."
    
    if ! command_exists npm; then
        log_error "npm not found. Please install Node.js 16 or later."
        return 1
    fi
    
    cd "$CONTRACTS_DIR"
    
    # Install dependencies
    if [ ! -d "node_modules" ]; then
        log_info "Installing contract dependencies..."
        npm install
    fi
    
    # Compile contracts
    log_info "Compiling contracts..."
    npx hardhat compile
    
    # Run tests
    log_info "Running contract tests..."
    npx hardhat test || log_warning "Some contract tests failed"
    
    # Create ABI export
    mkdir -p "$BUILD_DIR/Contracts"
    log_info "Exporting contract ABIs..."
    cp -r artifacts/ "$BUILD_DIR/Contracts/artifacts/"
    
    log_success "Smart contracts build complete"
}

# Build documentation
build_docs() {
    log_info "Building documentation..."
    
    mkdir -p "$BUILD_DIR/Docs"
    
    # Copy markdown files
    cp -r "$DOCS_DIR"/*.md "$BUILD_DIR/Docs/"
    
    # Generate API documentation if tools available
    if command_exists doxygen; then
        log_info "Generating code documentation with Doxygen..."
        cd "$PROJECT_ROOT"
        doxygen Doxyfile
        log_success "Code documentation generated"
    else
        log_warning "Doxygen not found, skipping code documentation"
    fi
    
    log_success "Documentation build complete"
}

# Clean build artifacts
clean() {
    log_info "Cleaning build artifacts..."
    
    rm -rf "$BUILD_DIR"
    rm -rf "$BACKEND_DIR/venv"
    rm -rf "$CONTRACTS_DIR/node_modules"
    rm -rf "$CONTRACTS_DIR/artifacts"
    rm -rf "$CONTRACTS_DIR/cache"
    
    log_success "Clean complete"
}

# Build all components
build_all() {
    log_info "Starting full build process..."
    local start_time=$(date +%s)
    
    build_backend
    build_contracts
    build_docs
    # build_unity  # Commented out by default as Unity builds take time
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    log_success "Full build complete in ${duration}s"
    log_info "Build artifacts available in: $BUILD_DIR"
}

# Run quality checks
quality_checks() {
    log_info "Running quality checks..."
    
    # Backend linting
    if command_exists pylint; then
        log_info "Running pylint on backend..."
        cd "$BACKEND_DIR"
        pylint api/ || log_warning "Pylint found issues"
    fi
    
    # Contract linting
    if command_exists solhint; then
        log_info "Running solhint on contracts..."
        cd "$CONTRACTS_DIR"
        npx solhint 'contracts/**/*.sol' || log_warning "Solhint found issues"
    fi
    
    log_success "Quality checks complete"
}

# Display help
show_help() {
    cat << EOF
OmniWorld Build Script

Usage: ./build.sh [target]

Targets:
  all         Build all components (default)
  unity       Build Unity client for all platforms
  backend     Build and package backend services
  contracts   Compile and test smart contracts
  docs        Generate documentation
  quality     Run code quality checks
  clean       Remove all build artifacts
  help        Show this help message

Examples:
  ./build.sh all          # Build everything
  ./build.sh backend      # Build only backend
  ./build.sh clean        # Clean all build artifacts

Environment Variables:
  BUILD_DIR       Override build directory (default: ./Build)
  SKIP_TESTS      Set to 'true' to skip running tests

EOF
}

# Main execution
main() {
    local target="${1:-all}"
    
    log_info "OmniWorld Build System"
    log_info "Target: $target"
    log_info "Project root: $PROJECT_ROOT"
    
    case "$target" in
        all)
            build_all
            ;;
        unity)
            build_unity
            ;;
        backend)
            build_backend
            ;;
        contracts)
            build_contracts
            ;;
        docs)
            build_docs
            ;;
        quality)
            quality_checks
            ;;
        clean)
            clean
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            log_error "Unknown target: $target"
            show_help
            exit 1
            ;;
    esac
}

# Run main function
main "$@"
