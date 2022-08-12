using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace SimpleProject.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [DisplayName("Title:")]
        [Required(ErrorMessage = "Title required")]
        public string Title { get; set; }

        [DisplayName("First Name:")]
        [Required(ErrorMessage = "First Name required")]
        [RegularExpression("[a-zA-Z]+", ErrorMessage = "Numbers & special charaters not allowed !")]
        [Remote("IsFirstNameAvailable", "User", HttpMethod = "POST", ErrorMessage = "First Name not available.")]
        public string FirstName { get; set; }

        [DisplayName("Last Name:")]
        [Required(ErrorMessage = "Last Name required")]
        [RegularExpression("[a-zA-Z]+", ErrorMessage = "Numbers & special charaters not allowed !")]
        [Remote("IsLastNameAvailable", "User", HttpMethod = "POST", ErrorMessage = "Last Name not available.")]
        public string LastName { get; set; }

        [DisplayName("Address:")]
        [Required(ErrorMessage = "Address required")]
        public string Address { get; set; }

        [DisplayName("Email:")]
        [Required(ErrorMessage = "Email required")]
        [RegularExpression(".+\\@.+\\..+", ErrorMessage = "Enter a valid Email address")]
        public string Email { get; set; }

        [DisplayName("Mobile Number:")]
        [Required(ErrorMessage = "Mobile Number required")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "Mobile Number should be 8 digits")]
        public string MobNum { get; set; }

        // table documents
        public int docsId { get; set; }
        public string DocsType { get; set; }
        [DisplayName("Certificate upload:")]
        public string DocsPath { get; set; }
        public HttpPostedFileBase[] DocsFile { get; set; }
        public string ImgType { get; set; }
        [DisplayName("Profile Image:")]
        public string ImgPath { get; set; }
        public HttpPostedFileBase ImgFile { get; set; }
        public int userId { get; set; }

    }
}