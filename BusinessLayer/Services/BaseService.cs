using BusinessLayer.DTOs;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using System.Linq.Expressions;

namespace BusinessLayer.Services
{
    public abstract class BaseService<T, TDto, TCreateDto, TUpdateDto> : IBaseService<T, TDto, TCreateDto, TUpdateDto>
        where T : class
        where TDto : class
        where TCreateDto : class
        where TUpdateDto : class
    {
        protected readonly IBaseRepository<T> _repository;

        protected BaseService(IBaseRepository<T> repository)
        {
            _repository = repository;
        }

        public virtual async Task<TDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity != null ? await MapToDto(entity) : null;
        }
        public virtual async Task<IEnumerable<TDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return await MapToDtoList(entities);
        }
        public virtual async Task<IEnumerable<TDto>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            var entities = await _repository.FindAsync(predicate);
            return await MapToDtoList(entities);
        }
        public virtual async Task<TDto> CreateAsync(TCreateDto createDto)
        {
            await ValidateBeforeCreateAsync(createDto);
            var entity = MapToEntity(createDto);
            var createdEntity = await _repository.AddAsync(entity);
            return await MapToDto(createdEntity);
        }
        public virtual async Task<TDto> UpdateAsync(int id, TUpdateDto updateDto)
        {
            var existingEntity = await _repository.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Entity with ID {id} not found");
            await ValidateBeforeUpdateAsync(id, updateDto);
            UpdateEntity(existingEntity, updateDto);
            var updatedEntity = await _repository.UpdateAsync(existingEntity);
            return await MapToDto(updatedEntity);
        }
        public virtual async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null) return false;

            await ValidateBeforeDeleteAsync(id);

            return await _repository.DeleteAsync(entity);
        }
        public virtual async Task<bool> ExistsAsync(int id)
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, "Id");
            var constant = Expression.Constant(id);

            var body = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
            return await _repository.ExistsAsync(lambda);
        }
        public virtual async Task<int> CountAsync()
        {
            return await _repository.CountAsync();
        }
        public virtual async Task<PagedResult<TDto>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Expression<Func<T, object>>? orderBy = null,
            bool orderByDescending = false)
        {
            var pagedResult = await _repository.GetPagedAsync(pageNumber, pageSize, predicate, orderBy, orderByDescending);

            return new PagedResult<TDto>
            {
                Items = await MapToDtoList(pagedResult.Items),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }

        protected abstract Task<TDto> MapToDto(T entity);
        protected abstract Task<IEnumerable<TDto>> MapToDtoList(IEnumerable<T> entities);
        protected abstract T MapToEntity(TCreateDto createDto);
        protected abstract void UpdateEntity(T entity, TUpdateDto updateDto);

        protected virtual Task ValidateBeforeCreateAsync(TCreateDto createDto)
        {
            return Task.CompletedTask;
        }
        protected virtual Task ValidateBeforeUpdateAsync(int id, TUpdateDto updateDto)
        {
            return Task.CompletedTask;
        }
        protected virtual Task ValidateBeforeDeleteAsync(int id)
        {
            return Task.CompletedTask;
        }
    }
}
