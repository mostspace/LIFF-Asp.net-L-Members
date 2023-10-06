using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using nxLINEadminAPI.Entity;
using nxLINEadminAPI.Repositories.GenericRepos;

namespace nxLINEadminAPI.Repositories.GenericRepos
{
    public class GenericRepos<T> : IGenericRepos<T> where T : class
    {
        private readonly nxLINEadminAPIContext _adminContext;

        public GenericRepos(nxLINEadminAPIContext adminContext)
        {
            _adminContext = adminContext;
        }

        public virtual void Add(T entity)
        {
            _adminContext.Set<T>().Add(entity);
        }

        public virtual void AddRange(IEnumerable<T> entities)
        {
            _adminContext.Set<T>().AddRange(entities);
        }

        public virtual IQueryable<T> GetAll()
        {
            return _adminContext.Set<T>().AsQueryable();
        }

        public virtual IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression)
        {
            if (expression != null)
                return _adminContext.Set<T>().Where(expression);
            return _adminContext.Set<T>();
        }

        public virtual T GetById(int id)
        {
            return _adminContext.Set<T>().Find(id);
        }

        public virtual void Remove(T entity)
        {
            _adminContext.Set<T>().Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            _adminContext.Set<T>().RemoveRange(entities);
        }

        public T Update(T entity)
        {
            return _adminContext.Set<T>().Update(entity).Entity;
        }
    }
}
