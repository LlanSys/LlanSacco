using Microsoft.EntityFrameworkCore;

namespace LS.Persistence.Features.CheckOff.DataContext;

public class CheckOffSqlServerDBContext : CheckOffDBContext
{
    public CheckOffSqlServerDBContext(DbContextOptions<CheckOffSqlServerDBContext> options)
        : base(options)
    {
    }
}
