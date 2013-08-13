﻿using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.ShoppingCart;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class ShoppingCartController : AdminControllerBase
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
		private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Constructors

        public ShoppingCartController(ICustomerService customerService,
            IDateTimeHelper dateTimeHelper, IPriceFormatter priceFormatter,
			IStoreService storeService, ITaxService taxService,
			IPriceCalculationService priceCalculationService,
            IPermissionService permissionService, ILocalizationService localizationService)
        {
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
            this._priceFormatter = priceFormatter;
			this._storeService = storeService;
            this._taxService = taxService;
            this._priceCalculationService = priceCalculationService;
            this._permissionService = permissionService;
            this._localizationService = localizationService;
        }

        #endregion
        
        #region Methods

        //shopping carts
        public ActionResult CurrentCarts()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CurrentCarts(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var customers = _customerService.GetAllCustomers(null, null, null, null, null,
                null, null, 0, 0, null, null, null, true, ShoppingCartType.ShoppingCart,
                command.Page - 1, command.PageSize);

            var gridModel = new GridModel<ShoppingCartModel>
            {
                Data = customers.Select(x =>
                {
                    return new ShoppingCartModel()
                    {
                        CustomerId = x.Id,
                        CustomerEmail = x.IsGuest() ?
                        _localizationService.GetResource("Admin.Customers.Guest") :
                        x.Email,
                        TotalItems = x.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList().GetTotalProducts()
                    };
                }),
                Total = customers.TotalCount
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult GetCartDetails(int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            var cart = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();

            var gridModel = new GridModel<ShoppingCartItemModel>()
            {
                Data = cart.Select(sci =>
                {
                    decimal taxRate;
					var store = _storeService.GetStoreById(sci.StoreId);
                    var sciModel = new ShoppingCartItemModel()
                    {
                        Id = sci.Id,
						Store = store != null ? store.Name : "Unknown",
                        ProductVariantId = sci.ProductVariantId,
                        Quantity = sci.Quantity,
                        FullProductName = !String.IsNullOrEmpty(sci.ProductVariant.Name) ?
                            string.Format("{0} ({1})", sci.ProductVariant.Product.Name, sci.ProductVariant.Name) :
                            sci.ProductVariant.Product.Name,
                        UnitPrice = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetUnitPrice(sci, true), out taxRate)),
                        Total = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetSubTotal(sci, true), out taxRate)),
                        UpdatedOn = _dateTimeHelper.ConvertToUserTime(sci.UpdatedOnUtc, DateTimeKind.Utc)
                    };
                    return sciModel;
                }),
                Total = cart.Count
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }





        //wishlists
        public ActionResult CurrentWishlists()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CurrentWishlists(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var customers = _customerService.GetAllCustomers(null, null, null, null, null,
                null, null, 0, 0, null, null, null, 
                true, ShoppingCartType.Wishlist, command.Page - 1, command.PageSize);

            var gridModel = new GridModel<ShoppingCartModel>
            {
                Data = customers.Select(x =>
                {
                    return new ShoppingCartModel()
                    {
                        CustomerId = x.Id,
                        CustomerEmail = x.IsGuest() ?
                        _localizationService.GetResource("Admin.Customers.Guest") :
                        x.Email,
                        TotalItems = x.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList().GetTotalProducts()
                    };
                }),
                Total = customers.TotalCount
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult GetWishlistDetails(int customerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var customer = _customerService.GetCustomerById(customerId);
            var cart = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.Wishlist).ToList();

            var gridModel = new GridModel<ShoppingCartItemModel>()
            {
                Data = cart.Select(sci =>
                {
                    decimal taxRate;
					var store = _storeService.GetStoreById(sci.StoreId); 
                    var sciModel = new ShoppingCartItemModel()
                    {
                        Id = sci.Id,
						Store = store != null ? store.Name : "Unknown",
                        ProductVariantId = sci.ProductVariantId,
                        Quantity = sci.Quantity,
                        FullProductName = !String.IsNullOrEmpty(sci.ProductVariant.Name) ?
                            string.Format("{0} ({1})", sci.ProductVariant.Product.Name, sci.ProductVariant.Name) :
                            sci.ProductVariant.Product.Name,
                        UnitPrice = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetUnitPrice(sci, true), out taxRate)),
                        Total = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.ProductVariant, _priceCalculationService.GetSubTotal(sci, true), out taxRate)),
                        UpdatedOn = _dateTimeHelper.ConvertToUserTime(sci.UpdatedOnUtc, DateTimeKind.Utc)
                    };
                    return sciModel;
                }),
                Total = cart.Count
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        #endregion
    }
}