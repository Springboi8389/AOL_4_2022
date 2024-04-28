﻿using aol.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace aol.Forms
{
    public partial class home_menu : Win95Theme
    {
        List<Rectangle> rects = new List<Rectangle>();

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            MoveWindow(sender, e);
        }

        private void home_menu_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void home_menu_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void home_menu_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            foreach (Rectangle r in rects)
            {
                if (r.Contains(e.Location))
                {
                    Cursor = Cursors.Hand;
                    return;
                }
            }
            Cursor = Cursors.Default;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void titleLabel_MouseMove(object sender, MouseEventArgs e)
        {
            MoveWindow(sender, e);
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void maxBtn_Click(object sender, EventArgs e)
        {

        }

        private void miniBtn_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void panel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void StartForm()
        {
            todayLabel.Invoke(new MethodInvoker(delegate
            {
                todayLabel.Text = DateTime.Now.ToString("MMMM dd, yyyy");
            }));
            titleLabel.Invoke(new MethodInvoker(delegate
            {
                titleLabel.Text = "Welcome, " + accForm.tmpUsername;
            }));
            temperatureLabel.Invoke(new MethodInvoker(delegate
            {
                temperatureLabel.Text = location.getCurrentWeather();
            }));
        }

        private void home_menu_ShownAsync(object sender, EventArgs e)
        {
            location.PositionWindow(this);

            Thread thr = new Thread(StartForm);
            thr.Start();
        }

        public home_menu()
        {
            FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();
        }

        private void mailCenterBtn_Click(object sender, EventArgs e)
        {
            mailbox mb = new mailbox();
            mb.Owner = this;
            mb.MdiParent = MdiParent;
            mb.Show();
        }

        private void YouveGotPicturesBtn_Click(object sender, EventArgs e)
        {
            pictures p = new pictures();
            p.Owner = this;
            p.MdiParent = MdiParent;
            p.Show();
        }

        private void aolChannelsBtn_Click(object sender, EventArgs e)
        {
            channels chan = new channels();
            chan.Owner = this;
            chan.MdiParent = MdiParent;
            chan.Show();
        }

        private void chatBtn_Click(object sender, EventArgs e)
        {
            chat_list cl = new chat_list();
            cl.Owner = this;
            cl.MdiParent = MdiParent;
            cl.Show();
        }

        private void mailCenterBtn_MouseHover(object sender, EventArgs e)
        {

        }

        private void YouveGotPicturesBtn_MouseHover(object sender, EventArgs e)
        {

        }

        private void aolChannelsBtn_MouseHover(object sender, EventArgs e)
        {

        }

        private void chatBtn_MouseHover(object sender, EventArgs e)
        {

        }
    }
}
