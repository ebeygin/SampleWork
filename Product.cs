using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using MVCMobileDetect.Geonames;
using MVCMobileDetect.DBML.GC;

namespace MVCMobileDetect.Product.GC
{
    public class Product : MVCMobileDetect.Interfaces.Product.IProduct
    {
        #region Fields
        private GCDataContext _dataContext = null;
        private static readonly ILog _Logger;
        #endregion

        #region Construction

        static Product()
        {
            _Logger = LogManager.GetLogger(typeof(Product));
        }

        public Product()
        {
            _dataContext = new GCDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["HDGardenClubSpanishDevConnectionString"].ConnectionString);
            
        }
        #endregion

        #region Public Methods

        public IEnumerable<Department> GetDepartments()
        {
            using (DataContext)
            {
                var dept = from b in DataContext.GCProductDepartments
                           select new MVCMobileDetect.Product.Department
                           {
                               DeptID = b.DepartmentId,
                               DeptName = b.Description.Trim()
                           };

                return dept.OrderBy(e => e.DeptName).ToList();
            }
        }

        public IEnumerable<Category> GetCategories(int deptID)
        {
            var cat = from b in DataContext.GCProductClasses
                      where b.Department == deptID
                      select new MVCMobileDetect.Product.Category
                      {
                          DeptID = b.GCProductDepartment.DepartmentId,
                          DeptName = b.GCProductDepartment.Description.Trim(),
                          CategoryID = b.Class,
                          CategoryName = b.Description
                      };

            return cat.OrderBy(e => e.CategoryName).ToList();
        }

        public IEnumerable<SubCategory> GetSubCategories(int deptID, int catID)
        {
            var subCat = from b in DataContext.GCProductSubClasses
                         where b.Department == deptID && b.Class == catID

                         select new MVCMobileDetect.Product.SubCategory
                         {
                             DeptID = b.GCProductClass.GCProductDepartment.DepartmentId,
                             DeptName = b.GCProductClass.GCProductDepartment.Description.Trim(),

                             CategoryID = b.GCProductClass.Class,
                             CategoryName = b.GCProductClass.Description.Trim(),

                             SubCategoryID = b.SubClass,
                             SubCategoryName = b.Description.Trim(),

                         };

            return subCat.OrderBy(e => e.SubCategoryName).ToList();
        }

        public IEnumerable<SubCategoryProduct> GetSubCategoryProduct(int deptID, int catID, int subCatID)
        {
            var product = from b in DataContext.GCProducts
                          where b.DepartmentId == deptID && b.ClassId == catID && b.SubClassId == subCatID

                          select new MVCMobileDetect.Product.SubCategoryProduct
                          {
                              DeptID = b.GCProductSubClass.GCProductClass.GCProductDepartment.DepartmentId,
                              DeptName = b.GCProductSubClass.GCProductClass.GCProductDepartment.Description.Trim(),

                              CategoryID = b.GCProductSubClass.GCProductClass.Class,
                              CategoryName = b.GCProductSubClass.GCProductClass.Description.Trim(),

                              SubCategoryID = b.GCProductSubClass.SubClass,
                              SubCategoryName = b.GCProductSubClass.Description.Trim(),

                              ProductID = b.ProductId,
                              ShortDescription = b.ShortDescription.Trim()

                          };

            return product.OrderBy(e => e.SubCategoryName).ToList();
        }

        public IEnumerable<CategoryProducts> GetCategories(string zipCode)
        {
            var product = from i in
                           (from prod in DataContext.GCProducts
                            join productClass in DataContext.GCProductClasses on prod.ClassId equals productClass.Class
                            join marketPrice in DataContext.GCMarketPrices on prod.ProductId equals marketPrice.ProductId
                            join marketZip in DataContext.GCMarketZips on marketPrice.MarketId equals marketZip.MarketId
                            where productClass.Department == prod.DepartmentId && marketZip.ZipCode == zipCode && prod.IsActive == true 
                            select new {prod.ClassId, productClass.Description, prod.DepartmentId})
                            group i by new {i.ClassId, i.Description, i} into g
                          select new CategoryProducts { CategoryID = g.Key.ClassId, CategoryName = g.Key.Description.Trim(), ProductsCount = g.Select(x => x.ClassId).Count() };

            
            /*
            var product = from b in DataContext.GCProducts
                       group b by b.ClassId
                           into grp
                           select new
                           {
                               CategoryID = grp.Key, 
                               //CategoryName = grp.Key
                               Count1 = grp.Select(x => x.ClassId).Count(),
                               Count2 = grp.Count()
                           };
            var tmp2 = product.ToList();
            var ddd = product.GroupBy(a => a.CategoryID).ToList();
            */
            return product.OrderBy(e => e.CategoryName).ToList();
        }

