using System.Collections.Generic;
using Howbot.Core.Entities;

namespace Howbot.Core.Interfaces;

public interface IRepository
{
  T GetById<T>(ulong id) where T : BaseEntity;

  List<T> List<T>() where T : BaseEntity;

  T Add<T>(T entity) where T : BaseEntity;

  void Update<T>(T entity) where T : BaseEntity;

  void Delete<T>(T entity) where T : BaseEntity;
}
