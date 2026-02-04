using System.Linq.Expressions;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Specifications;
using MedLink.Domain.Common;
using MedLink.Infrastructure.Persistence.Context;
using MedLink.Infrastructure.Persistence.Specifications;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {

        private readonly ApplicationDbContext _dbContext;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }



        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec)
        {
            return await ApplySpecifications(spec).ToListAsync();
        }


        public async Task<T?> GetEntityWithAsync(ISpecification<T> spec)
        {
            return await ApplySpecifications(spec).FirstOrDefaultAsync();

        }

    


        public void Delete(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
        }


        public void Update(T entity)
        {
            _dbContext.Set<T>().Update(entity);
        }

        public async Task AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
        }

        public async Task<int> GetCountAsync(ISpecification<T> spec)
        {
            return await ApplySpecifications(spec).CountAsync();
        }


        private IQueryable<T> ApplySpecifications(ISpecification<T> spec)
        {
            return SpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>(), spec);
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>().AnyAsync(predicate);
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>()
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>()
                .FirstOrDefaultAsync(predicate);
        }
    }
}
