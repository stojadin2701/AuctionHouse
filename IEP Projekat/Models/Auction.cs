namespace IEP_Projekat.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web;
    using System.Web.Mvc;
    public partial class Auction
    {
        public int Id { get; set; }


        [StringLength(50)]
        [Required]
        public string Name { get; set; }


        //[Display(Name = "Duration (seconds)")]
        //[Range(10, int.MaxValue, ErrorMessage = "Auction duration must be 10 seconds or longer!")]
        [Required]
        public int Duration { get; set; }


        //[Display(Name = "Starting price (tokens)")]
        //[Range(0, int.MaxValue, ErrorMessage = "Token price must be zero or higher!")]
        [Required]
        public int StartPrice { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? OpenTime { get; set; }

        public DateTime? CloseTime { get; set; }

        [StringLength(50)]
        public string State { get; set; }

        //[NotMapped, Required]
        //[Display(Name = "Upload Image")]
        [NotMapped]
        public HttpPostedFileBase ImageToUpload { get; set; }

        public byte[] ImageContent { get; set; }

        [StringLength(50)]
        public string ImageMimeType { get; set; }

        [StringLength(128)]
        public string LastBidderId { get; set; }
        
    }

    public class AuctionCreateModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Duration (seconds)")]
        [Range(10, int.MaxValue, ErrorMessage = "Auction duration must be 10 seconds or longer!")]
        public int Duration { get; set; }

        [Required]
        [Display(Name = "Starting price (tokens)")]
        [Range(0, int.MaxValue, ErrorMessage = "Token price must be zero or higher!")]
        public int StartPrice { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? OpenTime { get; set; }

        public DateTime? CloseTime { get; set; }

        [StringLength(50)]
        public string State { get; set; }

        [NotMapped, Required]
        [Display(Name = "Upload Image")]
        public HttpPostedFileBase ImageToUpload { get; set; }

        public byte[] ImageContent { get; set; }

        [StringLength(50)]
        public string ImageMimeType { get; set; }

        [StringLength(128)]
        public string LastBidderId { get; set; }
    }

    public class AuctionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        [Range(10, int.MaxValue, ErrorMessage = "Auction duration must be 10 seconds or longer!")]
        public int Duration { get; set; }

        [Display(Name = "Current price (tokens)")]
        [Range(0, int.MaxValue, ErrorMessage = "Token price must be zero or higher!")]
        public int Price { get; set; }
        public byte[] ImageContent { get; set; }
        public string State { get; set; }
        public string LastBidderUserName { get; set; }

        [Display(Name = "Created")]
        public DateTime CreateTime { get; set; }

        [Display(Name = "Opened")]
        public DateTime? OpenTime { get; set; }

        [Display(Name = "Closed")]
        public DateTime? CloseTime { get; set; }
    }

    public class AuctionMemoryModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Duration { get; set; }

        public int Price { get; set; }

        public string State { get; set; }

        public string LastBidderUserName { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? OpenTime { get; set; }

        public DateTime? CloseTime { get; set; }
    }
}
