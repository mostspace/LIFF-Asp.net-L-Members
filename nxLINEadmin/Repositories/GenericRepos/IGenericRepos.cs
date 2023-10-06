using System.Linq.Expressions;

namespace nxLINEadmin.Repositories.GenericRepos
{
    public interface IGenericRepos<T> where T : class
    {
        #region Syn Func
        T GetById(int id);
        IQueryable<T> GetAll();
        IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression);
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        T Update(T entity);
        #endregion
    }
}
