using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBackUpFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            Util util = new Util();
            util.BackupFiles();
        }
    }
}
