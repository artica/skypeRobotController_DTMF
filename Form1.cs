using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SKYPE4COMLib;
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;


namespace skypeRobotController
{
    public partial class skypeRobotController : Form
    {
        Skype skype;
        List<SkypeUserTab> skypeUserTabs = new List<SkypeUserTab>();

        string selectedSkypeChatUser;
        bool chatUserSelected;
        string[] skypeUserName = new string[100];
        System.Threading.Timer time;
        string skypeAdmin;
        bool skypeAdminSelected;
        TcpControlServer controlServer = new TcpControlServer();

        int skypeChats = 0;
        int controlState = 0;
        int callID;
        int numCalls = 0;

        public skypeRobotController()
        {
            this.FormClosing += delegate
            {
                serialPort.Close();
                // serialPort.
            };

            InitializeComponent();
            skype = new Skype();
            TimerCallback tcb = this.CheckStatus;
            AutoResetEvent ar = new AutoResetEvent(true);
            skype.MessageStatus += new _ISkypeEvents_MessageStatusEventHandler(skype_MessageStatus);
            time = new System.Threading.Timer(tcb, ar, 250, 250);

            //availabe COM ports
            SerialPort tmp;
            foreach (string str in SerialPort.GetPortNames())
            {
                tmp = new SerialPort(str);
                if (tmp.IsOpen == false)
                    port_combobox.Items.Add(str);
            }

            try
            {
                // Try setting our custom event handler while avoiding any ambiguity.
                ((_ISkypeEvents_Event)skype).CallStatus += OurCallStatus;
                

            }
            catch (Exception e)
            {
                // Write Call Status Event Handler Failed to window.
                msg_listbox.Text += string.Format(DateTime.Now.ToLocalTime() + ": " +
                 "Call Status Event Handler Failed" +
                 " - Exception Source: " + e.Source + " - Exception Message: " + e.Message +
                 "\r\n");

                
            }

            try
            {
                // Try setting our custom event handler while avoiding any ambiguity.
                ((_ISkypeEvents_Event)skype).CallDtmfReceived += OurCallDtmfReceived;
            }
            catch (Exception e)
            {
            }
        }

        private void CheckStatus(Object stateInfo)
        {
            try
            {
                skypeChats = skype.ActiveChats.Count;
            }
            catch (InvalidCastException e)
            {
            }

            if (controlState == 3 && controlServer.Client != null && controlServer.Client.DataFromClient != null)
            {
                var splitter = "[/TCP]";
                var messages = controlServer.Client.DataFromClient.Split(splitter.ToCharArray());
                Console.WriteLine(messages[0]);
                if (serialPort.IsOpen)
                    this.BeginInvoke((Action)(() => { serialPort.WriteLine(messages[0]); }));
                //controlServer.Client.DataFromClient = string.Empty;
            }
            //this.BeginInvoke((Action)(() => { Console.WriteLine(skype.ActiveChats.Count); }));
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // blocks until TERM_CHAR is received
            string msg = serialPort.ReadExisting();
            if (msg[0] == 'i')
            {
                if (chatUserSelected)
                    this.BeginInvoke((Action)(() => { skype.SendMessage(selectedSkypeChatUser, "Please don't hurt me!! \n - I've just detected a hole in the ground."); }));
                msg_listbox.Invoke((Action)delegate() { msg_listbox.Text += string.Format("R: {0} \r\n", msg); });
            }
            else if (msg[0] == 'b')
            {
                if (chatUserSelected)
                    this.BeginInvoke((Action)(() => { skype.SendMessage(selectedSkypeChatUser, "Please don't hurt me!! \n - I've bumped against something."); }));
                msg_listbox.Invoke((Action)delegate() { msg_listbox.Text += string.Format("R: {0} \r\n", msg); });
            }
        }

        public void SerialPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort.PortName = port_combobox.Text;
            //open serial port
            serialPort.Open();
            port_combobox.Enabled = false;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
        }

