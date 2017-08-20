using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;

namespace Backup
{
    public class Manager
    {
        const string TRUSTEDINSTALLER = @"NT SERVICE\TrustedInstaller";
        const string ADDITION = "___Backup";

        PrivilegeDriver _privilegeDriver = new PrivilegeDriver();
        private readonly string _pathApp;

        public Manager(string pathApp, IEnumerable<string> filePaths)
        {
            Paths = filePaths.Select(item => Environment.ExpandEnvironmentVariables(item));
            _pathApp = pathApp;
        }

        public IEnumerable<string> Paths { get; private set; }

        public void Backup()
        {
            try
            {
                Version ver = Environment.OSVersion.Version;
                if (ver.Major > 5)
                    _privilegeDriver.UpPrivilege();

                Paths.ToList().ForEach(path =>
                    {
                        if (File.Exists(path))
                        {
                            File.Move(path, PathAddition(path));
                            File.Copy(_pathApp, path);
                        }
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;    
            }
        }
        public void ComeBack()
        {
            try
            {
                Paths.ToList().ForEach(path =>
                {
                    string additionPath = PathAddition(path);
                    if (File.Exists(additionPath))
                    {
                        File.Delete(path);
                        File.Move(additionPath, path);
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;   
            }
        }

        public string PathAddition(string path)
        {
            return path + ADDITION;
        }
        /*private void SetOwnerFile(string path, string account)
        {

            try
            {
                var owner = new NTAccount(account);
                FileInfo file = new FileInfo(path);
                var fileSecurity = file.GetAccessControl();
                fileSecurity.SetOwner(owner);
                file.SetAccessControl(fileSecurity);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }*/

    }
}
