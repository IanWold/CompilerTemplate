using CommandLine;
using CompilerTemplate.Cli;

Parser.Default.ParseArguments<Build>(args)
    .WithParsed(build => build.Execute())
    .WithNotParsed(errors => Console.WriteLine($"Did not understand '{string.Join(" ", args)}'"));
