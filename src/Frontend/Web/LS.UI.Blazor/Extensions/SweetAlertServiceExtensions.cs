using CurrieTechnologies.Razor.SweetAlert2;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Extensions;

internal static class SweetAlertServiceExtensions
{
    private const string ThemePrimaryColor = "#112250";
    private const string ThemeErrorColor = "#b71c1c";

    /// <summary>
    /// Displays a standard confirmation dialog using the application's theme colors.
    /// </summary>
    public static async Task<bool> FireConfirmAsync(
        this SweetAlertService swal,
        string title,
        string text = "Are you sure you want to proceed?",
        string confirmButtonText = "Yes",
        string cancelButtonText = "Cancel",
        SweetAlertIcon? icon = null,
        bool isDestructive = false)
    {
        var result = await swal.FireAsync(new SweetAlertOptions
        {
            Title = title,
            Text = text,
            Icon = icon ?? SweetAlertIcon.Warning,
            ShowCancelButton = true,
            ConfirmButtonText = confirmButtonText,
            CancelButtonText = cancelButtonText,
            ConfirmButtonColor = isDestructive ? ThemeErrorColor : ThemePrimaryColor,
            CancelButtonColor = "#757575", // standard grey
            ReverseButtons = true
        }).ConfigureAwait(false);

        return result.IsConfirmed;
    }
}