        public void skype_MessageStatus(ChatMessage pMessage, TChatMessageStatus Status)
        {
            if (Status == TChatMessageStatus.cmsReceived)
            {
                if (!skypeUserTabs.Contains(new SkypeUserTab(pMessage.Sender.Handle)))
                {

                    user_combobox.Items.Add(pMessage.Sender.Handle);
                    var newTabUser = new SkypeUserTab(pMessage.Sender.Handle);
                    skypeUserTabs.Add(newTabUser);
                    tabUsers.Controls.Add(newTabUser.tabPage);
                    pMessage.Chat.SendMessage(Properties.Settings.Default.welcomeMessage);
                    //pMessage.Chat.SendMessage("Hello! \n \n Please wait until we give you the control of the robot.");
                    //pMessage.Chat.SendMessage(welcome_textBox.Text);

                    if (skypeAdminSelected) skype.SendMessage(skypeAdmin, string.Format("New user connected: {0}", pMessage.Sender.Handle));
                }

                if (pMessage.Body.Length == 1)
                {
                    if (pMessage.Sender.Handle == skypeAdmin)
                    {
                        /*if (pMessage.Body == "u")
                        {
                            string response = "Connected Users: \n";
                            foreach( var userTab in skypeUserTabs)
                            {
                                response += userTab.skypeChatUser;
                                skype.SendMessage(skypeAdmin, );
                            }
                        }*/
                    }
                    SkypeUserTab selected = skypeUserTabs.First(delegate(SkypeUserTab value) { return value.skypeChatUser == pMessage.Sender.Handle; });
                    selected.textBox.Text += string.Format("{0} \r\n", pMessage.Body);

                    if (selected.skypeChatUser == selectedSkypeChatUser)
                    {
                        if (serialPort.IsOpen)
                        {
                            if (pMessage.Body.Contains('3'))
                                controlState = 3;
                            else if (pMessage.Body.Contains('2'))
                                controlState = 2;
                            else if (pMessage.Body.Contains('1'))
                                controlState = 1;

                            serialPort.Write(pMessage.Body.ToCharArray(), 0, 1);
                            msg_listbox.Paste(string.Format("S: {0} \r\n", pMessage.Body));
                        }
                        else
                        {
                            msg_listbox.Text += string.Format("Failed to Send: {0} \r\n", pMessage.Body);
                            pMessage.Chat.SendMessage(Properties.Settings.Default.failedToSendMessage);
                        }
                    }
                    else
                    {
                        pMessage.Chat.SendMessage(Properties.Settings.Default.waitMessage);
                    }
                }
            }
        }

        private void msg_listbox_TextChanged(object sender, EventArgs e)
        {
            msg_listbox.SelectionStart = msg_listbox.Text.Length;
            msg_listbox.ScrollToCaret();
            msg_listbox.Refresh();

        }

