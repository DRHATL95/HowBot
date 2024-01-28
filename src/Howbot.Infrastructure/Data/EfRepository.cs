﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Howbot.Core.Entities;
using Howbot.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Howbot.Infrastructure.Data;

/// <summary>
///   A simple repository implementation for EF Core
///   If you don't want changes to be saved immediately, add a SaveChanges method to the interface
///   and remove the calls to _dbContext.SaveChanges from the Add/Update/Delete methods
/// </summary>
public class EfRepository(AppDbContext dbContext) : IRepository
{
  public T GetById<T>(ulong id) where T : BaseEntity
  {
    return dbContext.Set<T>().SingleOrDefault(e => e.Id == id);
  }

  public List<T> List<T>() where T : BaseEntity
  {
    return dbContext.Set<T>().ToList();
  }
  
  public async Task<T> AddAsync<T>(T entity) where T : BaseEntity
  {
    dbContext.Set<T>().Add(entity);
    
    await dbContext.SaveChangesAsync()
      .ConfigureAwait(false);

    return entity;
  }

  public T Add<T>(T entity) where T : BaseEntity
  {
    dbContext.Set<T>().Add(entity);
    
    dbContext.SaveChanges();

    return entity;
  }

  public void Delete<T>(T entity) where T : BaseEntity
  {
    dbContext.Set<T>().Remove(entity);
    
    dbContext.SaveChanges();
  }

  public void Update<T>(T entity) where T : BaseEntity
  {
    dbContext.Entry(entity).State = EntityState.Modified;
    
    dbContext.SaveChanges();
  }
  
  public async Task UpdateAsync<T>(T entity) where T : BaseEntity
  {
    dbContext.Set<T>().Attach(entity);
    
    dbContext.Entry(entity).State = EntityState.Modified;
    
    await dbContext.SaveChangesAsync()
      .ConfigureAwait(false);
  }
}
