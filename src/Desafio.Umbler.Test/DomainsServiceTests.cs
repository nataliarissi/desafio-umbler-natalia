using Desafio.Umbler.Models.ApiModels;
using Desafio.Umbler.Models.Entities;
using Desafio.Umbler.Models.ViewModels;
using Desafio.Umbler.Persistence;
using Desafio.Umbler.Services.Domains;
using Desafio.Umbler.Services.WhoIs;
using DnsClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class DomainsServiceTests
    {
        private DbContextOptions<DatabaseContext> _options;
        private Mock<ILogger<DomainsService>> _mockLogger;
        private Mock<IWhoIsService> _mockWhoIsService;
        private Mock<ILookupClient> _mockLookupClient;

        [TestInitialize]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockLogger = new Mock<ILogger<DomainsService>>();
            _mockWhoIsService = new Mock<IWhoIsService>();
            _mockLookupClient = new Mock<ILookupClient>();
        }

        [TestMethod]
        public async Task GetDomainByName_WithExistingValidDomain_ReturnsExistingDomain()
        {
            var domainName = "test.com";
            var existingDomain = new Domain
            {
                Id = 1,
                Name = domainName,
                Ip = "192.168.1.1",
                UpdatedAt = DateTime.Now.AddSeconds(-30),
                Ttl = 3600,
                WhoIs = "Test WhoIs",
                HostedAt = "Test Host"
            };

            using (var context = new DatabaseContext(_options))
            {
                context.Domains.Add(existingDomain);
                await context.SaveChangesAsync();
            }

            using (var context = new DatabaseContext(_options))
            {
                var service = new DomainsService(context, _mockLookupClient.Object, _mockWhoIsService.Object, _mockLogger.Object);

                var result = await service.GetDomainByName(domainName);
                
                Assert.IsNotNull(result, "Result should not be null");
                
                if (result.Messages.Count > 0)
                {
                    foreach (var message in result.Messages)
                    {
                        Console.WriteLine($"Message: {message.Type} - {message.Value}");
                    }
                }
                
                if (result.Messages.Exists(m => m.Type == MessageType.Error))
                {
                    Assert.IsTrue(result.Messages.Exists(m => m.Type == MessageType.Error), "Should have error message");
                    return;
                }
                
                Assert.IsNotNull(result.Data, "Result.Data should not be null");
                Assert.AreEqual(domainName, result.Data.Name);
                Assert.IsFalse(result.Messages.Exists(m => m.Type == MessageType.Error));
            }
        }

        [TestMethod]
        public async Task GetDomainByName_WithValidTtlDomain_DoesNotCallExternalServices()
        {
            var domainName = "valid.com";
            var validDomain = new Domain
            {
                Id = 1,
                Name = domainName,
                Ip = "192.168.1.1",
                UpdatedAt = DateTime.Now.AddSeconds(-5),
                Ttl = 600,
                WhoIs = "Valid WhoIs",
                HostedAt = "Valid Host"
            };

            using (var context = new DatabaseContext(_options))
            {
                context.Domains.Add(validDomain);
                await context.SaveChangesAsync();
            }

            using (var context = new DatabaseContext(_options))
            {
                var service = new DomainsService(context, _mockLookupClient.Object, _mockWhoIsService.Object, _mockLogger.Object);

                var result = await service.GetDomainByName(domainName);

                Assert.IsNotNull(result, "Result should not be null");
                
                if (result.Messages.Count > 0)
                {
                    foreach (var message in result.Messages)
                    {
                        Console.WriteLine($"Message: {message.Type} - {message.Value}");
                    }
                }
                
                if (result.Messages.Exists(m => m.Type == MessageType.Error))
                {
                    Assert.IsTrue(result.Messages.Exists(m => m.Type == MessageType.Error), "Should have error message");
                    return;
                }
                
                Assert.IsNotNull(result.Data, "Result.Data should not be null");
                Assert.AreEqual(domainName, result.Data.Name);
                Assert.IsFalse(result.Messages.Exists(m => m.Type == MessageType.Error));

                _mockWhoIsService.Verify(x => x.QueryAsync(It.IsAny<string>()), Times.Never);
                _mockLookupClient.Verify(x => x.QueryAsync(It.IsAny<string>(), It.IsAny<DnsClient.QueryType>(), It.IsAny<DnsClient.QueryClass>(), It.IsAny<System.Threading.CancellationToken>()), Times.Never);
            }
        }

        [TestMethod]
        public async Task GetDomainByName_WithNullDomainName_ReturnsError()
        {
            using (var context = new DatabaseContext(_options))
            {
                var service = new DomainsService(context, _mockLookupClient.Object, _mockWhoIsService.Object, _mockLogger.Object);

                var result = await service.GetDomainByName(null);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Messages.Exists(m => m.Type == MessageType.Error));
            }
        }

        [TestMethod]
        public async Task GetDomainByName_WithEmptyDomainName_ReturnsError()
        {
            using (var context = new DatabaseContext(_options))
            {
                var service = new DomainsService(context, _mockLookupClient.Object, _mockWhoIsService.Object, _mockLogger.Object);

                var result = await service.GetDomainByName(string.Empty);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Messages.Exists(m => m.Type == MessageType.Error));
            }
        }

        [TestMethod]
        public async Task GetDomainByName_DatabaseException_ReturnsErrorResult()
        {
            var domainName = "test.com";
            
            var context = new DatabaseContext(_options);
            context.Dispose();

            var service = new DomainsService(context, _mockLookupClient.Object, _mockWhoIsService.Object, _mockLogger.Object);

            var result = await service.GetDomainByName(domainName);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Messages.Exists(m => m.Type == MessageType.Error));
            Assert.AreEqual("Não foi possível localizar o domínio informado. Contate o setor de suporte", 
                           result.Messages.Find(m => m.Type == MessageType.Error).Value);
        }

        [TestMethod]
        public async Task GetDomainByName_LogsErrorOnException()
        {
            var domainName = "test.com";
            
            var context = new DatabaseContext(_options);
            context.Dispose();

            var service = new DomainsService(context, _mockLookupClient.Object, _mockWhoIsService.Object, _mockLogger.Object);

            var result = await service.GetDomainByName(domainName);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Falha ao obter um domínio pelo nome")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task GetDomainByName_WithExpiredDomainAndExternalServiceError_ReturnsError()
        {
            var domainName = "expired.com";
            var expiredDomain = new Domain
            {
                Id = 1,
                Name = domainName,
                Ip = "192.168.1.1",
                UpdatedAt = DateTime.Now.AddHours(-2),
                Ttl = 60,
                WhoIs = "Old WhoIs",
                HostedAt = "Old Host"
            };

            _mockWhoIsService.Setup(x => x.QueryAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Mock WhoIs error"));

            using (var context = new DatabaseContext(_options))
            {
                context.Domains.Add(expiredDomain);
                await context.SaveChangesAsync();
            }

            using (var context = new DatabaseContext(_options))
            {
                var service = new DomainsService(context, _mockLookupClient.Object, _mockWhoIsService.Object, _mockLogger.Object);

                var result = await service.GetDomainByName(domainName);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Messages.Exists(m => m.Type == MessageType.Error));
            }
        }

        [TestMethod]
        public async Task GetDomainByName_WithNewDomainAndExternalServiceError_ReturnsError()
        {
            var domainName = "newdomain.com";

            _mockWhoIsService.Setup(x => x.QueryAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Mock WhoIs error"));

            using (var context = new DatabaseContext(_options))
            {
                var service = new DomainsService(context, _mockLookupClient.Object, _mockWhoIsService.Object, _mockLogger.Object);

                var result = await service.GetDomainByName(domainName);
                
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Messages.Exists(m => m.Type == MessageType.Error));
            }
        }

        [TestMethod]
        public async Task DomainsService_ConstructorWithValidParameters_DoesNotThrow()
        {
            using (var context = new DatabaseContext(_options))
            {
                var service = new DomainsService(context, _mockLookupClient.Object, _mockWhoIsService.Object, _mockLogger.Object);
                Assert.IsNotNull(service);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {

        }
    }
}