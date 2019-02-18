﻿using aol.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aol.Forms
{
    public partial class chatroom : Form
    {
        #region DLLImports
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);
        #endregion

        #region win95_theme
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private const int cGrip = 16;
        private const int cCaption = 32;

        private const int
            HTLEFT = 10,
            HTRIGHT = 11,
            HTTOP = 12,
            HTTOPLEFT = 13,
            HTTOPRIGHT = 14,
            HTBOTTOM = 15,
            HTBOTTOMLEFT = 16,
            HTBOTTOMRIGHT = 17;

        int wndX = 0;
        int wndY = 0;
        int wndWidth = 0;
        int wndHeight = 0;
        public bool maximized = false;

        void maxiMini()
        {
            if (maximized)
            {
                this.Location = new Point(wndX, wndY);
                this.Width = wndWidth;
                this.Height = wndHeight;
                maximized = false;
            }
            else
            {
                wndX = this.Location.X;
                wndY = this.Location.Y;
                wndWidth = this.Width;
                wndHeight = this.Height;
                maximized = true;
                this.Location = new Point(0, 101);
                this.Width = Parent.Width - 4;
                this.Height = Parent.Height - 105;
            }
        }

        const int _ = 2;

        Rectangle Top { get { return new Rectangle(0, 0, this.ClientSize.Width, _); } }
        Rectangle Left { get { return new Rectangle(0, 0, _, this.ClientSize.Height); } }
        Rectangle Bottom { get { return new Rectangle(0, this.ClientSize.Height - _, this.ClientSize.Width, _); } }
        Rectangle Right { get { return new Rectangle(this.ClientSize.Width - _, 0, _, this.ClientSize.Height); } }

        Rectangle TopLeft { get { return new Rectangle(0, 0, _, _); } }

        Rectangle TopRight { get { return new Rectangle(this.ClientSize.Width - _, 0, _, _); } }
        Rectangle BottomLeft { get { return new Rectangle(0, this.ClientSize.Height - _, _, _); } }

        Rectangle BottomRight { get { return new Rectangle(this.ClientSize.Width - _, this.ClientSize.Height - _, _, _); } }

        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);

            if (message.Msg == 0x84)
            {
                var cursor = this.PointToClient(Cursor.Position);

                if (TopLeft.Contains(cursor)) message.Result = (IntPtr)HTTOPLEFT;
                else if (TopRight.Contains(cursor)) message.Result = (IntPtr)HTTOPRIGHT;
                else if (BottomLeft.Contains(cursor)) message.Result = (IntPtr)HTBOTTOMLEFT;
                else if (BottomRight.Contains(cursor)) message.Result = (IntPtr)HTBOTTOMRIGHT;

                else if (Top.Contains(cursor)) message.Result = (IntPtr)HTTOP;
                else if (Left.Contains(cursor)) message.Result = (IntPtr)HTLEFT;
                else if (Right.Contains(cursor)) message.Result = (IntPtr)HTRIGHT;
                else if (Bottom.Contains(cursor)) message.Result = (IntPtr)HTBOTTOM;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Gray, Top);
            e.Graphics.FillRectangle(Brushes.Gray, Left);
            e.Graphics.FillRectangle(Brushes.Gray, Right);
            e.Graphics.FillRectangle(Brushes.Gray, Bottom);
        }
        #endregion

        string chatlog = "";
        string roomname = "";

        #region winform_functions
        public chatroom(string channel)
        {
            chat.pChat = channel;
            roomname = channel;
            string logpath = Application.StartupPath + @"\chatlogs";
            chatlog = logpath + @"\" + channel + ".txt";
            
            if (!Directory.Exists(logpath))
                Directory.CreateDirectory(logpath);
            if (!File.Exists(chatlog))
                File.Create(chatlog).Dispose();

            while (!chat.irc.IsClientRunning())
            {
                Debug.WriteLine("not connected yet");
                Thread.Sleep(1000); // wait 1 sec
            }
            chat.irc.SendRawMessage("join #" + channel);

            InitializeComponent();
        }

        private void chatroom_Shown(object sender, EventArgs e)
        {
            //chat.users.Clear();
            Text = chat.pChat + " Chatroom";
            mainTitle.Text = chat.pChat + " Chatroom";
            if (!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
        }

        private void writeFileToBox(bool init = false)
        {
            string lastLine = "";
            chatRoomTextBox.Invoke(new MethodInvoker(delegate
            {
                using (FileStream file = new FileStream(chatlog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(file))
                    {
                        while (!sr.EndOfStream)
                        {
                            if (init)
                                chatRoomTextBox.AppendText(sr.ReadLine() + Environment.NewLine);
                            else
                                lastLine = sr.ReadLine();
                        }
                    }
                }
                if (!init)
                    chatRoomTextBox.AppendText(lastLine + Environment.NewLine);
                chatRoomTextBox.ScrollToCaret();
            }));
        }

        public void OnChanged(object source, FileSystemEventArgs e)
        {
            writeFileToBox();
        }

        public void keepReading()
        {
            var watch = new FileSystemWatcher();
            watch.Path = Path.GetDirectoryName(chatlog);
            watch.Filter = Path.GetFileName(chatlog);
            watch.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            watch.Changed += new FileSystemEventHandler(OnChanged);
            watch.EnableRaisingEvents = true;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            writeFileToBox(true);
            keepReading();

            // keep users list up to date
            while (true)
            {
                try
                {
                    usersListView.Invoke(new MethodInvoker(delegate
                    {
                        if (!chat.users.ContainsKey(roomname))
                        {
                            Debug.WriteLine("chat.users key [" + roomname + "] doesn't exist");
                            return;
                        }
                        // remove offline users
                        foreach (ListViewItem item in usersListView.Items)
                        {
                            if (!chat.users[roomname].Contains(item.Text))
                                usersListView.Items.Remove(item);
                        }
                        // add online users
                        List<string> usersList = chat.users[roomname]; // gotta declare it, so we can use it
                        for (int i = 0; i < usersList.Count; i++)
                        {
                            if (!usersListView.Items.ContainsKey(usersList[i]))
                            {
                                ListViewItem lvi = new ListViewItem();
                                lvi.Text = usersList[i];
                                lvi.Tag = usersList[i];
                                lvi.Name = usersList[i];
                                usersListView.Items.Add(lvi);
                            }
                        }
                    }));
                } catch { }
                Thread.Sleep(5000);
            }
        }

        private void usersListView_DoubleClick(object sender, EventArgs e)
        {
            instant_message im = new instant_message(usersListView.SelectedItems[0].Text.ToString());
            im.Owner = this;
            im.MdiParent = MdiParent;
            im.Tag = usersListView.SelectedItems[0].Text.ToString();
            im.Show();
        }

        private void chatroom_FormClosing(object sender, FormClosingEventArgs e)
        {
            chat.irc.SendRawMessage("leave #" + chat.pChat);
        }

        private void messageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                sendBtn.PerformClick();
        }

        private void chatroom_Load(object sender, EventArgs e)
        {

        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            chat.irc.SendMessageToChannel(messageTextBox.Text, "#" + chat.pChat);
            // write to file
            string logpath = Application.StartupPath + @"\chatlogs";
            string privateLog = logpath + @"\" + chat.pChat + ".txt";
            File.AppendAllText(privateLog, accounts.tmpUsername + ": " + messageTextBox.Text + '\n');
            messageTextBox.Clear();
        }

        private void miniBtn_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void panel1_DoubleClick(object sender, EventArgs e)
        {
            maxiMini();
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void maxBtn_Click(object sender, EventArgs e)
        {
            maxiMini();
        }
        #endregion
    }
}
