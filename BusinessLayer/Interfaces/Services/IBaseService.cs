using BusinessLayer.DTOs;
using System.Linq.Expressions;

namespace BusinessLayer.Interfaces.Services
{
    public interface IBaseService<T, TDto, TCreateDto, TUpdateDto>
        where T : class
        where TDto : class
        where TCreateDto : class
        where TUpdateDto : class
    {
        Task<TDto?> GetByIdAsync(int id);
        Task<IEnumerable<TDto>> GetAllAsync();
        Task<IEnumerable<TDto>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<TDto> CreateAsync(TCreateDto createDto);
        Task<TDto> UpdateAsync(int id, TUpdateDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync();
        Task<PagedResult<TDto>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Expression<Func<T, object>>? orderBy = null,
            bool orderByDescending = false);
    }
}
