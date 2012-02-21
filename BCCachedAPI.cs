using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using THDGC.Core.Interfaces;
using THDGC.Core.CacheManager;
using THDGC.Core.ClassFactories;

namespace THDGC.Core.Brightcove
{
    class BCCachedAPI : BCVideoAPI
    {
		private static ILog _log = LogManager.GetLogger(typeof(BCVideoAPI));
		private const int cacheTimeoutInMinutes = 120;
		private const int emptyCacheTimeoutInMinutes = 5;
		private const int maxPerPage = 100;
        private bool _enableBrightcoveAPI = bool.Parse(ConfigurationManager.AppSettings["EnableBrightcoveAPI"]);

		public override BCResult FindAllVideos(BCSortByType sortBy, BCSortOrderType sortOrder)
		{
			return CacheHelperFactory.Get(CacheHelperType.Empty).GetFromCache<BCResult>("FindAllVideos", cacheTimeoutInMinutes, () => 
				{
					BCResult result;

					try
					{
                        if (_enableBrightcoveAPI == false)
                            throw new EmptyResultException(new BCResult(0, new List<BCVideo>()), emptyCacheTimeoutInMinutes);

						result = base.FindAllVideos(sortBy, sortOrder);
					}
					catch (Exception ex)
					{
						// Log the exception
						_log.Error("Brightcove API call failed.", ex);
						throw new EmptyResultException(new BCResult(0, new List<BCVideo>()), emptyCacheTimeoutInMinutes);
					}

					if (result.totalCount == 0)
						throw new EmptyResultException(new BCResult(0, new List<BCVideo>()), emptyCacheTimeoutInMinutes);

					return result;
				});
		}

		public override BCResult FindAllVideos(int pageSize, int pageNumber, BCSortByType sortBy, BCSortOrderType sortOrder)
		{
			return CacheHelperFactory.Get(CacheHelperType.Empty).GetFromCache<BCResult>(String.Format("FindAllVideos_{0}{1}", pageSize, pageNumber), cacheTimeoutInMinutes, () =>
			{
				BCResult result;

				try
				{
                    if (_enableBrightcoveAPI == false)
                        throw new EmptyResultException(new BCResult(0, new List<BCVideo>()), emptyCacheTimeoutInMinutes);

					result = base.FindAllVideos(pageSize, pageNumber, sortBy, sortOrder);
				}
				catch (Exception ex)
				{
					// Log the exception
					_log.Error("Brightcove API call failed.", ex);
					throw new EmptyResultException(new BCResult(0, new List<BCVideo>()), emptyCacheTimeoutInMinutes);
				}

				if (result.totalCount == 0)
					throw new EmptyResultException(new BCResult(0, new List<BCVideo>()), emptyCacheTimeoutInMinutes);

				return result;
			});
		}

		public override BCResult FindVideosByTags(string and_tags, string or_tags, BCSortByType sortBy, BCSortOrderType sortOrder)
		{
			return CacheHelperFactory.Get(CacheHelperType.Empty).GetFromCache<BCResult>(String.Format("FindVideosByTags_{0}{1}_{2}{3}", and_tags, or_tags, sortBy, sortOrder), cacheTimeoutInMinutes, () =>
			{
				BCResult result;

				try
				{
                    if (_enableBrightcoveAPI == false)
                        throw new EmptyResultException(new BCResult(0, new List<BCVideo>()), emptyCacheTimeoutInMinutes);

					result = base.FindVideosByTags(and_tags, or_tags, sortBy, sortOrder);
				}
				catch (Exception ex)
				{
					// Log the exception
					_log.Error("Brightcove API call failed.", ex);
					throw new EmptyResultException(new BCResult(0, new List<BCVideo>()), emptyCacheTimeoutInMinutes);
				}

				if (result.totalCount == 0)
					throw new EmptyResultException(new BCResult(0, new List<BCVideo>()), emptyCacheTimeoutInMinutes);

				return result;
			});
		}

		public override BCResult FindVideosByTags(string and_tags, string or_tags, int pageSize, int pageNumber, BCSortByType sortBy, BCSortOrderType sortOrder)
		{
			return CacheHelperFactory.Get(CacheHelperType.Empty).GetFromCache<BCResult>(String.Format("FindVideosByTags_{0}{1}_{2}{3}_{4}{5}", and_tags, or_tags, sortBy, sortOrder, pageSize, pageNumber), cacheTimeoutInMinutes, () =>
			{
				BCResult result;

				try
				{
                    if (_enableBrightcoveAPI == false)
                        throw new EmptyResultException(new BCResult(0, new List<BCVideo>()), emptyCacheTimeoutInMinutes);

					result = base.FindVideosByTags(and_tags, or_tags, pageSize, pageNumber, sortBy, sortOrder);
				}
				catch (Exception ex)
				{
					// Log the exception
					_log.Error("Brightcove API call failed.", ex);
					throw new EmptyResultException(new BCResult(0, new List<BCVideo>()), emptyCacheTimeoutInMinutes);
				}

				if (result.totalCount == 0)
					throw new EmptyResultException(new BCResult(0, new List<BCVideo>()), emptyCacheTimeoutInMinutes);

				return result;
			});
		}
	}
}