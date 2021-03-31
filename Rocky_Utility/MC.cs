using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Rocky_Utility
{
    public static class MC
    {
        public const string ImagePath = @"\Images\Product\";
        public const string SesssionCart = "ShoppingCartSession";
        public const string SesssionInquiryId = "InquiryCartSession";
        public const string AdminRole = "Admin";
        public const string CustomerRole = "Customer";


        public const string EmailAdmin = "Andrei_liquid@hotmail.com";

        public const string Category = "Category";
        public const string Application = "ApplicationTypeName";

        public const string Success = "Success";
        public const string Error = "Error";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";

        public const string StatusInProcces = "Proccessing";

        public const string StatusShipped = "Shipped";
        public const string StatusCancelled  = "Cancelled";

        public const string StatusRefunded = "Refunded";

        public static readonly IEnumerable<string> listStatus = new ReadOnlyCollection<string>(
        
            new List<string>
            {
                StatusApproved,StatusCancelled,StatusInProcces,StatusPending,StatusRefunded,StatusShipped
            });
        }
}
