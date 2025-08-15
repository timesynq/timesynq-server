namespace TimesynqServer.Persistence.UnitOfWork
{
    public interface IUnitOfWork
    {
        public Task SaveChangesAsync();
    }
}
