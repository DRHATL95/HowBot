using System.Collections.Generic;
using System.Threading.Tasks;
using Howbot.Core.Entities;

namespace Howbot.Core.Interfaces;

// Purpose: Interface for the Repository
public interface IRepository
{
  T? GetById<T>(ulong id) where T : BaseEntity;

  List<T> List<T>() where T : BaseEntity;

  T Add<T>(T entity) where T : BaseEntity;

  Task<T> AddAsync<T>(T entity) where T : BaseEntity;

  void Update<T>(T entity) where T : BaseEntity;

  Task UpdateAsync<T>(T entity) where T : BaseEntity;

  void Delete<T>(T entity) where T : BaseEntity;
}
