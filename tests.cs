#:project tests/GEngine.Core.Tests/GEngine.Core.Tests.csproj
#:project tests/GEngine.Physics.Tests/GEngine.Physics.Tests.csproj
#:project tests/GEngine.Rendering.Tests/GEngine.Rendering.Tests.csproj
#:project tests/GEngine.Input.Tests/GEngine.Input.Tests.csproj
#:project tests/GEngine.Testing.Tests/GEngine.Testing.Tests.csproj
#:project tests/MarioClone.Tests/MarioClone.Tests.csproj
#:project tests/GEngine.Architecture.Tests/GEngine.Architecture.Tests.csproj
#:project src/GEngine.Testing/GEngine.Testing.csproj

// Every test in the repository, in one command:
//
//     dotnet run tests.cs
//     dotnet run tests.cs --filter=Physics --verbose
//     dotnet run tests.cs --list
//
// The suites are named by their marker type rather than by a test class, so renaming a
// test never breaks this file. Exit code: 0 all good, 1 something failed, 2 bad argument.

using System.Reflection;
using GEngine.Architecture.Tests;
using GEngine.Core.Tests;
using GEngine.Input.Tests;
using GEngine.Physics.Tests;
using GEngine.Rendering.Tests;
using GEngine.Testing;
using GEngine.Testing.Tests;
using MarioClone.Tests;

Assembly[] suites =
[
    CoreTestSuite.Assembly,
    PhysicsTestSuite.Assembly,
    RenderingTestSuite.Assembly,
    InputTestSuite.Assembly,
    FrameworkTestSuite.Assembly,
    MarioTestSuite.Assembly,
    ArchitectureTestSuite.Assembly,
];

return TestRunner.Run(args, suites);
