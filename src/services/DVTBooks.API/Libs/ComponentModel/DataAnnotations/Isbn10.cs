using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DVTBooks.API.Libs.ComponentModel.DataAnnotations
{
    public class Isbn10 : ValidationAttribute
    {
        /// <summary>
        /// Performs a rudimentary validation on the length of an ISBN-10 number and its check digit.
        /// </summary>
        /// <param name="value">The ISBN-10 number</param>
        /// <param name="validationContext">The validation context</param>
        /// <returns>A validation result</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var isbnStr = value.ToString();

            try
            {
                var isbnDigits = Regex.Replace(isbnStr, @"[^\d]", string.Empty, RegexOptions.None, TimeSpan.FromMilliseconds(500));

                if (isbnDigits.StartsWith("0") && isbnDigits.Length == 10)
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
