using FluentValidation;
using LS.Application.Features.IAM.Users.Commands;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class UpdateProfilePictureCommandValidator : AbstractValidator<UpdateProfilePictureCommand>
{
    public UpdateProfilePictureCommandValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.Content).NotNull();
        RuleFor(request => request.FileName).NotEmpty();
        RuleFor(request => request.ContentType).NotEmpty();
        RuleFor(request => request.Length).GreaterThan(0);
    }
}
