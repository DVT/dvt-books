using System;
using System.ComponentModel.DataAnnotations;

namespace DVTBooks.API.Libs.ComponentModel.DataAnnotations
{
    public class RelativeOrAbsoluteUrl : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null && !Uri.TryCreate(value.ToString(), UriKind.RelativeOrAbsolute, out _))
                return new ValidationResult(FormatErrorMessage(validationContext?.DisplayName));

            return ValidationResult.Success; 
        }
    }
}
