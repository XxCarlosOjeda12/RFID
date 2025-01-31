using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bienvenida
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                bool demo = false;
                if (args.Length != 0)
                {
                    demo = args[0] == "/demo";
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1(demo));
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
