using LS.Domain.Features.IAM.Menus.Entities;

namespace LS.Application.Features.IAM.Menus;

/// <summary>
/// A menu item is visible only when it and every ancestor are permitted (ported from StarterTemplate). Cycles and
/// chains deeper than 64 levels are treated as not visible.
/// </summary>
public static class NavigationHierarchy
{
    public static IReadOnlyList<MenuItem> Visible(IEnumerable<MenuItem> items, Func<MenuItem, bool> permitted)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(permitted);

        var lookup = items.ToDictionary(static item => item.Id);
        var resolved = new Dictionary<Guid, bool>();

        bool Visit(MenuItem item, HashSet<Guid> path)
        {
            if (resolved.TryGetValue(item.Id, out var ready))
            {
                return ready;
            }

            if (!permitted(item) || path.Count >= 64 || !path.Add(item.Id))
            {
                return false;
            }

            ready = item.ParentId is not { } parent || (lookup.TryGetValue(parent, out var ancestor) && Visit(ancestor, path));
            path.Remove(item.Id);
            resolved[item.Id] = ready;
            return ready;
        }

        return lookup.Values.Where(item => Visit(item, [])).ToArray();
    }
}
