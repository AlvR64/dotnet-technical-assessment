using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Interfaces;

namespace GtMotive.Estimate.Microservice.Infrastructure.Persistence
{
    public sealed class UnitOfWork(RentingDbContext context) : IUnitOfWork
    {
        public Task<int> Save() => context.SaveChangesAsync();
    }
}
