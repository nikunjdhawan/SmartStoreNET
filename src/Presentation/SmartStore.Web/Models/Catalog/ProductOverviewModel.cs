﻿using System.Collections.Generic;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Models.Media;
using SmartStore.Core;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ProductOverviewModel : EntityModelBase
    {
        public ProductOverviewModel()
        {
            ProductPrice = new ProductPriceModel();
            DefaultPictureModel = new PictureModel();
            SpecificationAttributeModels = new List<ProductSpecificationModel>();
            //codehint: sm-add begin
            Manufacturers = new List<ManufacturerOverviewModel>();
            PagingFilteringContext = new CatalogPagingFilteringModel();
            ColorAttributes = new List<ColorAttributeModel>();
            //codehint: sm-add end
        }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string SeName { get; set; }

        //codehint: sm-add begin
        public int ThumbDimension { get; set; }
        public bool ShowSku { get; set; }
        public string Sku { get; set; }
        public bool ShowWeight { get; set; }
        public string Weight { get; set; }
        public bool ShowDimensions { get; set; }
        public string Dimensions { get; set; }
        public string DimensionMeasureUnit { get; set; }
        public bool ShowLegalInfo { get; set; }
        public string LegalInfo { get; set; }
        public IList<ManufacturerOverviewModel> Manufacturers { get; set; }
        public string TransportSurcharge { get; set; }
        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }
        public int RatingSum { get; set; }
        public int TotalReviews { get; set; }
        public bool ShowReviews { get; set; }
        public bool ShowDeliveryTimes { get; set; }
        public DeliveryTime DeliveryTime { get; set; }
        public bool IsShipEnabled { get; set; }
        public bool DisplayDeliveryTimeAccordingToStock { get; set; }
        public string StockAvailablity { get; set; }
        public bool DisplayBasePrice { get; set; }
        public string BasePriceInfo { get; set; }
		public int DefaultProductVariantId { get; set; }
        public bool CompareEnabled { get; set; }
        //codehint: sm-add end

        //price
        public ProductPriceModel ProductPrice { get; set; }
        //picture
        public PictureModel DefaultPictureModel { get; set; }
        //specification attributes
        public IList<ProductSpecificationModel> SpecificationAttributeModels { get; set; }
        // color Attributes
        public IList<ColorAttributeModel> ColorAttributes { get; set; }

		#region Nested Classes

        public partial class ProductPriceModel : ModelBase
        {
            public string OldPrice { get; set; }

            public string Price {get;set;}

            public bool DisableBuyButton { get; set; }

            //codehint: sm-add
            public bool DisableWishListButton { get; set; }

            public bool AvailableForPreOrder { get; set; }

            public bool ForceRedirectionAfterAddingToCart { get; set; }
        }

        public partial class ColorAttributeModel : ModelBase
        {
            public string Color { get; set; }
            public string Alias { get; set; }
            public string FriendlyName { get; set; }

            public override int GetHashCode()
            {
                return this.Color.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var equals = base.Equals(obj);
                if (!equals)
                {
                    var o2 = obj as ColorAttributeModel;
                    if (o2 != null)
                    {
                        equals = this.Color.IsCaseInsensitiveEqual(o2.Color);
                    }
                }
                return equals;
            }
        }

		#endregion
    }
}