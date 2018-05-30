using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enferno.Web.StormUtils.Test
{
    [TestClass]
    public class LinkTest
    {
        [TestMethod, TestCategory("UnitTest")]
        public void ImageUrlWithImageStringAndThumbnailPresetShouldReturnImageWithPreset()
        {
            // Arrange
            var Link = new Link();
            var initialImageString =
                "http://services.enferno.se/image/0d39bb82-df0b-4872-9ec4-37e46805dd47.jpg?preset=65";
            var imgString = "http://services.enferno.se/image/0d39bb82-df0b-4872-9ec4-37e46805dd47.jpg";
            var preset = 65;
            
            // Act
            var result = Link.ImageUrl(imgString, preset);

            // Assert
            Assert.AreEqual(initialImageString, result);
        }

        [TestMethod, TestCategory("UnitTest")]
        public void ImageWithGuidShouldReturnPresetAndExtension()
        {
            // Arrange
            var Link = new Link();
            var initialImageString =
                "http://services.enferno.se/image/0d39bb82-df0b-4872-9ec4-37e46805dd47.jpg?preset=65";
            var imgString = "0d39bb82-df0b-4872-9ec4-37e46805dd47";
            var preset = 65;
            var extension = "jpg";

            // Act
            var result = Link.ImageUrl(imgString, preset, extension);

            // Assert
            Assert.AreEqual(initialImageString, result);
        }

        [TestMethod, TestCategory("UnitTest")]
        public void ImageWithGuidOnlyShouldReturnNoPreset()
        {
            // Arrange
            var link = new Link();
            var imgString = Guid.NewGuid();

            var initialImageString =string.Format("http://services.enferno.se/image/{0}.jpg", imgString);
           
            // Act
            var result = Link.ImageUrl(imgString, null);

            // Assert
            Assert.AreEqual(initialImageString, result);
        }
    }
}
