using Desafio.Umbler.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class DomainAttributeTests
    {
        private readonly DomainValidationAttribute _validator;

        public DomainAttributeTests()
        {
            _validator = new DomainValidationAttribute();
        }

        [TestMethod]
        [DataRow("google.com", true)]
        [DataRow("www.google.com", true)]
        [DataRow("https://google.com", true)]
        [DataRow("http://www.google.com", true)]
        [DataRow("subdomain.google.com", true)]
        [DataRow("example.com.br", true)]
        [DataRow("test-domain.org", true)]
        [DataRow("123.example.com", true)]
        [DataRow("example-123.com", true)]
        public void IsValid_ValidDomains_ReturnsTrue(string domain, bool expected)
        {
            var result = _validator.IsValid(domain);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow("192.168.1.1", true)]
        [DataRow("127.0.0.1", true)]
        [DataRow("10.0.0.1", true)]
        [DataRow("255.255.255.255", true)]
        [DataRow("0.0.0.0", true)]
        [DataRow("8.8.8.8", true)]
        [DataRow("::1", true)]
        [DataRow("2001:db8::1", true)]
        [DataRow("fe80::1", true)]
        public void IsValid_ValidIpAddresses_ReturnsTrue(string ip, bool expected)
        {
            var result = _validator.IsValid(ip);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow("", true)]
        [DataRow(null, true)]
        [DataRow("invalid", false)]
        [DataRow("invalid.", false)]
        [DataRow(".invalid", false)]
        [DataRow("invalid..com", false)]
        [DataRow("-invalid.com", false)]
        [DataRow("invalid-.com", false)]
        [DataRow("space domain.com", false)]
        [DataRow("domain.c", false)]
        [DataRow("verylongdomainnamethatexceedsthemaximumlengthallowedfordomainnames.com", false)]
        public void IsValid_InvalidDomains_ReturnsFalse(string domain, bool expected)
        {
            var result = _validator.IsValid(domain);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow("256.1.1.1", false)]
        [DataRow("192.168.1.1.1", false)]
        [DataRow("192.168.-1.1", false)]
        [DataRow("192.168.1.256", false)]
        public void IsValid_InvalidIpAddresses_ReturnsFalse(string ip, bool expected)
        {
            var result = _validator.IsValid(ip);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void IsValid_DomainWithPath_ReturnsTrue()
        {
            var domain = "google.com/search?q=test";

            var result = _validator.IsValid(domain);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValid_IpWithProtocol_ReturnsTrue()
        {
            var ip = "http://192.168.1.1";

            var result = _validator.IsValid(ip);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValid_DomainWithQueryString_ReturnsTrue()
        {
            var domain = "google.com?param=value";

            var result = _validator.IsValid(domain);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValid_DomainWithProtocolAndPath_ReturnsTrue()
        {
            var domain = "https://www.google.com/search?q=test";

            var result = _validator.IsValid(domain);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ErrorMessage_HasUpdatedMessage()
        {
            Assert.AreEqual("O valor informado não é um domínio ou IP válido.", _validator.ErrorMessage);
        }

        [TestMethod]
        public void IsValid_DomainWithConsecutiveDots_ReturnsFalse()
        {
            var domain = "test..example.com";

            var result = _validator.IsValid(domain);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValid_DomainStartingWithHyphen_ReturnsFalse()
        {
            var domain = "-example.com";

            var result = _validator.IsValid(domain);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValid_DomainEndingWithHyphen_ReturnsFalse()
        {
            var domain = "example-.com";

            var result = _validator.IsValid(domain);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValid_VeryLongDomain_ReturnsFalse()
        {
            var longLabel = new string('a', 250);
            var domain = $"{longLabel}.com";

            var result = _validator.IsValid(domain);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValid_LabelTooLong_ReturnsFalse()
        {
            var longLabel = new string('a', 64);
            var domain = $"{longLabel}.com";

            var result = _validator.IsValid(domain);

            Assert.IsFalse(result);
        }
    }
}