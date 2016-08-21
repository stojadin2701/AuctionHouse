namespace IEP_Projekat.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;
    public partial class Order
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string UserId { get; set; }

        public int TokenNum { get; set; }

        [Column(TypeName = "money")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public DateTime? OrderTime { get; set; }
    }

    public class OrderViewModel
    {


        //[Phone]
        [Required(ErrorMessage = "Phone number required")]
        [RegularExpression(@"^06[01234569]\d{6,7}$",
         ErrorMessage = "This isn't a valid mobile phone number in Serbia (ex. 06X1234567).")]
        public string PhoneNum { get; set; }

        [Display(Name = "Token Bundles")]
        public List<SelectListItem> BundleList { get; set; }

        public int BundleId { get; set; }

    }
}
