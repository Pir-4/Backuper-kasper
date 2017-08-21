using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Backup;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class ManagerTest
    {
        Manager manager;

        /// <summary>
        /// Тестирование функции Backup на специально созданных файлах
        /// </summary>
        [TestMethod]
        public void BackupFakeFile()
        {
            string currentDirectory = Path.Combine(Environment.CurrentDirectory, "Test");
            Directory.CreateDirectory(currentDirectory);
            try
            {
                List<string> paths = new List<string>()
                {
                    Path.Combine(currentDirectory,"1.dll"),
                    Path.Combine(currentDirectory,"2.exe"),
                    Path.Combine(currentDirectory,"3.txt"),
                };

                foreach (var path in paths)
                {
                    FileInfo file = new FileInfo(path);
                    FileStream fs = file.Create();
                    fs.Close();
                }
                manager = new Manager(Application.ExecutablePath, paths);
                manager.Backup();
                foreach (var path in paths)
                {
                    Assert.IsTrue(File.Exists(path), String.Format("File {0} not exists", path));
                    Assert.IsTrue(File.Exists(manager.PathAddition(path)), String.Format("File {0} not exists", manager.PathAddition(path)));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
            finally
            {
                Directory.Delete(currentDirectory, true);
            }
        }

        /// <summary>
        /// Тестирование функции ComeBack на специально созданных файлах
        /// </summary>
        [TestMethod]
        public void ComeBackFakeFile()
        {
            string currentDirectory = Path.Combine(Environment.CurrentDirectory, "Test");
            Directory.CreateDirectory(currentDirectory);
            try
            {
                List<string> paths = new List<string>()
                {
                    Path.Combine(currentDirectory,"1.dll"),
                    Path.Combine(currentDirectory,"2.exe"),
                    Path.Combine(currentDirectory,"3.txt"),
                };

                manager = new Manager(Application.ExecutablePath, paths);

                foreach (var path in paths)
                {
                    FileInfo file = new FileInfo(path);
                    FileStream fs = file.Create();
                    fs.Close();

                    file = new FileInfo(manager.PathAddition(path));
                    fs = file.Create();
                    fs.Close();
                }

                manager.ComeBack();
                foreach (var path in paths)
                {
                    Assert.IsTrue(File.Exists(path), String.Format("File {0} not exists", path));
                    Assert.IsFalse(File.Exists(manager.PathAddition(path)), String.Format("File {0} exists", manager.PathAddition(path)));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
            finally
            {
                Directory.Delete(currentDirectory, true);
            }
        }

        /// <summary>
        /// Тестироване функций Backup и ComeBack на несуществующих файлах
        /// </summary>
        [TestMethod]
        public void NotExistsFile()
        {
            string currentDirectory = Path.Combine(Environment.CurrentDirectory, "Test");
            Directory.CreateDirectory(currentDirectory);
            try
            {
                List<string> paths = new List<string>()
                {
                    Path.Combine(currentDirectory,"1.dll"),
                    Path.Combine(currentDirectory,"2.exe"),
                    Path.Combine(currentDirectory,"3.txt"),
                };

                manager = new Manager(Application.ExecutablePath, paths);
                manager.Backup();
                string[] files = Directory.GetFiles(currentDirectory);
                Assert.IsTrue(files.Length == 0, " Backup: files.Count != 0 ");

                manager.ComeBack();
                files = Directory.GetFiles(currentDirectory);
                Assert.IsTrue(files.Length == 0, " ComeBack: files.Count != 0 ");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
            finally
            {
                Directory.Delete(currentDirectory, true);
            }
        }

        /// <summary>
        /// Проверка преобразования шаблонных путей в абсолютные
        /// </summary>
        [TestMethod]
        public void ControllFilePaths()
        {
            try
            {
                List<string> paths = new List<string>()
                {
                    @"%SystemRoot%\twain_32.dll",
                    @"%SystemRoot%\system32\nslookup.exe",
                    @"%ProgramFiles%\Internet Explorer\iexplore.exe"
                };

                manager = new Manager(Application.ExecutablePath, paths);

                List<string> newPaths = paths.Select(item => Environment.ExpandEnvironmentVariables(item)).ToList();
                Assert.IsTrue(newPaths.Count() == manager.Paths.Count(), "Length lists not equals");

                foreach (var path in newPaths)
                    Assert.IsTrue(manager.Paths.ToList().Contains(path), String.Format("List not contains {0}", path));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Тестирование функции Backup на контрольных файлах системы
        /// </summary>
        [TestMethod]
        public void ControllFile()
        {
            List<string> paths = new List<string>()
                {
                    @"%SystemRoot%\twain_32.dll",
                    @"%SystemRoot%\system32\nslookup.exe",
                    @"%ProgramFiles%\Internet Explorer\iexplore.exe"
                };

            manager = new Manager(Application.ExecutablePath, paths);

            try
            {
                manager.Backup();

                List<string> newPaths = paths.Select(item => Environment.ExpandEnvironmentVariables(item)).ToList();
                foreach (var path in newPaths)
                {
                    Assert.IsTrue(File.Exists(path), String.Format("File {0} not exists", path));
                    Assert.IsTrue(File.Exists(manager.PathAddition(path)), String.Format("File {0} not exists", manager.PathAddition(path)));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
            finally
            {
                manager.ComeBack();
            }
        }
    }
}
