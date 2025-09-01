using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Net;

namespace Desafio.Umbler.Attributes
{
    public class DomainValidationAttribute : ValidationAttribute
    {
        private static readonly Regex DomainRegex = new Regex(
            @"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public DomainValidationAttribute()
        {
            ErrorMessage = "O valor informado não é um domínio ou IP válido.";
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            var input = value.ToString();

            if (string.IsNullOrWhiteSpace(input))
                return true; 

            var cleanInput = RemoveProtocol(input);
            cleanInput = RemoveWww(cleanInput);
            cleanInput = RemovePath(cleanInput);

            if (IsValidIpAddress(cleanInput))
                return true;

            return IsValidDomain(cleanInput);
        }

        private static bool IsValidIpAddress(string input)
        {
            return IPAddress.TryParse(input, out _);
        }

        private static bool IsValidDomain(string domain)
        {
            if (domain.Length > 253) // RFC 1035
                return false;

            if (domain.StartsWith("-") || domain.EndsWith("-"))
                return false;

            if (domain.Contains(".."))
                return false;

            if (!DomainRegex.IsMatch(domain))
                return false;

            if (!domain.Contains("."))
                return false;

            var labels = domain.Split('.');
            foreach (var label in labels)
            {
                if (string.IsNullOrEmpty(label) || label.Length > 63)
                    return false;

                if (label.StartsWith("-") || label.EndsWith("-"))
                    return false;
            }

            return true;
        }

        private static string RemoveProtocol(string domain)
        {
            if (domain.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                return domain.Substring(7);

            if (domain.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return domain.Substring(8);

            return domain;
        }

        private static string RemoveWww(string domain)
        {
            if (domain.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                return domain.Substring(4);

            return domain;
        }

        private static string RemovePath(string domain)
        {
            var slashIndex = domain.IndexOf('/');
            if (slashIndex > 0)
                return domain.Substring(0, slashIndex);

            var questionIndex = domain.IndexOf('?');
            if (questionIndex > 0)
                return domain.Substring(0, questionIndex);

            return domain;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, name);
        }
    }
}