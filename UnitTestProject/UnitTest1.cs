using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HorseMobileService;
using CustomAPIAMobileService.Controllers;
using Microsoft.WindowsAzure.Mobile.Service;
using Moq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using HorseMobileService.Controllers;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// Test method to test meController.
        /// Not completed yet.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestMethod1()
        {
            var controller = new meController();

            var mock = new Mock<HttpControllerContext>();
            controller.Request = new System.Net.Http.HttpRequestMessage();
            controller.Configuration = new System.Web.Http.HttpConfiguration();
            mock.SetupGet(x => x.RequestContext.Principal.Identity.Name).Returns("SOMEUSER");
            mock.SetupGet(x => x.RequestContext.Principal.Identity.IsAuthenticated).Returns(true);
            controller.ControllerContext = mock.Object;

            var me = await controller.Get();

            Assert.AreEqual(me.StatusCode, HttpStatusCode.OK);
        }
    }
}
