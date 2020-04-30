using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DVTBooks.API.Libs.ComponentModel.DataAnnotations
{
    public class Isbn10 : ValidationAttribute
    {
        /// <summary>
        /// Checks that an ISBN-10 number is valid by checking its length and the validity of its check digit.
        /// </summary>
        /// <param name="value">The ISBN-10 number</param>
        /// <param name="validationContext">The validation context</param>
        /// <returns>A validation result</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var isbnString = value.ToString();

            var endOfIsbnPrefix = isbnString.IndexOf("ISBN-10:", StringComparison.OrdinalIgnoreCase) != -1 ?
                7 : isbnString.IndexOf("ISBN-10", StringComparison.OrdinalIgnoreCase) != -1 ?
                6 : -1;

            isbnString = isbnString.Substring(endOfIsbnPrefix + 1);

            try
            {
                var isbnDigits = Regex.Replace(isbnString, @"[^\d]", string.Empty, RegexOptions.None, TimeSpan.FromMilliseconds(500));

                if (isbnDigits.Length == 10)
                {

                    int digitSum = 0;
                    ReadOnlySpan<char> digitSpan = isbnDigits.AsSpan();

                    for (var i = 0; i < 9; i++)
                    {

                        if (!int.TryParse(digitSpan.Slice(i, 1), out int digit))
                            return new ValidationResult(FormatErrorMessage(validationContext?.DisplayName));

                        digitSum += digit * (10 - i);
                    }

                    if (digitSpan.Slice(9, 1).EndsWith("X", StringComparison.OrdinalIgnoreCase))
                    {
                        digitSum += 10;
                    }
                    else
                    {
                        digitSum += int.Parse(digitSpan.Slice(9, 1));
                    }

                    if (digitSum % 11 == 0)
                    {
                        return ValidationResult.Success;
                    }

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
