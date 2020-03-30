using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GitStat.Core.Contracts;
using GitStat.Core.Entities;
using GitStat.Persistence;
using ConsoleTableExt;

namespace GitStat.ImportConsole
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Import der Commits in die Datenbank");
            using (IUnitOfWork unitOfWorkImport = new UnitOfWork())
            {
                Console.WriteLine("Datenbank löschen");
                unitOfWorkImport.DeleteDatabase();
                Console.WriteLine("Datenbank migrieren");
                unitOfWorkImport.MigrateDatabase();
                Console.WriteLine("Commits werden von commits.txt eingelesen");
                var commits = ImportController.ReadFromCsv();
                if (commits.Length == 0)
                {
                    Console.WriteLine("!!! Es wurden keine Commits eingelesen");
                    return;
                }
                Console.WriteLine(
                    $"  Es wurden {commits.Count()} Commits eingelesen, werden in Datenbank gespeichert ...");
                unitOfWorkImport.CommitRepository.AddRange(commits);
                int countDevelopers = commits.GroupBy(c => c.Developer).Count();
                int savedRows = unitOfWorkImport.SaveChanges();
                Console.WriteLine(
                    $"{countDevelopers} Developers und {savedRows - countDevelopers} Commits wurden in Datenbank gespeichert!");
                Console.WriteLine();
                var csvCommits = commits.Select(c =>
                    $"{c.Developer.Name};{c.Date};{c.Message};{c.HashCode};{c.FilesChanges};{c.Insertions};{c.Deletions}");
                File.WriteAllLines("commits.csv", csvCommits, Encoding.UTF8);
            }
            Console.WriteLine("Datenbankabfragen");
            Console.WriteLine("=================");

            using (IUnitOfWork unitOfWork = new UnitOfWork())
            {
                List<Commit> commitsOf2019 = unitOfWork.CommitRepository.GetCommitsOf2019().ToList();

                Console.WriteLine("Commits von 2019!");
                Console.WriteLine();

                ConsoleTableBuilder.From(commitsOf2019
                    .Select(o => new object[]
                    {
                        o.Developer.Name,
                        o.Date,
                        o.FilesChanges,
                        o.Insertions,
                        o.Deletions
                    })
                    .ToList())
                    .WithColumn(
                        "Name",
                        "Date",
                        "FileChanges",
                        "Insertions",
                        "Deletions"
                    )
                    .WithFormat(ConsoleTableBuilderFormat.Minimal)
                    .WithOptions(new ConsoleTableBuilderOption { DividerString = "" })
                    .ExportAndWrite();

                Console.WriteLine();
                Console.WriteLine("____________________________________________");
                Console.WriteLine();

                Commit commit = unitOfWork.CommitRepository.GetCommitByID(4);
                Console.WriteLine("Commit mit ID 4");
                Console.WriteLine();
                Console.WriteLine(commit.ToString());
                Console.WriteLine();

                List<Developer> devops = unitOfWork.DeveloperRepository.GetDevopsAndCommits().OrderBy(d => d.Commits.Count()).ToList();

                Console.WriteLine("Statistik der Developer");
                Console.WriteLine("____________________________________________");

                ConsoleTableBuilder.From(devops
                    .Select(o => new object[]
                    {
                        o.Name,
                        o.Commits.Count(),
                        o.Commits.Select(c => c.FilesChanges).Sum(),
                        o.Commits.Select(c => c.Insertions).Sum(),
                        o.Commits.Select(c => c.Deletions).Sum()
                    })
                    .ToList())
                    .WithColumn(
                        "Name",
                        "FileChanges",
                        "Insertions",
                        "Deletions"
                    )
                    .WithFormat(ConsoleTableBuilderFormat.Minimal)
                    .WithOptions(new ConsoleTableBuilderOption { DividerString = "" })
                    .ExportAndWrite();

                Console.WriteLine();
            }
            Console.Write("Beenden mit Eingabetaste ...");
            Console.ReadLine();
        }

    }
}
