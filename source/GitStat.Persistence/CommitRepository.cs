using System;
using System.Collections.Generic;
using System.Linq;
using GitStat.Core.Contracts;
using GitStat.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GitStat.Persistence
{
    public class CommitRepository : ICommitRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CommitRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddRange(Commit[] commits)
        {
            _dbContext.Commits.AddRange(commits);
        }
        public Commit GetCommitByID(int id) => _dbContext.Commits
            .Where(c => c.Id == id)
            .Include(c => c.Developer)
            .SingleOrDefault();
        public IEnumerable<Commit> GetCommitsOf2019() => _dbContext.Commits
            .Where(c => c.Date.Year == 2019)
            .Include(c => c.Developer);
    }
}