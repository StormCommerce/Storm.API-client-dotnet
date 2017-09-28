
using System.Text;
using System.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enferno.Web.StormUtils.Test
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod, Description("Tests the MachineKeyEncryption when dencrypting and decrypting with new methods")]
        public void MachineKeyEncryptionTest1()
        {
            const string data = "Some text to encrypt";
            var encrypted = MachineKeyEncryption.Encode(data);
            var decrypted = MachineKeyEncryption.Decode(encrypted);
            Assert.AreEqual(data, decrypted);
        }

        [TestMethod, Description("Tests the MachineKeyEncryption when dencrypting and decrypting with new methods using old encrypted data.")]
        public void MachineKeyEncryptionTest2()
        {
            const string originalText = "Some text to encrypt";
            // ReSharper disable once CSharpWarnings::CS0618
            var data = MachineKey.Encode(Encoding.UTF8.GetBytes(originalText), MachineKeyProtection.All);
            var decrypted = MachineKeyEncryption.Decode(data);
            Assert.AreEqual(originalText, decrypted);
        }
    }
}
