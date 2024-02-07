using System.Collections.Generic;
using System.Threading.Tasks;
using Howbot.Core.Entities;

namespace Howbot.Core.Interfaces;

// Purpose: Interface for the Repository
public interface IRepository
{
  /// <summary>
  ///   Called to get EF entity by id
  /// </summary>
  /// <param name="id"></param>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  T GetById<T>(ulong id) where T : BaseEntity;

  /// <summary>
  ///   Called to get a list of EF entities
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  List<T> List<T>() where T : BaseEntity;

  /// <summary>
  ///   Adds an EF entity
  /// </summary>
  /// <param name="entity"></param>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  T Add<T>(T entity) where T : BaseEntity;

  Task<T> AddAsync<T>(T entity) where T : BaseEntity;

  /// <summary>
  ///   Updates an EF entity
  /// </summary>
  /// <param name="entity"></param>
  /// <typeparam name="T"></typeparam>
  void Update<T>(T entity) where T : BaseEntity;

  Task UpdateAsync<T>(T entity) where T : BaseEntity;

  /// <summary>
  ///   Deletes an EF entity
  /// </summary>
  /// <param name="entity"></param>
  /// <typeparam name="T"></typeparam>
  void Delete<T>(T entity) where T : BaseEntity;
}
