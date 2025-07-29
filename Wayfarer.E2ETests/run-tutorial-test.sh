#!/bin/bash
# Run the comprehensive tutorial test

echo "=== RUNNING COMPREHENSIVE TUTORIAL TEST ==="

# Navigate to the E2E tests directory
cd /mnt/c/git/wayfarer/Wayfarer.E2ETests

# Create a temporary project file that uses ComprehensiveTutorialTest as the entry point
cat > Wayfarer.E2ETests.Tutorial.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <StartupObject>ComprehensiveTutorialTest</StartupObject>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="../src/Wayfarer.csproj" />
  </ItemGroup>
  
  <ItemGroup>
    <Compile Include="ComprehensiveTutorialTest.cs" />
  </ItemGroup>
  
  <!-- Copy content files to project directory for E2E test -->
  <ItemGroup>
    <ContentFiles Include="../src/Content/Templates/**/*.*" />
  </ItemGroup>
  
  <Target Name="CopyContentFiles" AfterTargets="Build">
    <Copy SourceFiles="@(ContentFiles)" 
          DestinationFiles="@(ContentFiles->'Content/Templates/%(RecursiveDir)%(Filename)%(Extension)')"
          SkipUnchangedFiles="true" />
  </Target>
</Project>
EOF

# Build and run the tutorial test
dotnet run --project Wayfarer.E2ETests.Tutorial.csproj

# Clean up temporary project file
rm -f Wayfarer.E2ETests.Tutorial.csproj