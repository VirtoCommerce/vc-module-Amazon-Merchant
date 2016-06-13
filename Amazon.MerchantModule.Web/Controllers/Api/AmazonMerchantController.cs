using Amazon.MerchantModule.Web.Helpers.Interfaces;
using Amazon.MerchantModule.Web.Providers;
using Amazon.MerchantModule.Web.Services;
using AmazonMWSClientLib.Implementation.Mws;
using AmazonMWSClientLib.Model.Feeds;
using MarketplaceWebService;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Web.Security;

namespace Amazon.MerchantModule.Web.Controllers.Api
{
    [ApiExplorerSettings(IgnoreApi=true)]
    [RoutePrefix("api/amazon")]
    [CheckPermission(Permission = PredefinedPermissions.Manage)]
    public class AmazonMerchantController : ApiController
    {
        private readonly IAmazonProductProvider _productProvider;
        private readonly IAmazonSettings _settingsManager;
		private readonly IPushNotificationManager _pushNotificationManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IMarketplaceWebServiceClient _amazonMwsService;

        public AmazonMerchantController(
            IAmazonSettings settingsManager, 
            IAmazonProductProvider productProvider,
			IPushNotificationManager pushNotificationManager, 
            IDateTimeProvider dateTimeProvider,
            IMarketplaceWebServiceClient amazonMwsService)
        {
            _settingsManager = settingsManager;
            _productProvider = productProvider;
			_pushNotificationManager = pushNotificationManager;
            _dateTimeProvider = dateTimeProvider;
            _amazonMwsService = amazonMwsService;
        }
        
        [HttpGet]
        [ResponseType(typeof(void))]
        [Route("products/sync/{productId}")]
        public IHttpActionResult SyncProduct(string productId)
        {
            var products = _productProvider.GetProductUpdates(new[] { productId });
            SubmitFeedSender.SendAmazonFeeds(_amazonMwsService, products, AmazonEnvelopeMessageType.Product, AmazonFeedType._POST_PRODUCT_DATA_, _settingsManager.MerchantId, _settingsManager.MarketplaceId, _settingsManager.ServiceURL, _settingsManager.AwsAccessKeyId, _settingsManager.AwsSecretAccessKey);
            return Ok();
        }

        [HttpGet]
        [Route("products/sync/batch/{catalogId}/{categoryId}")]
        public IHttpActionResult BatchCategoryProducts(string catalogId, string categoryId)
        {
            //get products to update by provided catalog and category
            var products = _productProvider.GetCatalogProductsBatchRequest(catalogId, categoryId);
            SubmitFeedSender.SendAmazonFeeds(_amazonMwsService, products, AmazonEnvelopeMessageType.Product, AmazonFeedType._POST_PRODUCT_DATA_, _settingsManager.MerchantId, _settingsManager.MarketplaceId, _settingsManager.ServiceURL, _settingsManager.AwsAccessKeyId, _settingsManager.AwsSecretAccessKey);

            return Ok(new[] { 0 });
        }
    }
}