        public IEnumerable<ProductsCategory> GetProductsByCategory(int catID, string zipCode, SortBy orderBy, bool isAsc)
        {

#if DEBUG
            MVCMobileDetect.Common.LogHelper lh = new MVCMobileDetect.Common.LogHelper();
#endif
            IEnumerable<ProductsCategory> sortedProducts=null;
            IQueryable<ProductsCategory> products = null;
            if (SortBy.TopSeller == orderBy)
            {
                products = from product in DataContext.GCProducts
                           join marketPrice in DataContext.GCMarketPrices on product.ProductId equals marketPrice.ProductId
                           join marketZip in DataContext.GCMarketZips on marketPrice.MarketId equals marketZip.MarketId
                           join topSeller in DataContext.GCProductTop50s on product.SKU equals topSeller.SKU
                           where (marketZip.ZipCode == zipCode && product.IsActive == true && product.ClassId == catID
                                && topSeller.MarketId == marketZip.MarketId && topSeller.ClassId == catID
                                && product.Brand != null && product.ShortDescription != null)

                           select new ProductsCategory
                           {
                               ProductID = product.ProductId,
                               Brand = (product.Brand == "NULL" ? null : product.Brand.Trim()),
                               Name = product.ShortDescription.Trim(),
                               RegularPrice = marketPrice.MarketPrice,
                               SalePrice = marketPrice.MarketPrice-1,
                               ThumbnailURL = product.SmallImage,
                               Ranking = topSeller.Ranking,
                               Rating = GetRandom(product.ProductId, 1, 5),
                               RatingCount = GetRandom(product.ProductId, 1, 100)
                           };

                sortedProducts = (isAsc == true ? products.ToList().OrderBy(a => a.Ranking) : products.ToList().OrderByDescending(a => a.Ranking));
            }
            else
            {
                products = from marketPrice in DataContext.GCMarketPrices
                           join product in DataContext.GCProducts on marketPrice.ProductId equals product.ProductId
                           join marketZip in DataContext.GCMarketZips on marketPrice.MarketId equals marketZip.MarketId
                           where (marketZip.ZipCode == zipCode && product.IsActive == true && product.ClassId == catID
                                    && product.Brand != null && product.ShortDescription != null)

                           select new ProductsCategory
                           {
                               ProductID = product.ProductId,
                               Brand = (product.Brand == "NULL" ? null : product.Brand.Trim()),
                               Name = product.ShortDescription.Trim(),
                               RegularPrice = marketPrice.MarketPrice,
                               SalePrice = marketPrice.MarketPrice - 1,
                               ThumbnailURL = product.SmallImage,
                               Rating = new Random(product.ProductId + (int)DateTime.Now.Ticks).Next(1, 5),
                               RatingCount =  new Random(product.ProductId + (int)DateTime.Now.Ticks).Next(1, 100),
                               DateCreated = product.CreateDate.Value.ToShortDateString()
                           };

                switch (orderBy)
                {
                    case SortBy.Product:
                        sortedProducts = (isAsc == true ? products.ToList().OrderBy(a => a.Name) : products.ToList().OrderByDescending(a => a.Name));
                        break;

                    case SortBy.Brand:
                        sortedProducts = (isAsc == true ? products.ToList().OrderBy(a => a.Brand) : products.ToList().OrderByDescending(a => a.Brand));
                        break;

                    case SortBy.Price:
                        sortedProducts = (isAsc == true ? products.ToList().OrderBy(a => a.RegularPrice) : products.ToList().OrderByDescending(a => a.RegularPrice));
                        break;

                    case SortBy.Newest:
                        sortedProducts = (isAsc == true ? products.ToList().OrderBy(a => a.DateCreated) : products.ToList().OrderByDescending(a => a.DateCreated));
                        break;

                    case SortBy.Rated:
                        sortedProducts = (isAsc == true ? products.ToList().OrderBy(a => a.RatingCount) : products.ToList().OrderByDescending(a => a.RatingCount));
                        break;

                    default:
                        sortedProducts = products.ToList().OrderBy(a => a.Name);
                        break;

                }
            }

#if DEBUG
            lh.StopWrite(_Logger, "Product.GetProductsByCategory took>>>>> {0}.");
#endif
            return sortedProducts;
            //return products.ToList();
        }

