namespace Titeenipeli.Database.Services.Interfaces;

public interface IEntityRepositoryService<T>
{
    public T? GetById(int id);
    public List<T> GetAll();
    public void Add(T entity);
}