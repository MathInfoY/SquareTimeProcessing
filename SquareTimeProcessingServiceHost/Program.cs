#define RELEASE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.InteropServices;


namespace SquareTimeProcessingServiceHost
{
    class Program
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        static void Main(string[] args)
        {
#if RELEASE
            IntPtr hWnd = GetConsoleWindow();

            if (hWnd != IntPtr.Zero)
                ShowWindow(hWnd, 0);
#endif

            using (ServiceHost host = new ServiceHost(typeof(SquareTimeProcessingService.SquareTimeProcessingService)))
            {
                host.Open();
                Console.WriteLine("Host started @ " + DateTime.Now.ToString());
                Console.ReadLine();
            }
        }
    }
}
