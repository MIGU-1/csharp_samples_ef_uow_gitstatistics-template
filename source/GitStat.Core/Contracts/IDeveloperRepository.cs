using GitStat.Core.Entities;
using System.Collections.Generic;

namespace GitStat.Core.Contracts
{
    public interface IDeveloperRepository
    {
        IEnumerable<Developer> GetDevopsAndCommits();
    }
}
