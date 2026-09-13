using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Domain.Features.IAM.Users.Contracts.Repositories;


public interface IUserRepository : IRepository<AppUser>
{

}
