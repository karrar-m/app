namespace Domain.Interfaces;

public interface IRepository<T>
{
    Task AddAsync(T entity);                   
    Task UpdateAsync(T entity);                 
    Task DeleteAsync(int id);                    
    Task<T> GetByIdAsync(int id);                
    Task<IEnumerable<T>> GetAllAsync();         
}
