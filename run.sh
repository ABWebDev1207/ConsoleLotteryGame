#!/bin/bash

echo "🎰 Bede Lottery Game Runner"
echo "=========================="

# Check if dotnet is available
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 9.0 SDK."
    exit 1
fi

echo "✅ .NET SDK found: $(dotnet --version)"
echo ""

# Build the solution
echo "🔨 Building the solution..."
if dotnet build --verbosity quiet; then
    echo "✅ Build successful!"
else
    echo "❌ Build failed!"
    exit 1
fi

echo ""

# Run tests
echo "🧪 Running tests..."
if dotnet test --verbosity quiet --nologo; then
    echo "✅ All tests passed!"
else
    echo "❌ Some tests failed!"
    exit 1
fi

echo ""
echo "🎮 Starting the Bede Lottery Game..."
echo "===================================="
echo ""

# Run the application
dotnet run --project src/BedeLottery
