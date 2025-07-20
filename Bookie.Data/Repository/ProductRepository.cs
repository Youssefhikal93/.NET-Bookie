using Bookie.DataAccess.Data;
using Bookie.DataAccess.Repository.IRepository;
using Bookie.Models;
using Bookie.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bookie.DataAccess.Repository
{
    class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
      
        public void update(Product product)
        {
            //updating all the fields
            //_db.Products.Update(product);

            var existingProduct = _db.Products.FirstOrDefault(p => p.Id == product.Id);
            if(existingProduct!= null)
            {
                existingProduct.Title = product.Title;
                existingProduct.Description = product.Description;
                existingProduct.Author = product.Author;
                existingProduct.ISBN = product.ISBN;
                existingProduct.Price = product.Price;
                existingProduct.Price50 = product.Price50;
                existingProduct.Price100 = product.Price100;
                existingProduct.ListPrice = product.ListPrice;
                existingProduct.CategoryId = product.CategoryId;

                if(existingProduct.ImageUrl != null)
                {
                    existingProduct.ImageUrl = product.ImageUrl;
                }

            }
        }
    }
}
