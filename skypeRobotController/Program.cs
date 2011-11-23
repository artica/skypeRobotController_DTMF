using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace skypeRobotController
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
                
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new skypeRobotController());
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.InnerException.ToString());
            }
        }
    }
}

/*
using System;
using System.Collections.Generic;
using System.Linq;
using SKYPE4COMLib;
using System.Threading;

namespace TestSkype
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Skype skype = new Skype();
            skype.MessageStatus += new _ISkypeEvents_MessageStatusEventHandler(skype_MessageStatus);

            int i = 0;
            while (true)
            {
                i = skype.ActiveChats.Count;
                Thread.Sleep(16);
            }
        }

        static void skype_MessageStatus(ChatMessage pMessage, TChatMessageStatus Status)
        {
            if (Status == TChatMessageStatus.cmsReceived)
            {
                Console.WriteLine(string.Format("{0}: {1}", Status, pMessage.Body));
            }
        }
    }
}
*/