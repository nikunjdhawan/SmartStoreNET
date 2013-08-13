using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Affiliates;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;

namespace SmartStore.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order
    /// </summary>
    //[DataContract]
    public partial class Order : BaseEntity
    {

        private ICollection<DiscountUsageHistory> _discountUsageHistory;
        private ICollection<GiftCardUsageHistory> _giftCardUsageHistory;
        private ICollection<OrderNote> _orderNotes;
        private ICollection<OrderProductVariant> _orderProductVariant;
        private ICollection<Shipment> _shipments;

        #region Utilities

        protected virtual SortedDictionary<decimal, decimal> ParseTaxRates(string taxRatesStr)
        {
            var taxRatesDictionary = new SortedDictionary<decimal, decimal>();
            if (String.IsNullOrEmpty(taxRatesStr))
                return taxRatesDictionary;

            string[] lines = taxRatesStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (String.IsNullOrEmpty(line.Trim()))
                    continue;

                string[] taxes = line.Split(new char[] { ':' });
                if (taxes.Length == 2)
                {
                    try
                    {
                        decimal taxRate = decimal.Parse(taxes[0].Trim(), CultureInfo.InvariantCulture);
                        decimal taxValue = decimal.Parse(taxes[1].Trim(), CultureInfo.InvariantCulture);
                        taxRatesDictionary.Add(taxRate, taxValue);
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine(exc.ToString());
                    }
                }
            }

            //add at least one tax rate (0%)
            if (taxRatesDictionary.Count == 0)
                taxRatesDictionary.Add(decimal.Zero, decimal.Zero);

            return taxRatesDictionary;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        [DataMember]
        public Guid OrderGuid { get; set; }

		/// <summary>
		/// Gets or sets the store identifier
		/// </summary>
		public int StoreId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the billing address identifier
        /// </summary>
        public int BillingAddressId { get; set; }

        /// <summary>
        /// Gets or sets the shipping address identifier
        /// </summary>
        public int? ShippingAddressId { get; set; }

        /// <summary>
        /// Gets or sets an order status identifier
        /// </summary>
        [DataMember]
        public int OrderStatusId { get; set; }

        /// <summary>
        /// Gets or sets the shipping status identifier
        /// </summary>
        public int ShippingStatusId { get; set; }

        /// <summary>
        /// Gets or sets the payment status identifier
        /// </summary>
        public int PaymentStatusId { get; set; }

        /// <summary>
        /// Gets or sets the payment method system name
        /// </summary>
        [DataMember]
        public string PaymentMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets the customer currency code (at the moment of order placing)
        /// </summary>
        [DataMember]
        public string CustomerCurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the currency rate
        /// </summary>
        [DataMember]
        public decimal CurrencyRate { get; set; }

        /// <summary>
        /// Gets or sets the customer tax display type identifier
        /// </summary>
        public virtual int CustomerTaxDisplayTypeId { get; set; }

        /// <summary>
        /// Gets or sets the VAT number (the European Union Value Added Tax)
        /// </summary>
        [DataMember]
        public string VatNumber { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal (incl tax)
        /// </summary>
        [DataMember]
        public decimal OrderSubtotalInclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal (excl tax)
        /// </summary>
        [DataMember]
        public decimal OrderSubtotalExclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal discount (incl tax)
        /// </summary>
        [DataMember]
        public decimal OrderSubTotalDiscountInclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal discount (excl tax)
        /// </summary>
        [DataMember]
        public decimal OrderSubTotalDiscountExclTax { get; set; }

        /// <summary>
        /// Gets or sets the order shipping (incl tax)
        /// </summary>
        [DataMember]
        public decimal OrderShippingInclTax { get; set; }

        /// <summary>
        /// Gets or sets the order shipping (excl tax)
        /// </summary>
        [DataMember]
        public decimal OrderShippingExclTax { get; set; }

        /// <summary>
        /// Gets or sets the payment method additional fee (incl tax)
        /// </summary>
        [DataMember]
        public decimal PaymentMethodAdditionalFeeInclTax { get; set; }

        /// <summary>
        /// Gets or sets the payment method additional fee (excl tax)
        /// </summary>
        [DataMember]
        public decimal PaymentMethodAdditionalFeeExclTax { get; set; }

        /// <summary>
        /// Gets or sets the tax rates
        /// </summary>
        [DataMember]
        public string TaxRates { get; set; }

        /// <summary>
        /// Gets or sets the order tax
        /// </summary>
        [DataMember]
        public decimal OrderTax { get; set; }

        /// <summary>
        /// Gets or sets the order discount (applied to order total)
        /// </summary>
        [DataMember]
        public decimal OrderDiscount { get; set; }

        /// <summary>
        /// Gets or sets the order total
        /// </summary>
        [DataMember]
        public decimal OrderTotal { get; set; }

        /// <summary>
        /// Gets or sets the refunded amount
        /// </summary>
        [DataMember]
        public decimal RefundedAmount { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether reward points were earned for this order
        /// </summary>
        [DataMember]
        public bool RewardPointsWereAdded { get; set; }
        
        /// <summary>
        /// Gets or sets the checkout attribute description
        /// </summary>
        [DataMember]
        public string CheckoutAttributeDescription { get; set; }

        /// <summary>
        /// Gets or sets the checkout attributes in XML format
        /// </summary>
        [DataMember]
        public string CheckoutAttributesXml { get; set; }

        /// <summary>
        /// Gets or sets the customer language identifier
        /// </summary>
        [DataMember]
        public int CustomerLanguageId { get; set; }

        /// <summary>
        /// Gets or sets the affiliate identifier
        /// </summary>
        public int AffiliateId { get; set; }

        /// <summary>
        /// Gets or sets the customer IP address
        /// </summary>
        [DataMember]
        public string CustomerIp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether storing of credit card number is allowed
        /// </summary>
        public bool AllowStoringCreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the card type
        /// </summary>
        [DataMember]
        public string CardType { get; set; }

        /// <summary>
        /// Gets or sets the card name
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// Gets or sets the card number
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets the masked credit card number
        /// </summary>
        public string MaskedCreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the card CVV2
        /// </summary>
        public string CardCvv2 { get; set; }

        /// <summary>
        /// Gets or sets the card expiration month
        /// </summary>
        public string CardExpirationMonth { get; set; }

        /// <summary>
        /// Gets or sets the card expiration year
        /// </summary>
        public string CardExpirationYear { get; set; }

        /// codehint:sm-add begin
        /// <summary>
        /// Gets or sets a value indicating whether storing of credit card number is allowed
        /// </summary>
        public bool AllowStoringDirectDebit { get; set; }

        /// <summary>
        /// Gets or sets the direct debit account holder
        /// </summary>
        public string DirectDebitAccountHolder { get; set; }

        /// <summary>
        /// Gets or sets the direct debit account number
        /// </summary>
        public string DirectDebitAccountNumber { get; set; }

        /// <summary>
        /// Gets or sets the direct debit bank code
        /// </summary>
        public string DirectDebitBankCode { get; set; }

        /// <summary>
        /// Gets or sets the direct debit bank name
        /// </summary>
        public string DirectDebitBankName { get; set; }

        /// <summary>
        /// Gets or sets the direct debit bic
        /// </summary>
        public string DirectDebitBIC { get; set; }

        /// <summary>
        /// Gets or sets the direct debit country
        /// </summary>
        public string DirectDebitCountry { get; set; }

        /// <summary>
        /// Gets or sets the direct debit iban
        /// </summary>
        public string DirectDebitIban { get; set; }

        /// codehint:sm-add end
        
        /// <summary>
        /// Gets or sets the authorization transaction identifier
        /// </summary>
        public string AuthorizationTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction code
        /// </summary>
        public string AuthorizationTransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction result
        /// </summary>
        public string AuthorizationTransactionResult { get; set; }

        /// <summary>
        /// Gets or sets the capture transaction identifier
        /// </summary>
        public string CaptureTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the capture transaction result
        /// </summary>
        public string CaptureTransactionResult { get; set; }

        /// <summary>
        /// Gets or sets the subscription transaction identifier
        /// </summary>
        public string SubscriptionTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the purchase order number
        /// </summary>
        public string PurchaseOrderNumber { get; set; }

        /// <summary>
        /// Gets or sets the paid date and time
        /// </summary>
        public DateTime? PaidDateUtc { get; set; }
        
        /// <summary>
        /// Gets or sets the shipping method
        /// </summary>
        public string ShippingMethod { get; set; }

        /// <summary>
        /// Gets or sets the shipping rate computation method identifier
        /// </summary>
        public string ShippingRateComputationMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the date and time of order creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        #endregion

        #region Navigation properties

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        [DataMember]
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the billing address
        /// </summary>
        public virtual Address BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets the shipping address
        /// </summary>
        public virtual Address ShippingAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the reward points history record
        /// </summary>
        public virtual RewardPointsHistory RedeemedRewardPointsEntry { get; set; }

        /// <summary>
        /// Gets or sets discount usage history
        /// </summary>
        public virtual ICollection<DiscountUsageHistory> DiscountUsageHistory
        {
            get { return _discountUsageHistory ?? (_discountUsageHistory = new List<DiscountUsageHistory>()); }
            protected set { _discountUsageHistory = value; }
        }

        /// <summary>
        /// Gets or sets gift card usage history (gift card that were used with this order)
        /// </summary>
        public virtual ICollection<GiftCardUsageHistory> GiftCardUsageHistory
        {
            get { return _giftCardUsageHistory ?? (_giftCardUsageHistory = new List<GiftCardUsageHistory>()); }
            protected set { _giftCardUsageHistory = value; }
        }

        /// <summary>
        /// Gets or sets order notes
        /// </summary>
        public virtual ICollection<OrderNote> OrderNotes
        {
            get { return _orderNotes ?? (_orderNotes = new List<OrderNote>()); }
            protected set { _orderNotes = value; }
        }

        /// <summary>
        /// Gets or sets order product variants
        /// </summary>
        public virtual ICollection<OrderProductVariant> OrderProductVariants
        {
            get { return _orderProductVariant ?? (_orderProductVariant = new List<OrderProductVariant>()); }
            protected set { _orderProductVariant = value; }
        }

        /// <summary>
        /// Gets or sets shipments
        /// </summary>
        public virtual ICollection<Shipment> Shipments
        {
            get { return _shipments ?? (_shipments = new List<Shipment>()); }
            protected set { _shipments = value; }
        }

        #endregion

        #region Custom properties

        /// <summary>
        /// Gets or sets the order status
        /// </summary>
        public OrderStatus OrderStatus
        {
            get
            {
                return (OrderStatus)this.OrderStatusId;
            }
            set
            {
                this.OrderStatusId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the payment status
        /// </summary>
        public PaymentStatus PaymentStatus
        {
            get
            {
                return (PaymentStatus)this.PaymentStatusId;
            }
            set
            {
                this.PaymentStatusId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the shipping status
        /// </summary>
        public ShippingStatus ShippingStatus
        {
            get
            {
                return (ShippingStatus)this.ShippingStatusId;
            }
            set
            {
                this.ShippingStatusId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the customer tax display type
        /// </summary>
        public TaxDisplayType CustomerTaxDisplayType
        {
            get
            {
                return (TaxDisplayType)this.CustomerTaxDisplayTypeId;
            }
            set
            {
                this.CustomerTaxDisplayTypeId = (int)value;
            }
        }

        /// <summary>
        /// Gets the applied tax rates
        /// </summary>
        public SortedDictionary<decimal, decimal> TaxRatesDictionary
        {
            get
            {
                return ParseTaxRates(this.TaxRates);
            }
        }
        
        #endregion
    }
}