﻿using System.Linq;

namespace StackWarden.Core.Persistence
{
    public interface IRepository
    {
        void Save<T>(T entity);
        IQueryable<T> Query<T>();
    }
}