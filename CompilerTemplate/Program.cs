using CommandLine;
using CompilerTemplate;

CommandLine.Parser.Default.ParseArguments<Build>(args)
    .WithParsed(build => build.Execute())
    .WithNotParsed(errors => Console.WriteLine($"Did not understand '{string.Join(" ", args)}'"));
