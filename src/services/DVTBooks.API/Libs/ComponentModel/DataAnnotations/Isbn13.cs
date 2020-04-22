using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DVTBooks.API.Libs.ComponentModel.DataAnnotations
{
    public class Isbn13 : ValidationAttribute
    {
        /// <summary>
        /// Checks that an ISBN-13 number is valid by checking its length and the validity of its check digit.
        /// </summary>
        /// <param name="value">The ISBN-13 number</param>
        /// <param name="validationContext">The validation context</param>
        /// <returns>A validation result</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            try
            {
                var isbnString = value.ToString();

                var endOfIsbnPrefix = isbnString.IndexOf("ISBN-13:", StringComparison.OrdinalIgnoreCase) != -1 ?
                    7 : isbnString.IndexOf("ISBN-13", StringComparison.OrdinalIgnoreCase) != -1 ?
                    6 : -1;

                isbnString = isbnString.Substring(endOfIsbnPrefix + 1);

                var isbnDigits = Regex.Replace(isbnString, @"[^\d]", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(500));

                if (!Regex.IsMatch(isbnDigits, @"^97(8|9)") || isbnDigits.Length != 13)
                    return new ValidationResult(FormatErrorMessage(validationContext?.DisplayName));

                int digitSum = 0;
                ReadOnlySpan<char> digitSpan = isbnDigits.AsSpan();

                for (var i = 0; i < 13; i++)
                {
                    int digit = int.Parse(digitSpan.Slice(i, 1));

                    int currentWeight = i % 2 == 0 ? 1 : 3;

                    digitSum += digit * currentWeight;
                }

                if (digitSum % 10 == 0)
                {
                    return ValidationResult.Success;
                }

                return new ValidationResult(FormatErrorMessage(validationContext?.DisplayName));
            }
            catch (RegexMatchTimeoutException)
            {
                return new ValidationResult(FormatErrorMessage(validationContext?.DisplayName));
            }
        }
    }
}
