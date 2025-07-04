using Howbot.Domain.Entities;
using Howbot.Domain.Entities.Abstract;
using Howbot.Domain.Entities.Concrete;

namespace Howbot.Application.Interfaces.Infrastructure;

public interface IRepository
{
  T? GetById<T>(Guid id) where T : BaseEntity;

  Guild? GetGuildByGuildId(ulong guildId);

  List<T> List<T>() where T : BaseEntity;

  T Add<T>(T entity) where T : BaseEntity;

  Task<T> AddAsync<T>(T entity) where T : BaseEntity;

  void Update<T>(T entity) where T : BaseEntity;

  Task UpdateAsync<T>(T entity) where T : BaseEntity;

  void Delete<T>(T entity) where T : BaseEntity;
}
