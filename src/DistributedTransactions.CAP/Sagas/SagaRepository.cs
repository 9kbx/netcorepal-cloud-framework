using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Repository.EntityframeworkCore;

namespace NetCorePal.Extensions.DistributedTransactions.Sagas;

public class SagaRepository<TDbContext> : RepositoryBase<SagaEntity, Guid, TDbContext>
    where TDbContext : EFContext
{
    public SagaRepository(TDbContext dbContext) : base(dbContext)
    {
    }


    public async Task<SagaEntity?> NoCacheGetAsync(Guid sagaId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<SagaEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == sagaId, cancellationToken);
    }
}