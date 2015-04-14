using System;
using System.Net;
using System.Linq;
using System.Drawing;
using System.Xml.Linq;
using gma.System.Windows;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using System.Xml;
using System.IO;
using System.Text;

namespace GlobalHookDemo
{
    class MainForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Label labelMousePosition;
        private System.Windows.Forms.TextBox textBox;

        public MainForm()
        {
            InitializeComponent();
        }

        // THIS METHOD IS MAINTAINED BY THE FORM DESIGNER
        // DO NOT EDIT IT MANUALLY! YOUR CHANGES ARE LIKELY TO BE LOST
        void InitializeComponent()
        {
            this.textBox = new System.Windows.Forms.TextBox();
            this.labelMousePosition = new System.Windows.Forms.Label();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox.Font = new System.Drawing.Font("Courier New", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.textBox.Location = new System.Drawing.Point(4, 55);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox.Size = new System.Drawing.Size(322, 340);
            this.textBox.TabIndex = 3;
            // 
            // labelMousePosition
            // 
            this.labelMousePosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMousePosition.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelMousePosition.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelMousePosition.Location = new System.Drawing.Point(4, 29);
            this.labelMousePosition.Name = "labelMousePosition";
            this.labelMousePosition.Size = new System.Drawing.Size(322, 23);
            this.labelMousePosition.TabIndex = 2;
            this.labelMousePosition.Text = "labelMousePosition";
            this.labelMousePosition.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonStop
            // 
            this.buttonStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStop.Location = new System.Drawing.Point(85, 3);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 1;
            this.buttonStop.Text = "Stop";
            this.buttonStop.Click += new System.EventHandler(this.ButtonStopClick);
            // 
            // buttonStart
            // 
            this.buttonStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStart.Location = new System.Drawing.Point(4, 3);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.Click += new System.EventHandler(this.ButtonStartClick);
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(328, 398);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.labelMousePosition);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Name = "MainForm";
            this.Text = "This application captures keystrokes";
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        [STAThread]
        public static void Main(string[] args)
        {
            Application.Run(new MainForm());
        }

        void ButtonStartClick(object sender, System.EventArgs e)
        {
            actHook.Start();
        }

        void ButtonStopClick(object sender, System.EventArgs e)
        {
            actHook.Stop();
        }


        UserActivityHook actHook;
        void MainFormLoad(object sender, System.EventArgs e)
        {
            actHook = new UserActivityHook(); // crate an instance with global hooks
            // hang on events
            actHook.OnMouseActivity += new MouseEventHandler(MouseMoved);
            actHook.KeyDown += new KeyEventHandler(MyKeyDown);
            actHook.KeyPress += new KeyPressEventHandler(MyKeyPress);
            actHook.KeyUp += new KeyEventHandler(MyKeyUp);
            /*get initial window ptr*/
            _ptrWnd = MainForm.GetWindowUnderCursor();
        }

        public void MouseMoved(object sender, MouseEventArgs e)
        {
            /* orig
			labelMousePosition.Text=String.Format("x={0}  y={1} wheel={2}", e.X, e.Y, e.Delta);
			if (e.Clicks>0) LogWrite("MouseButton 	- " + e.Button.ToString());
             */
        }

        public void MyKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData.ToString().Equals("LControlKey"))
            {
                IntPtr ptrWnd = MainForm.GetWindowUnderCursor();

                MainForm.stSendCtrlC(ptrWnd);
                if (Clipboard.GetText().Equals(_buff, StringComparison.Ordinal))
                {
                    LogWrite("Same text selected " + _buff);
                    return;
                }
                if (Clipboard.ContainsText())
                {
                    _buff = Clipboard.GetText();
                    /*Log translation */
                    //LogWrite("Copy to clipboard text: " + TranslateText("input"));
                    TranslateText("input");
                   // LogWrite("Copy to clipboard text: " + _buff);
                }
            }
            LogWrite("KeyDown 	- " + e.KeyData.ToString());
        }

        public void MyKeyPress(object sender, KeyPressEventArgs e)
        {
            LogWrite("KeyPress 	- " + e.KeyChar);
        }

        public void MyKeyUp(object sender, KeyEventArgs e)
        {
            LogWrite("KeyUp 		- " + e.KeyData.ToString());
        }

        private void LogWrite(string txt)
        {
            textBox.AppendText(txt + Environment.NewLine);
            textBox.SelectionStart = textBox.Text.Length;
        }

        /*--------------------------------------------------*/

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public static void stSendCtrlC(IntPtr hWnd)
        {
            uint KEYEVENTF_KEYUP = 2;

            byte VK_CONTROL = 0x11;
            SetForegroundWindow(hWnd);
            keybd_event(VK_CONTROL, 0, 0, 0);

            keybd_event(0x43, 0, 0, 0); //Send the C key (43 is "C")

            keybd_event(0x43, 0, KEYEVENTF_KEYUP, 0);

            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);// 'Left Control Up
        }

        /*-------------------------------------------------*/

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point lpPoint);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point lpPoint);

        public static IntPtr GetWindowUnderCursor()
        {
            Point ptCursor = new Point();

            if (!(GetCursorPos(out ptCursor)))
                return IntPtr.Zero;

            return WindowFromPoint(ptCursor);
        }

        /*-------------------------------------------------*/

        /**
         * Translation functinality
         */
        public string TranslateText(string input)
        {
            string url = String.Format("http://www.babla.ru/английский-русский/{0}", input);
            WebClient webClient = new WebClient();
            webClient.Encoding = System.Text.Encoding.UTF8;
            string result = webClient.DownloadString(url);
            /* stripping <DOCTYPE...> */
            result = Regex.Replace(result, "<!DOCTYPE html>\r\n", string.Empty);

            XElement root = GetXElement(result);
            IEnumerable<XElement> address =
                from el in root.Elements("div")
                where (string)el.Attribute("class") == "quick-result-section"
                select el;
            foreach (XElement el in address)
                //Console.WriteLine(el);
                LogWrite("xml line" + el);

            //return result;
            return null;
        }

        private XElement GetXElement(string result)
        {
            XDocument xDoc = XDocument.Parse(result);
            return xDoc.Root;
        }

        string _buff = null;
        IntPtr _ptrWnd;
    }
}
