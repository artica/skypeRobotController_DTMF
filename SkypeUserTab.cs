using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace skypeRobotController
{
    class SkypeUserTab : IEquatable<SkypeUserTab>
    {
        public string skypeChatUser;
        public TabPage tabPage;
        public TextBox textBox;


        public SkypeUserTab( string userName)
        {
            skypeChatUser = userName;
            
            tabPage = new TabPage(userName);
            
            textBox = new TextBox();
            
            textBox.Size = new System.Drawing.Size(252, 153);
            textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBox.Multiline = true;

            tabPage.Controls.Add(textBox);
        }
        public bool Equals(SkypeUserTab other)
        {
            return skypeChatUser == other.skypeChatUser;
        }


    }
}
