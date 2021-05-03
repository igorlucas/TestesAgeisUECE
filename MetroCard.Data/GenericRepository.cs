using System.Collections.Generic;
using System.Linq;

namespace MetroCard.Data
{
    public class GenericRepository<T> where T : class
    {
        private readonly DataContext _db;

        public GenericRepository(DataContext db) => _db = db;

        public void Create(T entity) => _db.Add(entity);

        public T Read(int id) => _db.Set<T>().Find(id);

        public IList<T> Read() => _db.Set<T>().ToList();

        public void Update(T entity) => _db.Update(entity);

        public void Delete(T entity) => _db.Set<T>().Remove(entity);

        public void Commit() => _db.SaveChanges();
    }
}
