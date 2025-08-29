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
        public void Home_Index_returns_View()
        {
            var controller = new HomeController();

            var response = controller.Index();
            var result = response as ViewResult;

            Assert.IsNotNull(result);
        }

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
                WhoIs = "Ns.umbler.com" 
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
    }
}