namespace Titeenipeli.Services.Interfaces;

public interface IEntityService<T>
{
    public T? GetById(int id);
    public List<T> GetAll();
    public void Add(T entity);
}