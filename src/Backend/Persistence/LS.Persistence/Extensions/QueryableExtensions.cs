using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Persistence.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> InDisplayOrder<T>(this IQueryable<T> query) where T : class, IOrderable 
        => query.OrderBy(x => x.DisplayOrder);
        
        
        
}
