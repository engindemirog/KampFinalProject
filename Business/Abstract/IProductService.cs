using Core.Utilities.Results;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IProductService
    {
        Task<IDataResult<List<Product>>> GetAllAsync();
        Task<IDataResult<List<Product>>> GetAllByCategoryIdAsync(int id);
        Task<IDataResult<List<Product>>> GetByUnitPriceAsync(decimal min, decimal max);
        Task<IDataResult<List<ProductDetailDto>>> GetProductDetailsAsync();
        Task<IDataResult<Product>> GetByIdAsync(int productId);
        Task<IResult> AddAsync(Product product);
        Task<IResult> UpdateAsync(Product product);
                    
        Task<IResult> AddTransactionalTestAsync(Product product);

        //RESTFUL --> HTTP --> 
    }
}
