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
            return entity != null ? MapToDto(entity) : null;
        }
        public virtual async Task<IEnumerable<TDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return MapToDtoList(entities);
        }
        public virtual async Task<IEnumerable<TDto>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            var entities = await _repository.FindAsync(predicate);
            return MapToDtoList(entities);
        }
        public virtual async Task<TDto> CreateAsync(TCreateDto createDto)
        {
            await ValidateBeforeCreateAsync(createDto);
            var entity = MapToEntity(createDto);
            var createdEntity = await _repository.AddAsync(entity);
            return MapToDto(createdEntity);
        }
        public virtual async Task<TDto> UpdateAsync(int id, TUpdateDto updateDto)
        {
            var existingEntity = await _repository.GetByIdAsync(id);
            if (existingEntity == null)
                throw new KeyNotFoundException($"Entity with ID {id} not found");

            await ValidateBeforeUpdateAsync(id, updateDto);
            UpdateEntity(existingEntity, updateDto);
            var updatedEntity = await _repository.UpdateAsync(existingEntity);
            return MapToDto(updatedEntity);
        }
        public virtual async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return false;

            await ValidateBeforeDeleteAsync(id);
            var result = await _repository.DeleteAsync(entity);
            return result;
        }
        public virtual async Task<bool> ExistsAsync(int id)
        {
            return await _repository.ExistsAsync(e => (int)typeof(T).GetProperty("Id")!.GetValue(e)! == id);
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
            var dtos = MapToDtoList(pagedResult.Items);

            return new PagedResult<TDto>
            {
                Items = dtos,
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }

        protected abstract TDto MapToDto(T entity);
        protected abstract IEnumerable<TDto> MapToDtoList(IEnumerable<T> entities);
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
