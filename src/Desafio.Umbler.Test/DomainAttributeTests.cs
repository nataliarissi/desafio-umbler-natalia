using Desafio.Umbler.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class DomainAttributeTests
    {
        private readonly DomainAttribute _validator;

        public DomainAttributeTests()
        {
            _validator = new DomainAttribute();
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
            // Act
            var result = _validator.IsValid(domain);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow("", true)] // Empty string é válido (deixa para Required validar)
        [DataRow(null, true)] // Null é válido (deixa para Required validar)
        [DataRow("invalid", false)]
        [DataRow("invalid.", false)]
        [DataRow(".invalid", false)]
        [DataRow("invalid..com", false)]
        [DataRow("-invalid.com", false)]
        [DataRow("invalid-.com", false)]
        [DataRow("space domain.com", false)]
        [DataRow("domain.c", false)] // TLD muito curto
        [DataRow("verylongdomainnamethatexceedsthemaximumlengthallowedfordomainnames.com", false)]
        public void IsValid_InvalidDomains_ReturnsFalse(string domain, bool expected)
        {
            // Act
            var result = _validator.IsValid(domain);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void IsValid_DomainWithPath_ReturnsTrue()
        {
            // Arrange
            var domain = "google.com/search?q=test";

            // Act
            var result = _validator.IsValid(domain);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValid_DomainWithQueryString_ReturnsTrue()
        {
            // Arrange
            var domain = "google.com?param=value";

            // Act
            var result = _validator.IsValid(domain);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValid_DomainWithProtocolAndPath_ReturnsTrue()
        {
            // Arrange
            var domain = "https://www.google.com/search?q=test";

            // Act
            var result = _validator.IsValid(domain);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ErrorMessage_HasDefaultMessage()
        {
            // Assert
            Assert.AreEqual("O domínio informado não possui um formato válido.", _validator.ErrorMessage);
        }

        [TestMethod]
        public void IsValid_DomainWithConsecutiveDots_ReturnsFalse()
        {
            // Arrange
            var domain = "test..example.com";

            // Act
            var result = _validator.IsValid(domain);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValid_DomainStartingWithHyphen_ReturnsFalse()
        {
            // Arrange
            var domain = "-example.com";

            // Act
            var result = _validator.IsValid(domain);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValid_DomainEndingWithHyphen_ReturnsFalse()
        {
            // Arrange
            var domain = "example-.com";

            // Act
            var result = _validator.IsValid(domain);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValid_VeryLongDomain_ReturnsFalse()
        {
            // Arrange - Criar um domínio com mais de 253 caracteres
            var longLabel = new string('a', 250);
            var domain = $"{longLabel}.com";

            // Act
            var result = _validator.IsValid(domain);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValid_LabelTooLong_ReturnsFalse()
        {
            // Arrange - Label com mais de 63 caracteres
            var longLabel = new string('a', 64);
            var domain = $"{longLabel}.com";

            // Act
            var result = _validator.IsValid(domain);

            // Assert
            Assert.IsFalse(result);
        }
    }
}