        public IEnumerable<ProductDetails> GetProductDetails(string zipCode, string productID)
        {
            int[] productIds = productID.Split(',').ToArray().Select(x => int.Parse(x)).ToArray();
            var products = from marketPrice in DataContext.GCMarketPrices
                           join product in DataContext.GCProducts on marketPrice.ProductId equals product.ProductId
                           join marketZip in DataContext.GCMarketZips on marketPrice.MarketId equals marketZip.MarketId
                           where (productIds.Contains(product.ProductId) && marketZip.ZipCode == zipCode && product.IsActive == true)
                           orderby product.ProductId ascending
                           select new ProductDetails
                           {
                               ProductID = product.ProductId.ToString(),
                               Brand = (product.Brand == "NULL" ? null : product.Brand.Trim()),
                               Name = product.ShortDescription.Trim(),
                               RegularPrice = String.Format("{0:0.00}", marketPrice.MarketPrice),
                               SalePrice  = String.Format("{0:0.00}", (marketPrice.MarketPrice*0.80)),
                               ThumbnailURL = product.SmallImage,
                               Rating = GetRandom(product.ProductId, 1, 5),
                               RatingCount = GetRandom(product.ProductId,1,100),
                               Description = "World's Fair Gold Medal 4 oz. SUPERthrive gives easier plant success worldwide. Used by countless government, university, commercial, and residential growers. Guaranteed best, unchallenged since 1940. Its non-toxic, ready-made vitamins and plant hormones normalize plants. Add to fertilizer program. It stretches....",
                               Offer = string.Format("${0} each for 2 or more", String.Format("{0:0.00}", marketPrice.MarketPrice*0.75)),//"$18.99 if purchase 2 or more",
                               SKU = product.SKU,
                               ProductURL = product.PIP,
                               RelatedArticle = GetRelatedArticle(product.ClassId)//product.ClassId.ToString()
                           };

            return products.ToList();
        }

