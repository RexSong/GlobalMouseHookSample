using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GloablMouseHookSample
{
    public partial class Form1 : Form
    {
        private MouseHook mouseHook;
        public Form1()
        {
            InitializeComponent();
            mouseHook = new MouseHook();
            mouseHook.Start();
            mouseHook.OnMouseXButton1Click += new MouseEventHandler(Event);
            mouseHook.OnMouseXButton2Click += new MouseEventHandler(Event);
        }

        private void Event(object sender, MouseEventArgs e) 
        {
            this.Text = e.Button.ToString();
            Console.WriteLine("Clicked");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            mouseHook.OnMouseXButton1Click -= new MouseEventHandler(Event);
            mouseHook.OnMouseXButton2Click -= new MouseEventHandler(Event);
            mouseHook.stop();
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            this.Text = "XButton Click again";
        }
    }
}
