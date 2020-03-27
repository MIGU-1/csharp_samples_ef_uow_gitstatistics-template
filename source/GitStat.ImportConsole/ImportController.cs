using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GitStat.Core.Entities;
using Utils;

namespace GitStat.ImportConsole
{
    public class ImportController
    {
        const string Filename = "commits.txt";
        private static List<Developer> _devops = new List<Developer>();

        /// <summary>
        /// Liefert die Messwerte mit den dazugehörigen Sensoren
        /// </summary>
        public static Commit[] ReadFromCsv()
        {
            List<Commit> commits = new List<Commit>();
            string[] lines = System.IO.File.ReadAllLines(MyFile.GetFullNameInApplicationTree(Filename));
            List<CommitBlock> blocks = GetBlocks(lines);
            _devops = GetDevops(blocks);
        }

        private static List<Developer> GetDevops(List<CommitBlock> blocks)
        {
            foreach (CommitBlock block in blocks)
            {
                string[] headData = block.Head.Split(',');
                string hashCode = headData[0];
                string Name = headData[1];
                DateTime date = DateTime.Parse(headData[2]);
                string message = headData[3];

                if (_devops.Where(d => d.Name == Name).SingleOrDefault() == null)
                {
                    Developer devop = new Developer
                    {
                        Name = Name,
                    };

                    _devops.Add(devop);
                }
            }
        }

        private static List<CommitBlock> GetBlocks(string[] lines)
        {
            List<CommitBlock> blocks = new List<CommitBlock>();
            int idx = 0;

            while (idx < lines.Length)
            {
                CommitBlock block = new CommitBlock();

                while (idx < lines.Length && !String.IsNullOrEmpty(lines[idx]))
                {
                    if (block.Head != null)
                    {
                        block.BodyLines.Add(lines[idx]);
                    }
                    else
                    {
                        block.Head = lines[idx];
                    }

                    idx++;
                }

                blocks.Add(block);
                idx++;
            }

            return blocks;
        }
        private class CommitBlock
        {
            public string Head { get; set; }
            public List<string> BodyLines { get; set; }

            public CommitBlock()
            {
                BodyLines = new List<string>();
            }
        }
    }
}
