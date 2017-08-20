using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace Backup
{
    class Program
    {//http://www.cyberforum.ru/blogs/251328/blog280.html как запусить приложение с правами админа

        static void Main(string[] args)
        {
            List<string> paths = new List<string>(){
                @"%SystemRoot%\twain_32.dll",
                @"%SystemRoot%\system32\nslookup.exe",
               @"%ProgramFiles%\Internet Explorer\iexplore.exe"
            };
            Manager manager = new Manager(Application.ExecutablePath, paths);
            manager.Backup();
            if (args.Length > 0 && args[0].Contains("comeback"))
                manager.ComeBack();
        }
    }
}
