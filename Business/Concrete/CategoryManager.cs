using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CategoryManager : ICategoryService
    {
        ICategoryDal _categoryDal;

        public CategoryManager(ICategoryDal categoryDal)
        {
            _categoryDal = categoryDal;
        }

        public async Task<IDataResult<List<Category>>> GetAllAsync()
        {
            //İş kodları
            return new SuccessDataResult<List<Category>>(await _categoryDal.GetAllAsync());
        }

        //Select * from Categories where CategoryId = 3
        public async Task<IDataResult<Category>> GetByIdAsync(int categoryId)
        {
            return new SuccessDataResult<Category>(await _categoryDal.GetAsync(c=>c.CategoryId == categoryId));
        }
    }
}