        public IEnumerable<ProductDetails> Search(string zipCode, string searchText, SortBy orderBy, bool isAsc)
        {

            if (!Enum.IsDefined(typeof(MVCMobileDetect.Product.SortBy), orderBy))
                throw new MVCMobileDetect.Exceptions.MDException(string.Format("SortBy not found for the Product:: Enum value:{0} is not defined in MVCMobileDetect.Product.SortBy", orderBy));
#if DEBUG
            MVCMobileDetect.Common.LogHelper lh = new MVCMobileDetect.Common.LogHelper();
#endif
            IEnumerable<ProductDetails> sortedProducts = null;
            IQueryable<ProductDetails> products = null;
            if (SortBy.TopSeller == orderBy)
            {
                if (isAsc)
                {
                    products = (from productClass in DataContext.GCProductClasses
                                join product in DataContext.GCProducts on productClass.Class equals product.ClassId
                                join marketPrice in DataContext.GCMarketPrices on product.ProductId equals marketPrice.ProductId
                                join marketZip in DataContext.GCMarketZips on marketPrice.MarketId equals marketZip.MarketId
                                join topSeller in DataContext.GCProductTop50s on product.SKU equals topSeller.SKU

                                where
                                  ((marketZip.ZipCode == zipCode && product.IsActive == true && topSeller.MarketId == marketZip.MarketId && topSeller.ClassId == product.ClassId) &&
                                  (productClass.Description.Contains(searchText) ||
                                  product.Brand.Contains(searchText) ||
                                  product.ShortDescription.Contains(searchText) ||
                                  product.Keywords.Contains(searchText) ||
                                  product.Category.Contains(searchText) ||
                                  product.SKU.Contains(searchText))
                                  && product.Brand != null && product.ShortDescription != null)

                                orderby topSeller.Ranking ascending 
                                select new MVCMobileDetect.Product.ProductDetails
                                {
                                    ProductID = product.ProductId.ToString(),
                                    Brand = (product.Brand == "NULL" ? null : product.Brand.Trim()),
                                    Name = product.ShortDescription.Trim(),
                                    RegularPrice = String.Format("{0:0.00}", marketPrice.MarketPrice),
                                    ThumbnailURL = product.SmallImage,
                                    Rating = GetRandom(product.ProductId, 1, 5),
                                    RatingCount = GetRandom(product.ProductId, 1, 100),
                                    Offer = string.Format("${0} each for 2 or more", String.Format("{0:0.00}", marketPrice.MarketPrice*0.75)),
                                    SKU = product.SKU,
                                    DateUpdated = (DateTime)product.UpdateDate
                                }).Distinct();
                }
                else
                {
                    products = (from productClass in DataContext.GCProductClasses
                                join product in DataContext.GCProducts on productClass.Class equals product.ClassId
                                join marketPrice in DataContext.GCMarketPrices on product.ProductId equals marketPrice.ProductId
                                join marketZip in DataContext.GCMarketZips on marketPrice.MarketId equals marketZip.MarketId
                                join topSeller in DataContext.GCProductTop50s on product.SKU equals topSeller.SKU

                                where
                                  ((marketZip.ZipCode == zipCode && product.IsActive == true && topSeller.MarketId == marketZip.MarketId && topSeller.ClassId == product.ClassId) &&
                                  (productClass.Description.Contains(searchText) ||
                                  product.Brand.Contains(searchText) ||
                                  product.ShortDescription.Contains(searchText) ||
                                  product.Keywords.Contains(searchText) ||
                                  product.Category.Contains(searchText) ||
                                  product.SKU.Contains(searchText))
                                  && product.Brand != null && product.ShortDescription != null)

                                orderby topSeller.Ranking descending 
                                select new MVCMobileDetect.Product.ProductDetails
                                {
                                    ProductID = product.ProductId.ToString(),
                                    Brand = (product.Brand == "NULL" ? null : product.Brand.Trim()),
                                    Name = product.ShortDescription.Trim(),
                                    RegularPrice = String.Format("{0:0.00}", marketPrice.MarketPrice),
                                    ThumbnailURL = product.SmallImage,
                                    Rating = GetRandom(product.ProductId, 1, 5),
                                    RatingCount = GetRandom(product.ProductId, 1, 100),
                                    Offer = string.Format("${0} each for 2 or more", String.Format("{0:0.00}", marketPrice.MarketPrice*0.75)),
                                    SKU = product.SKU,
                                    DateUpdated = (DateTime)product.UpdateDate
                                }).Distinct();
                }
            }
            else
            {
                products = (from productClass in DataContext.GCProductClasses
                            join product in DataContext.GCProducts on productClass.Class equals product.ClassId
                            join marketPrice in DataContext.GCMarketPrices on product.ProductId equals marketPrice.ProductId
                            join marketZip in DataContext.GCMarketZips on marketPrice.MarketId equals marketZip.MarketId

                            where
                              ((marketZip.ZipCode == zipCode && product.IsActive == true) &&
                              (productClass.Description.Contains(searchText) ||
                              product.Brand.Contains(searchText) ||
                              product.ShortDescription.Contains(searchText) ||
                              product.Keywords.Contains(searchText) ||
                              product.Category.Contains(searchText) ||
                              product.SKU.Contains(searchText))
                              && product.Brand != null && product.ShortDescription != null)
                            select new MVCMobileDetect.Product.ProductDetails
                            {
                                ProductID = product.ProductId.ToString(),
                                Brand = (product.Brand == "NULL" ? null : product.Brand.Trim()),
                                Name = product.ShortDescription.Trim(),
                                RegularPrice = String.Format("{0:0.00}", marketPrice.MarketPrice),
                                ThumbnailURL = product.SmallImage,
                                Rating = GetRandom(product.ProductId, 1, 5),
                                RatingCount = GetRandom(product.ProductId, 1, 100),
                                Offer = string.Format("${0} each for 2 or more", String.Format("{0:0.00}", marketPrice.MarketPrice*0.75)),
                                SKU = product.SKU,
                                DateUpdated = (DateTime)product.UpdateDate
                            }).Distinct();
            }

            switch (orderBy)
            {
                case SortBy.Product:
                    sortedProducts = (isAsc == true ? products.ToList().OrderBy(a => a.Name) : products.ToList().OrderByDescending(a => a.Name));
                    break;

                case SortBy.Brand:
                    sortedProducts = (isAsc == true ? products.ToList().OrderBy(a => a.Brand) : products.ToList().OrderByDescending(a => a.Brand));
                    break;

                case SortBy.Price:
                    sortedProducts = (isAsc == true ? products.ToList().OrderBy(a => decimal.Parse(a.RegularPrice)) : products.ToList().OrderByDescending(a => decimal.Parse(a.RegularPrice)));
                    break;

                case SortBy.Rated:
                    sortedProducts = (isAsc == true ? products.ToList().OrderBy(a => a.RatingCount) : products.ToList().OrderByDescending(a => a.RatingCount));
                    break;

                case SortBy.Newest:
                    sortedProducts = (isAsc == true ? products.ToList().OrderBy(a => a.DateUpdated) : products.ToList().OrderByDescending(a => a.DateUpdated));
                    break;

                default:
                    sortedProducts = products.ToList().OrderBy(a => a.Name);
                    break;
            }
#if DEBUG
            lh.StopWrite(_Logger, "Product.Search took>>>>> {0}.");
#endif
            return sortedProducts;
        }

