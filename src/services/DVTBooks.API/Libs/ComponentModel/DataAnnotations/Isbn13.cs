using System.ComponentModel.DataAnnotations;

namespace DVTBooks.API.Libs.ComponentModel.DataAnnotations
{
    public class Isbn13 : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return base.IsValid(value);
        }
    }
}
