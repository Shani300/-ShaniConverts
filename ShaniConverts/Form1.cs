using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using Microsoft.Win32;

namespace ShaniConverts
{
    public partial class Form1 : Form
    {
        private IKeyboardMouseEvents _globalHook;

        // מילוני ההמרה
        private readonly Dictionary<char, char> _enToHe = CreateMapping("qwertyuiop[]asdfghjkl;'zxcvbnm,./", "/'קראטוןםפ][שדגכעיחלךף,זסבהנמצתץ.");
        private readonly Dictionary<char, char> _enToHeCaps = CreateMapping("QWERTYUIOP[]ASDFGHJKL;'ZXCVBNM,./", "/'קראטוןםפ][שדגכעיחלךף,זסבהנמצתץ.");
        private readonly Dictionary<char, char> _heToEn = CreateMapping("/'קראטוןםפ][שדגכעיחלךף,זסבהנמצתץ.", "qwertyuiop[]asdfghjkl;'zxcvbnm,./");

        public Form1()
        {
            InitializeComponent();
            SetStartup();

            // עיצוב החלון
            this.Text = "Shani Converts - רץ ברקע";
            this.MaximizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size(700, 300);
            this.BackgroundImage = null;

            // הפעלת ההאזנה
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += OnKeyDown;
            _globalHook.MouseDownExt += OnMouseDown;
        }

        private void SetStartup()
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rk.SetValue("ShaniConverts", Application.ExecutablePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Registry error: " + ex.Message);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.Shift && e.KeyCode == Keys.A)
            {
                e.Handled = true;
                Task.Run(() => ConvertAllText());
            }
            else if (e.Control && e.Shift && e.KeyCode == Keys.Z)
            {
                e.Handled = true;
                Task.Run(() => ConvertSelectedText());
            }
        }

        private void OnMouseDown(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                e.Handled = true;
                Task.Run(() => ExecuteConversionCore());
            }
        }

        // פתרון בעיית ההתנגשות: המתנה עד לשחרור המקשים הפיזיים
        private void WaitForModifiersRelease()
        {
            while ((Control.ModifierKeys & (Keys.Control | Keys.Shift)) != Keys.None)
            {
                Thread.Sleep(10);
            }
            Thread.Sleep(20); // וידוא סופי לאחר השחרור
        }

        private void ConvertAllText()
        {
            try
            {
                WaitForModifiersRelease();
                SendKeys.SendWait("^a");
                Thread.Sleep(80);
                ExecuteConversionCore();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void ConvertSelectedText()
        {
            try
            {
                WaitForModifiersRelease();
                ExecuteConversionCore();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void ExecuteConversionCore()
        {
            try
            {
                string oldClipboard = "";

                // חילוץ וניקוי הלוח הישן בצורה בטוחה (STA Thread)
                this.Invoke((MethodInvoker)delegate
                {
                    oldClipboard = Clipboard.ContainsText() ? Clipboard.GetText() : "";
                    Clipboard.Clear();
                });

                // העתקה מובטחת - ללא חסימת מערכת כי אנו רצים ב-Task
                SendKeys.SendWait("^c");
                Thread.Sleep(100);

                string text = "";
                this.Invoke((MethodInvoker)delegate
                {
                    if (Clipboard.ContainsText()) text = Clipboard.GetText();
                });

                if (string.IsNullOrEmpty(text)) return;

                string converted = ProcessText(text);

                this.Invoke((MethodInvoker)delegate
                {
                    Clipboard.SetText(converted);
                });

                Thread.Sleep(50);
                SendKeys.SendWait("^v");

                // שחזור הלוח הישן בבידוד
                if (!string.IsNullOrEmpty(oldClipboard))
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(400);
                        if (this.IsHandleCreated)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                try { Clipboard.SetText(oldClipboard); }
                                catch { /* התעלמות משגיאות אם הלוח ננעל על ידי יישום אחר */ }
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private string ProcessText(string input)
        {
            bool hasHebrew = input.Any(c => c >= 'א' && c <= 'ת');
            char[] result = new char[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (hasHebrew)
                {
                    result[i] = _heToEn.ContainsKey(c) ? _heToEn[c] : c;
                }
                else
                {
                    if (_enToHe.ContainsKey(c))
                    {
                        result[i] = _enToHe[c];
                    }
                    else if (_enToHeCaps.ContainsKey(c))
                    {
                        result[i] = _enToHeCaps[c];
                    }
                    else
                    {
                        result[i] = c;
                    }
                }
            }
            return new string(result);
        }

        private static Dictionary<char, char> CreateMapping(string from, string to)
        {
            var dict = new Dictionary<char, char>();
            for (int i = 0; i < from.Length; i++)
            {
                if (i < to.Length) dict[from[i]] = to[i];
            }
            return dict;
        }

        private void Form1_Load(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void button1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label2_Click_1(object sender, EventArgs e) { }
        private void footer_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_globalHook != null)
            {
                _globalHook.KeyDown -= OnKeyDown;
                _globalHook.MouseDownExt -= OnMouseDown;
                _globalHook.Dispose();
            }
            base.OnFormClosing(e);
        }
    }
}