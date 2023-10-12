﻿using System.CommandLine;

namespace Wipedir;

public static class Program
{
    public static void Main(string[] args) => __MainAsync(args);
    private async static void __MainAsync(string[] args)
    {
        Console.Title = "Wipedir";

        var startingDirectory = new Option<string>(name: "--start", description: "The starting directory") { IsRequired = true };
        startingDirectory.AddAlias("-s");
        var directories = new Option<string[]>(name: "--dir", description: "Directory to delete. For multiple directories provide the '-d' argument up to 10 times.") { IsRequired = true, Arity = new ArgumentArity(1, 10) };
        directories.AddAlias("-d");
        var force = new Option<bool>(name: "--force", description: "Force deletion (currently not implemented)", getDefaultValue: () => false);
        force.AddAlias("-f");
        var recursive = new Option<bool>(name: "--recursive", description: "Recursive search", getDefaultValue: () => false);
        recursive.AddAlias("-r");


        var rootCommand = new RootCommand("Wipedir");
        rootCommand.AddOption(startingDirectory);
        rootCommand.AddOption(directories);
        rootCommand.AddOption(force);
        rootCommand.AddOption(recursive);

        rootCommand.SetHandler((startDirValue, dirValue, forceValue, recursiveValue) =>
        {
            __Execute(startDirValue, dirValue, forceValue, recursiveValue);
        }, startingDirectory, directories, force, recursive);

        await rootCommand.InvokeAsync(args);
    }

    private static void __Execute(string startDir, string[] directories, bool force, bool recursive)
    {
#if DEBUG
        __PrintArgs(startDir, directories, force, recursive);
#endif
        __ValidateStartDirectory(startDir);

        var FolderPathes = __GetMatchingFolderPathes(startDir, directories, recursive);

        foreach (var folderPath in FolderPathes)
            Console.WriteLine(folderPath);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Press any key to continue...");
        Console.ResetColor();
        Console.ReadKey();

        __RemoveFolders(FolderPathes, force);
    }

    private static void __RemoveFolders(string[] directories, bool force)
    {
        foreach(var dir in directories)
        {
            try
            {
                Directory.Delete(dir, true);
            } catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Couldn't delete folder '{dir}'. Exception: {ex.ToString()}");
                Console.ResetColor();
            }
        }
    }

    private static void __PrintArgs(string startDir, string[] directories, bool force, bool recursive)
    {
        Console.WriteLine($"StartDir: {startDir}");
        Console.WriteLine($"Directory: {string.Join(",", directories)}");
        Console.WriteLine($"Force: {force}");
        Console.WriteLine($"Recursive: {recursive}");
    }

    private static string[]? __GetMatchingFolderPathes(string startDir, string[] directories, bool recursive)
    {
        List<string> Result = new List<string>();
        try
        {
            foreach(var dir in directories)
                Result.AddRange(Directory.GetDirectories(startDir, dir, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
        } catch(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Couldn't access all folders due to permission issues. {ex.ToString()}");
            Environment.Exit(1);
            return null;
        }
        return Result.ToArray();
    }

    private static void __ValidateStartDirectory(string value)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The argument '-s' can't be null or empty or only containing whitespaces.");
            Environment.Exit(1);
        }

        if (!Directory.Exists(value))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The argument -s with the value '{value}' is not a directory.");
            Environment.Exit(1);
        }
    }
}
