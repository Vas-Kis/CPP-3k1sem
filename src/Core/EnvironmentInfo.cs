using System.Runtime.InteropServices;
namespace Core; // корінний namespace = ім'я проєкту Core

public sealed record EnvironmentReport(
string OsDescription,
string FrameworkDescription,
string ProcessArchitecture,
string DetectedRid,
string ReportedRid,
string BaseDirectory,
string EnvironmentOS,
string DotNetVersion,
string CurrentDirectory,
string BuildNote);

public static class EnvironmentInfo
{
#if NET10_0_OR_GREATER
    public const string BuildNote = "збірка під net10.0";
#else
    public const string BuildNote = "збірка під net8.0";
#endif

    public static EnvironmentReport Collect() => new(
    RuntimeInformation.OSDescription,
    RuntimeInformation.FrameworkDescription,
    RuntimeInformation.ProcessArchitecture.ToString(),
    DetectRid(),
    RuntimeInformation.RuntimeIdentifier,
    AppContext.BaseDirectory,
    Environment.OSVersion.ToString(),
    Environment.Version.ToString(),
    Environment.CurrentDirectory,
    BuildNote);
    // Ручне визначення RID: показує, з чого складається рядок win-x64.
    private static string DetectRid()
        {
        string os =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "unknown";
        string arch = RuntimeInformation.ProcessArchitecture switch
            {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "unknown"
            };
        return $"{os}-{arch}";
        }
}