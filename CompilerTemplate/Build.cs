using System.Diagnostics;
using System.Text;
using CommandLine;

namespace CompilerTemplate;

[Verb("build", HelpText = "Compile a CompilerTemplate project.")]
public class Build
{
    [Option('d', "directory", Required = false, HelpText = "The root directory of the project to compile.")]
    public string? ProjectDirectory { get; set; }

    [Option('o', "output", Required = true, HelpText = "The name of the executable file to output.")]
    public string? OutputFile { get; set; }

    private string TemporaryDirectory
    {
        get
        {
            if (string.IsNullOrEmpty(field))
            {
                field = Path.Combine(Path.GetTempPath(), "compilertemplate-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(field);
            }

            return field;
        }
    }

    private string WorkingDirectory => field ??= ProjectDirectory ?? AppContext.BaseDirectory;

    private string IntermediatePath => field ??= Path.Combine(TemporaryDirectory, "out.ssa");
    private string AssemblyPath => field ??= Path.Combine(TemporaryDirectory, "out.s");
    private string ExecutablePath => field ??= Path.Combine(WorkingDirectory, OutputFile ?? throw new Exception());

    public void Execute()
    {
        var source = File.ReadAllText(Path.Combine(WorkingDirectory, "Program.ct"));

        var ir = source.Lex().Parse().Analyze().Optimize().Lower();

        File.WriteAllText(IntermediatePath, ir, Encoding.ASCII);

        RunOrThrow("qbe", $"-o \"{AssemblyPath}\" \"{IntermediatePath}\"", workingDir: TemporaryDirectory);
        RunOrThrow("cc", $"\"{AssemblyPath}\" -o \"{ExecutablePath}\"", workingDir: TemporaryDirectory);

        Console.WriteLine("Built: " + ExecutablePath);
        Console.WriteLine("Temp dir: " + TemporaryDirectory);
    }

    static void RunOrThrow(string fileName, string args, string? workingDir = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            WorkingDirectory = workingDir ?? Environment.CurrentDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var p = Process.Start(psi) ?? throw new Exception($"Failed to start: {fileName}");
        
        var stdout = p.StandardOutput.ReadToEnd();
        var stderr = p.StandardError.ReadToEnd();

        p.WaitForExit();

        if (p.ExitCode != 0)
        {
            throw new Exception(
                $"Command failed: {fileName} {args}\n" +
                $"ExitCode: {p.ExitCode}\n" +
                (stdout.Length > 0 ? $"--- stdout ---\n{stdout}\n" : "") +
                (stderr.Length > 0 ? $"--- stderr ---\n{stderr}\n" : "")
            );
        }
    }
}
