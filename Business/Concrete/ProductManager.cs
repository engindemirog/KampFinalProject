using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.CCS;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Validation;
using Core.CrossCuttingConcerns.Caching;
using Core.CrossCuttingConcerns.Validation;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.InMemory;
using Entities.Concrete;
using Entities.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class ProductManager : IProductService
    {
        IProductDal _productDal;
        ICategoryService _categoryService;

        public ProductManager(IProductDal productDal,ICategoryService categoryService)
        {
            _productDal = productDal;
            _categoryService = categoryService;
        }

        //00.25 Dersteyiz
        //Claim
        [SecuredOperation("product.add,admin")]
        [ValidationAspect(typeof(ProductValidator))]
        [CacheRemoveAspect("IProductService.Get")]
        public async Task<IResult> AddAsync(Product product)
        {

            //Aynı isimde ürün eklenemez
            //Eğer mevcut kategori sayısı 15'i geçtiyse sisteme yeni ürün eklenemez. ve 
            IResult result = BusinessRules.Run(await CheckIfProductNameExistsAsync(product.ProductName), 
                await CheckIfProductCountOfCategoryCorrectAsync(product.CategoryId),await CheckIfCategoryLimitExcededAsync());

            if (result != null)
            {
                return result;
            }

            await _productDal.AddAsync(product);

            return new SuccessResult(Messages.ProductAdded);

        }


        [CacheAspect] //key,value
        public async Task<IDataResult<List<Product>>> GetAllAsync()
        {
            if (DateTime.Now.Hour == 1)
            {
                return new ErrorDataResult<List<Product>>(Messages.MaintenanceTime);
            }

            return new SuccessDataResult<List<Product>>(await _productDal.GetAllAsync(), Messages.ProductsListed);
        }

        public async Task<IDataResult<List<Product>>> GetAllByCategoryIdAsync(int id)
        {
            return new SuccessDataResult<List<Product>>(await _productDal.GetAllAsync(p => p.CategoryId == id));
        }

        [CacheAspect]
        //[PerformanceAspect(5)]
        public async Task<IDataResult<Product>> GetByIdAsync(int productId)
        {
            return new SuccessDataResult<Product>(await _productDal.GetAsync(p => p.ProductId == productId));
        }

        public async Task<IDataResult<List<Product>>> GetByUnitPriceAsync(decimal min, decimal max)
        {
            return new SuccessDataResult<List<Product>>(await _productDal.GetAllAsync(p => p.UnitPrice >= min && p.UnitPrice <= max));
        }

        public async Task<IDataResult<List<ProductDetailDto>>> GetProductDetailsAsync()
        {
            if (DateTime.Now.Hour == 23)
            {
                return new ErrorDataResult<List<ProductDetailDto>>(Messages.MaintenanceTime);
            }
            return new SuccessDataResult<List<ProductDetailDto>>(await _productDal.GetProductDetailsAsync());
        }

        [ValidationAspect(typeof(ProductValidator))]
        [CacheRemoveAspect("IProductService.Get")]
        public async Task<IResult> UpdateAsync(Product product)
        {
            var result =await _productDal.GetAllAsync(p => p.CategoryId == product.CategoryId);
            if (result.Count >= 10)
            {
                return new ErrorResult(Messages.ProductCountOfCategoryError);
            }
            throw new NotImplementedException();
        }

        private async Task<IResult> CheckIfProductCountOfCategoryCorrectAsync(int categoryId)
        {
            //Select count(*) from products where categoryId=1
            var result = await _productDal.GetAllAsync(p => p.CategoryId == categoryId);
            if (result.Count >= 15)
            {
                return new ErrorResult(Messages.ProductCountOfCategoryError);
            }
            return new SuccessResult();
        }

        private async Task<Result> CheckIfProductNameExistsAsync(string productName)
        {
            var result = await _productDal.GetAllAsync(p => p.ProductName == productName);
            if (result.Any())
            {
                return new ErrorResult(Messages.ProductNameAlreadyExists);
            }
            return new SuccessResult();
        }

        private async Task<IResult> CheckIfCategoryLimitExcededAsync()
        {
            var result =await _categoryService.GetAllAsync();
            if (result.Data.Count>15)
            {
                return new ErrorResult(Messages.CategoryLimitExceded);
            }

            return new SuccessResult();
        }

        //[TransactionScopeAspect]
        public async Task<IResult> AddTransactionalTestAsync(Product product)
        {

            await AddAsync(product);
            if (product.UnitPrice < 10)
            {
                    throw new Exception("");
            }
            
            await AddAsync(product);

            return null;
        }
    }
}
