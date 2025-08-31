using Desafio.Umbler.Controllers;
using Desafio.Umbler.Models.Entities;
using Desafio.Umbler.Models.ViewModels;
using Desafio.Umbler.Models.ApiModels;
using Desafio.Umbler.Services.Domains;
using Desafio.Umbler.Persistence;
using DnsClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class ControllersTest
    {
        [TestMethod]
        public void Home_Error_returns_View_With_Model()
        {
            var controller = new HomeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var response = controller.Error();
            var result = response as ViewResult;
            var model = result.Model as ErrorViewModel;

            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }
        
        [TestMethod]
        public async Task Domain_Controller_With_Valid_Domain_Returns_Ok()
        {
            var mockService = new Mock<IDomainsService>();
            var domainViewModel = new DomainViewModel 
            { 
                Name = "test.com", 
                Ip = "192.168.0.1", 
                HostedAt = "umbler.corp",
            };
            
            var result = new Result<DomainViewModel>(domainViewModel);
            mockService.Setup(s => s.GetDomainByName("test.com")).ReturnsAsync(result);

            var controller = new DomainController(mockService.Object);

            var response = await controller.Get("test.com");
            var okResult = response as OkObjectResult;
            var resultData = okResult.Value as Result<DomainViewModel>;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(resultData);
            Assert.AreEqual("test.com", resultData.Data.Name);
        }

        [TestMethod]
        public async Task Domain_Controller_With_Service_Error_Returns_BadRequest()
        {
            var mockService = new Mock<IDomainsService>();
            var errorResult = new Result<DomainViewModel>("Erro no serviço");
            mockService.Setup(s => s.GetDomainByName("invalid.com")).ReturnsAsync(errorResult);

            var controller = new DomainController(mockService.Object);

            var response = await controller.Get("invalid.com");
            var badRequestResult = response as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task Domain_Controller_With_Invalid_Domain_Returns_BadRequest()
        {
            var mockService = new Mock<IDomainsService>();
            var controller = new DomainController(mockService.Object);
            
            controller.ModelState.AddModelError("domainName", "Domínio ou IP informado é inválido");
            
            var response = await controller.Get("invalid");
            var badRequestResult = response as BadRequestObjectResult;
            var resultData = badRequestResult.Value as Result<Domain>;

            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsNotNull(resultData);
            Assert.IsTrue(resultData.Messages.Count > 0);
            Assert.AreEqual(MessageType.Error, resultData.Messages[0].Type);
        }

        [TestMethod]
        public async Task Domain_Controller_With_Valid_IP_Returns_Ok()
        {
            var mockService = new Mock<IDomainsService>();
            var domainViewModel = new DomainViewModel 
            { 
                Name = "192.168.1.1", 
                Ip = "192.168.1.1", 
                HostedAt = "Local Network",
            };
            
            var result = new Result<DomainViewModel>(domainViewModel);
            mockService.Setup(s => s.GetDomainByName("192.168.1.1")).ReturnsAsync(result);

            var controller = new DomainController(mockService.Object);

            var response = await controller.Get("192.168.1.1");
            var okResult = response as OkObjectResult;
            var resultData = okResult.Value as Result<DomainViewModel>;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(resultData);
            Assert.AreEqual("192.168.1.1", resultData.Data.Name);
        }

        [TestMethod]
        public async Task Domain_Controller_With_Multiple_Service_Errors_Returns_BadRequest()
        {
            var mockService = new Mock<IDomainsService>();
            var errorResult = new Result<DomainViewModel>();
            errorResult.Messages.Add(new Message(MessageType.Error, "Primeiro erro"));
            errorResult.Messages.Add(new Message(MessageType.Error, "Segundo erro"));
            
            mockService.Setup(s => s.GetDomainByName("error.com")).ReturnsAsync(errorResult);

            var controller = new DomainController(mockService.Object);

            var response = await controller.Get("error.com");
            var badRequestResult = response as BadRequestObjectResult;
            var resultData = badRequestResult.Value as Result<DomainViewModel>;

            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsNotNull(resultData);
            Assert.AreEqual(2, resultData.Messages.Count);
            Assert.IsTrue(resultData.Messages.TrueForAll(m => m.Type == MessageType.Error));
        }

        [TestMethod]
        public async Task Domain_Controller_With_Default_Messages_Returns_Ok()
        {
            var mockService = new Mock<IDomainsService>();
            var domainViewModel = new DomainViewModel 
            { 
                Name = "example.com", 
                Ip = "93.184.216.34", 
                HostedAt = "Example Corp",
            };
            
            var result = new Result<DomainViewModel>(domainViewModel);
            result.Messages.Add(new Message(MessageType.Default, "Cache atualizado"));
            result.Messages.Add(new Message(MessageType.Default, "Dados processados"));
            
            mockService.Setup(s => s.GetDomainByName("example.com")).ReturnsAsync(result);

            var controller = new DomainController(mockService.Object);

            var response = await controller.Get("example.com");
            var okResult = response as OkObjectResult;
            var resultData = okResult.Value as Result<DomainViewModel>;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(resultData);
            Assert.AreEqual("example.com", resultData.Data.Name);
            Assert.AreEqual(2, resultData.Messages.Count);
            Assert.IsFalse(resultData.Messages.Exists(m => m.Type == MessageType.Error));
        }

        [TestMethod]
        public async Task Domain_Controller_With_Default_And_Error_Returns_BadRequest()
        {
            var mockService = new Mock<IDomainsService>();
            var result = new Result<DomainViewModel>();
            result.Data = null;
            result.Messages.Add(new Message(MessageType.Default, "Informação geral"));
            result.Messages.Add(new Message(MessageType.Error, "Erro fatal"));
            
            mockService.Setup(s => s.GetDomainByName("mixed.com")).ReturnsAsync(result);

            var controller = new DomainController(mockService.Object);

            var response = await controller.Get("mixed.com");
            var badRequestResult = response as BadRequestObjectResult;
            var resultData = badRequestResult.Value as Result<DomainViewModel>;

            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsNotNull(resultData);
            Assert.IsTrue(resultData.Messages.Exists(m => m.Type == MessageType.Error));
        }

        [TestMethod]
        public async Task Domain_Controller_With_Empty_String_Returns_BadRequest()
        {
            var mockService = new Mock<IDomainsService>();
            var controller = new DomainController(mockService.Object);
            
            controller.ModelState.AddModelError("domainName", "Domínio ou IP informado é inválido");

            var response = await controller.Get("");
            var badRequestResult = response as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            
            mockService.Verify(s => s.GetDomainByName(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Domain_Controller_With_Null_Domain_Returns_BadRequest()
        {
            var mockService = new Mock<IDomainsService>();
            var controller = new DomainController(mockService.Object);
            
            controller.ModelState.AddModelError("domainName", "Domínio ou IP informado é inválido");

            var response = await controller.Get(null);
            var badRequestResult = response as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            
            mockService.Verify(s => s.GetDomainByName(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Domain_Controller_With_Multiple_ModelState_Errors_Returns_All_Errors()
        {
            var mockService = new Mock<IDomainsService>();
            var controller = new DomainController(mockService.Object);
            
            controller.ModelState.AddModelError("domainName", "Primeiro erro de validação");
            controller.ModelState.AddModelError("domainName", "Segundo erro de validação");

            var response = await controller.Get("invalid-domain");
            var badRequestResult = response as BadRequestObjectResult;
            var resultData = badRequestResult.Value as Result<Domain>;

            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsNotNull(resultData);
            Assert.AreEqual(2, resultData.Messages.Count);
            Assert.IsTrue(resultData.Messages.TrueForAll(m => m.Type == MessageType.Error));
        }

        [TestMethod]
        public async Task Domain_Controller_Service_Returns_Null_Result_Returns_Ok()
        {
            var mockService = new Mock<IDomainsService>();
            var result = new Result<DomainViewModel>();
            result.Data = null;
            
            mockService.Setup(s => s.GetDomainByName("test.com")).ReturnsAsync(result);

            var controller = new DomainController(mockService.Object);

            var response = await controller.Get("test.com");
            var okResult = response as OkObjectResult;
            var resultData = okResult.Value as Result<DomainViewModel>;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(resultData);
            Assert.IsNull(resultData.Data);
        }

        [TestMethod]
        public async Task Domain_Controller_Service_Throws_Exception_Should_Propagate()
        {
            var mockService = new Mock<IDomainsService>();
            mockService.Setup(s => s.GetDomainByName(It.IsAny<string>()))
                       .ThrowsAsync(new InvalidOperationException("Service error"));

            var controller = new DomainController(mockService.Object);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await controller.Get("test.com")
            );
        }

        [TestMethod]
        public async Task Domain_Controller_With_Only_Default_Messages_Returns_Ok()
        {
            var mockService = new Mock<IDomainsService>();
            var domainViewModel = new DomainViewModel 
            { 
                Name = "success.com", 
                Ip = "1.2.3.4", 
                HostedAt = "Success Host",
            };
            
            var result = new Result<DomainViewModel>(domainViewModel);
            result.Messages.Add(new Message(MessageType.Default, "Operação realizada"));
            result.Messages.Add(new Message(MessageType.Default, "Informação adicional"));
            
            mockService.Setup(s => s.GetDomainByName("success.com")).ReturnsAsync(result);

            var controller = new DomainController(mockService.Object);

            var response = await controller.Get("success.com");
            var okResult = response as OkObjectResult;
            var resultData = okResult.Value as Result<DomainViewModel>;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(resultData);
            Assert.AreEqual("success.com", resultData.Data.Name);
            Assert.AreEqual(2, resultData.Messages.Count);
            Assert.IsFalse(resultData.Messages.Exists(m => m.Type == MessageType.Error));
            Assert.IsTrue(resultData.Messages.TrueForAll(m => m.Type == MessageType.Default));
        }

        [TestMethod]
        public async Task Domain_Controller_With_Long_Domain_Name_Returns_Ok()
        {
            var mockService = new Mock<IDomainsService>();
            var longDomain = "very-long-subdomain.another-subdomain.example-domain.com";
            var domainViewModel = new DomainViewModel 
            { 
                Name = longDomain, 
                Ip = "5.6.7.8", 
                HostedAt = "Long Domain Host",
            };
            
            var result = new Result<DomainViewModel>(domainViewModel);
            mockService.Setup(s => s.GetDomainByName(longDomain)).ReturnsAsync(result);

            var controller = new DomainController(mockService.Object);

            var response = await controller.Get(longDomain);
            var okResult = response as OkObjectResult;
            var resultData = okResult.Value as Result<DomainViewModel>;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(resultData);
            Assert.AreEqual(longDomain, resultData.Data.Name);
        }

        [TestMethod]
        public async Task Domain_Controller_Calls_Service_Exactly_Once()
        {
            var mockService = new Mock<IDomainsService>();
            var domainViewModel = new DomainViewModel 
            { 
                Name = "verify.com", 
                Ip = "10.20.30.40", 
                HostedAt = "Verify Host",
            };
            
            var result = new Result<DomainViewModel>(domainViewModel);
            mockService.Setup(s => s.GetDomainByName("verify.com")).ReturnsAsync(result);

            var controller = new DomainController(mockService.Object);

            var response = await controller.Get("verify.com");

            mockService.Verify(s => s.GetDomainByName("verify.com"), Times.Once);
            mockService.Verify(s => s.GetDomainByName(It.IsAny<string>()), Times.Once);
        }
    }
}