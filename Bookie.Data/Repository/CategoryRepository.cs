﻿using Bookie.DataAccess.Data;
using Bookie.DataAccess.Repository.IRepository;
using Bookie.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bookie.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category> , ICategoryRepository

    {
        private  ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
        _db = db;
        }
   
        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Category obj)
        {
           _db.Categories.Update(obj);
        }
    }
}
