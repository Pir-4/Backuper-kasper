using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Backup;
namespace MangerTest
{
    [TestClass]
    public class PrivilegeDriverTest
    {
        PrivilegeDriver driver = new PrivilegeDriver();

        /// <summary>
        /// Тест получение привелегий. Если не произошло исключения, то привелегии получены.
        /// </summary>
        [TestMethod]
        public void UpPrivilegeTest()
        {
            try
            {
                driver.UpPrivilege();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
    }
}
