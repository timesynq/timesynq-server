namespace TimesynqServer.Persistence.UnitOfWork
{
    public class EfUnitOfWork(TimesynqDbContext dbContext) : IUnitOfWork
    {
        private TimesynqDbContext _dbContext => dbContext;

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
