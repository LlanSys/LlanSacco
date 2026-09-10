using Microsoft.EntityFrameworkCore;

namespace LS.Persistence.Features.CheckOff.DataContext;

public class CheckOffPostgreSqlDBContext : CheckOffDBContext
{
    public CheckOffPostgreSqlDBContext(DbContextOptions<CheckOffPostgreSqlDBContext> options)
        : base(options)
    {
    }
}