        #endregion

        #region Private Methods
        private int GetRandom(int seed, int min, int max)
        {
            return new Random(seed + (int)DateTime.Now.Ticks).Next(min, max);
        }

        private List<ProductRelatedArticles> GetRelatedArticle(int categoryID)
        {
            List<ProductRelatedArticles> relatedArticles = new List<ProductRelatedArticles>();

            switch (categoryID)
            {
                #region BBQ GRILL, PATIO FURNITURE
                case 26: //BBQ GRILL
                case 22: //PATIO FURNITURE
                    relatedArticles.Add(new ProductRelatedArticles
                    {
                        ID = "11",
                        Title = "Hosting the Perfect BBQ Party",
                        Type = 3,
                        Dimension = 1
                    }
                    );
                    break;
                #endregion

                #region FERTILIZERS
                case 2:
                    relatedArticles.Add(new ProductRelatedArticles
                    {
                        ID = "13",
                        Title = "Patch and Repair Lawn Damage",
                        Type = 1,
                        Dimension = 1
                    }
                        );
                    relatedArticles.Add(new ProductRelatedArticles
                    {
                        ID = "12",
                        Title = "How to Fertilize Your Garden",
                        Type = 2,
                        Dimension = 2
                    }
                    );
                    break;
                #endregion

                #region GOURMET
                case 13:
                    relatedArticles.Add(new ProductRelatedArticles
                    {
                        ID = "11",
                        Title = "Hosting the Perfect BBQ Party",
                        Type = 3,
                        Dimension = 1
                    }
                    );
                    break;
                #endregion

                #region LANDSCAPE
                case 3:
                    relatedArticles.Add(new ProductRelatedArticles
                    {
                        ID = "13",
                        Title = "Patch and Repair Lawn Damage",
                        Type = 1,
                        Dimension = 1
                    }
                    );
                    relatedArticles.Add(new ProductRelatedArticles
                    {
                        ID = "8",
                        Title = "Lawn Sprinklers",
                        Type = 3,
                        Dimension = 2
                    }
                    );
                    relatedArticles.Add(new ProductRelatedArticles
                    {
                        ID = "14",
                        Title = "Basic Pruning - Small Trees & Shrubs",
                        Type = 2,
                        Dimension = 1
                    }
                    );
                    break;
                #endregion

                #region LAWN ACCESSORIES, SEED
                case 23://LAWN ACCESSORIES
                case 6://SEED
                    relatedArticles.Add(new ProductRelatedArticles
                    {
                        ID = "13",
                        Title = "Patch and Repair Lawn Damage",
                        Type = 1,
                        Dimension = 1
                    }
                    );
                    break;
                #endregion

                #region PLANTERS
                case 9:
                    relatedArticles.Add(new ProductRelatedArticles
                    {
                        ID = "11",
                        Title = "Hosting the Perfect BBQ Party",
                        Type = 3,
                        Dimension = 1
                    }
                        );
                    relatedArticles.Add(new ProductRelatedArticles
                    {
                        ID = "9",
                        Title = "Choosing Houseplants",
                        Type = 1,
                        Dimension = 2
                    }
                    );
                    break;
                #endregion

                #region TOOLS
                case 11:
                    relatedArticles.Add(new ProductRelatedArticles
                    {
                        ID = "14",
                        Title = "Basic Pruning - Small Trees & Shrubs",
                        Type = 2,
                        Dimension = 1
                    }
                    );
                    break;
                #endregion

                #region WATERING
                case 10:
                    relatedArticles.Add(new ProductRelatedArticles
                    {
                        ID = "8",
                        Title = "Lawn Sprinklers",
                        Type = 3,
                        Dimension = 2
                    }
                        );
                    relatedArticles.Add(new ProductRelatedArticles
                    {
                        ID = "9",
                        Title = "Choosing Houseplants",
                        Type = 1,
                        Dimension = 2
                    }
                    );
                    break;
                #endregion

                default:
                    break;
            }
            return relatedArticles;
        }
        #endregion

        #region Properties
        private GCDataContext DataContext
        {
            get
            {
                return _dataContext;
            }
        }
        #endregion
    }
}