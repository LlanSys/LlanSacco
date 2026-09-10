using System.ComponentModel.DataAnnotations;

namespace LS.SharedKernel.Features.Shared.Lookups.Dtos;

public sealed record CreateLookupRequest(
    [Required, StringLength(100)] string Code,
    [Required, StringLength(200)] string Description);
