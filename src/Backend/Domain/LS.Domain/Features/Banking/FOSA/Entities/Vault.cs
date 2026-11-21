using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.FOSA.Entities;

public class Vault : BaseEntity
{
    public string Name { get; set; } = null!;
    public decimal CurrentBalance { get; set; }

    public static Vault Create(string name, string createdBy)
    {
        return new Vault
        {
            Name = name,
            CurrentBalance = 0,
            CreatedBy = createdBy
        };
    }
}