        private void user_combobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //this.BeginInvoke((Action)(() => { skype.Call.Finish(); }));
            selectedSkypeChatUser = user_combobox.Text;
            this.BeginInvoke((Action)(() => { skype.SendMessage(selectedSkypeChatUser, Properties.Settings.Default.controllerMessage); }));
            //this.BeginInvoke((Action)(() => { skype.PlaceCall(selectedSkypeChatUser);}));
            ////this.BeginInvoke((Action)(() => { skype.Call.StartVideoSend(); }));
            chatUserSelected = true;
        }

        private void admin_button_Click(object sender, EventArgs e)
        {
            skypeAdmin = admin_textBox.Text;
            this.BeginInvoke((Action)(() => { skype.SendMessage(skypeAdmin, Properties.Settings.Default.adminMessage); }));
            skypeAdminSelected = true;
            admin_textBox.Enabled = false;
        }

        private void one_button_Click(object sender, EventArgs e)
        {
            controlState = 1;

            String msg = "1";
            if (serialPort.IsOpen)
            {
                this.BeginInvoke((Action)(() => { serialPort.WriteLine(msg); }));
                this.BeginInvoke((Action)(() => { msg_listbox.Paste(string.Format("S: {0} \r\n", msg)); }));
            }
            else
            {
                this.BeginInvoke((Action)(() => { msg_listbox.Text += string.Format("Failed to Send: {0} \r\n", msg); }));
            }
        }

        private void two_button_Click(object sender, EventArgs e)
        {
            controlState = 2;

            String msg = "2";
            if (serialPort.IsOpen)
            {
                this.BeginInvoke((Action)(() => { serialPort.WriteLine(msg); }));
                this.BeginInvoke((Action)(() => { msg_listbox.Paste(string.Format("S: {0} \r\n", msg)); }));
            }
            else
            {
                this.BeginInvoke((Action)(() => { msg_listbox.Text += string.Format("Failed to Send: {0} \r\n", msg); }));
            }
        }

        private void up_button_Click(object sender, EventArgs e)
        {
            if (controlState != 1) one_button.PerformClick();

            String msg = "w";
            if (serialPort.IsOpen)
            {
                this.BeginInvoke((Action)(() => { serialPort.WriteLine(msg); }));
                this.BeginInvoke((Action)(() => { msg_listbox.Paste(string.Format("S: {0} \r\n", msg)); }));
            }
            else
            {
                this.BeginInvoke((Action)(() => { msg_listbox.Text += string.Format("Failed to Send: {0} \r\n", msg); }));
            }
        }

        private void stop_button_Click(object sender, EventArgs e)
        {
            if (controlState != 1) one_button.PerformClick();

            String msg = "p";
            if (serialPort.IsOpen)
            {
                this.BeginInvoke((Action)(() => { serialPort.WriteLine(msg); }));
                this.BeginInvoke((Action)(() => { msg_listbox.Paste(string.Format("S: {0} \r\n", msg)); }));
            }
            else
            {
                this.BeginInvoke((Action)(() => { msg_listbox.Text += string.Format("Failed to Send: {0} \r\n", msg); }));
            }
        }

        private void down_button_Click(object sender, EventArgs e)
        {
            if (controlState != 1) one_button.PerformClick();

            String msg = "s";
            if (serialPort.IsOpen)
            {
                this.BeginInvoke((Action)(() => { serialPort.WriteLine(msg); }));
                this.BeginInvoke((Action)(() => { msg_listbox.Paste(string.Format("S: {0} \r\n", msg)); }));
            }
            else
            {
                this.BeginInvoke((Action)(() => { msg_listbox.Text += string.Format("Failed to Send: {0} \r\n", msg); }));
            }
        }

        private void left_button_Click(object sender, EventArgs e)
        {
            if (controlState != 1) one_button.PerformClick();

            String msg = "a";
            if (serialPort.IsOpen)
            {
                this.BeginInvoke((Action)(() => { serialPort.WriteLine(msg); }));
                this.BeginInvoke((Action)(() => { msg_listbox.Paste(string.Format("S: {0} \r\n", msg)); }));
            }
            else
            {
                this.BeginInvoke((Action)(() => { msg_listbox.Text += string.Format("Failed to Send: {0} \r\n", msg); }));
            }
        }

        private void right_button_Click(object sender, EventArgs e)
        {
            if (controlState != 1) one_button.PerformClick();

            String msg = "d";
            if (serialPort.IsOpen)
            {
                this.BeginInvoke((Action)(() => { serialPort.WriteLine(msg); }));
                this.BeginInvoke((Action)(() => { msg_listbox.Paste(string.Format("S: {0} \r\n", msg)); }));
            }
            else
            {
                this.BeginInvoke((Action)(() => { msg_listbox.Text += string.Format("Failed to Send: {0} \r\n", msg); }));
            }
        }

        private void one_tabPage_Click(object sender, EventArgs e)
        {
            controlState = 1;

            String msg = "1";
            if (serialPort.IsOpen)
            {
                this.BeginInvoke((Action)(() => { serialPort.WriteLine(msg); }));
                this.BeginInvoke((Action)(() => { msg_listbox.Paste(string.Format("S: {0} \r\n", msg)); }));
            }
            else
            {
                this.BeginInvoke((Action)(() => { msg_listbox.Text += string.Format("Failed to Send: {0} \r\n", msg); }));
            }
        }

        private void two_tabPage_Click(object sender, EventArgs e)
        {
            controlState = 2;

            String msg = "2";
            if (serialPort.IsOpen)
            {
                this.BeginInvoke((Action)(() => { serialPort.WriteLine(msg); }));
                this.BeginInvoke((Action)(() => { msg_listbox.Paste(string.Format("S: {0} \r\n", msg)); }));
            }
            else
            {
                this.BeginInvoke((Action)(() => { msg_listbox.Text += string.Format("Failed to Send: {0} \r\n", msg); }));
            }
        }
        private void SerialSkype_Load(object sender, EventArgs e)
        {

        }

        private void tabUsers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void admin_combobox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void admin_textBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void LineFollowSelectBut_Click(object sender, EventArgs e)
        {
            controlState = 3;

            String msg = "1";
            if (serialPort.IsOpen)
            {
                this.BeginInvoke((Action)(() => { serialPort.WriteLine(msg); }));
                this.BeginInvoke((Action)(() => { msg_listbox.Paste(string.Format("S: {0} \r\n", msg)); }));
            }
            else
            {
                this.BeginInvoke((Action)(() => { msg_listbox.Text += string.Format("Failed to Send: {0} \r\n", msg); }));
            }
        }

        public void OurCallDtmfReceived(Call call, string code)
        {
            // Always use try/catch with ANY Skype calls.
            try
            {
                String msg = "";
                if (code == "2")
                    msg = "w";
                else if (code == "4")
                    msg = "a";
                else if (code == "6")
                    msg = "d";
                else if (code == "8")
                    msg = "s";
                else if (code == "5")
                    msg = "p";
                // Write Call DTMF Received to Window.
                msg_listbox.Text += string.Format("Code DTMF Received -" + code + "\r\n");

                if (serialPort.IsOpen)
                {
                    this.BeginInvoke((Action)(() => { serialPort.WriteLine(msg); }));
                    this.BeginInvoke((Action)(() => { msg_listbox.Paste(string.Format("S: {0} \r\n", msg)); }));
                }
                else
                {
                    this.BeginInvoke((Action)(() => { msg_listbox.Text += string.Format("Failed to Send: {0} \r\n", msg); }));
                }
            }
            catch (Exception e)
            {
            }
        }
        public void OurCallStatus(Call call, TCallStatus status)
        {

            try
            {
                if (status == TCallStatus.clsInProgress /*call.VideoSendStatus == TCallVideoSendStatus.vssAvailable*/)
                {
                    call.StartVideoSend();
                }
            }
            catch
            {

            }
            
            
            // Always use try/catch with ANY Skype calls.
            try
            {
                if (status == TCallStatus.clsRinging)
                {
                    if (numCalls == 0)
                    {
                        call.Answer();
                        
                        
                        callID = call.Id;
                        numCalls++;
                    }

                    else if (numCalls == 1 && callID != call.Id)
                    {
                       try
                       {
                           this.BeginInvoke((Action)(() =>
                           {
                               call.Finish();
                           }));
                       }
                       catch (InvalidCastException e)
                       {
                       }
                    }
                }
                // If this call is from a SkypeIn/Online number, show which one.
                else if (call.Type == TCallType.cltIncomingP2P && status == TCallStatus.clsInProgress && call.Id == callID)
                {
                    selectedSkypeChatUser = call.PartnerHandle.ToString();

                    this.BeginInvoke((Action)(() => 
                    { 
                        skype.SendMessage(selectedSkypeChatUser, Properties.Settings.Default.controllerMessage);
                    }));
                    
                    chatUserSelected = true;

                    user_combobox.ResetText();
                    user_combobox.SelectedText = call.PartnerHandle.ToString();

                    if (!skypeUserTabs.Contains(new SkypeUserTab(call.PartnerHandle)))
                    {
                        user_combobox.Items.Add(call.PartnerHandle);
                        var newTabUser = new SkypeUserTab(call.PartnerHandle);
                        skypeUserTabs.Add(newTabUser);
                        tabUsers.Controls.Add(newTabUser.tabPage);
                    }
                
                }

                else if (status == TCallStatus.clsFinished && call.Id == callID)
                {
                    numCalls--;
                    callID = 0;
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}
