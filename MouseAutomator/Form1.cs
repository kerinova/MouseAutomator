using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace MouseAutomator
{
    public partial class Form1 : Form
    {
        //import statements for mouse output
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        //import statements for registering and unregistering keyboard hotkeys
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private const int HOTKEY_RIGHT_ID = 1; //hotkey for right mouse click
        private const int HOTKEY_STOP_ID = 2; //hotkey for stopping playback


        private List<Point> positions = new List<Point>(); //list of mouse positions to click
        private bool isLooping = true; //if playback is ongoing
        private int globalDelay = 250; //global delay between every action, can be customized by user
        private string fileName = ""; //name of current working file
        private Thread loopThread;

        public Form1()
        {
            InitializeComponent();
            RegisterHotKey(this.Handle, HOTKEY_RIGHT_ID, 1, (int)Keys.Z);
            RegisterHotKey(this.Handle, HOTKEY_STOP_ID, 4, (int)Keys.Z);
            txtInstructions.Text =
@"Hotkeys:
Record position to click: Alt + Z
Stop play loop: Shift + Z
";
        }

        /// <summary>
        /// Handles key press events
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == HOTKEY_RIGHT_ID) //Key to save a mouse click
            {
                SaveMousePosition(Cursor.Position.X, Cursor.Position.Y);
            }
            else if (m.Msg == 0x0312 && m.WParam.ToInt32() == HOTKEY_STOP_ID) //Key to stop looping
            {
                isLooping = false;
                loopThread.Abort();
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Saves the current position of the mouse adding it to the list and to the history.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void SaveMousePosition(int x, int y)
        {
            positions.Add(new Point(x, y, 0));
            txtPositions.Text += $"X: {x} + Y: {y}" + Environment.NewLine;
        }

        /// <summary>
        /// Adds a pause of given length provided by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddPause_Click(object sender, EventArgs e)
        {
            int delay = (int)numDelay.Value;
            Point oldPreviousPoint = positions[positions.Count - 1]; //appends pause to the previous point to be used after the appended point but before the next point.
            positions[positions.Count - 1] = new Point(oldPreviousPoint.X, oldPreviousPoint.Y, delay);
            txtPositions.Text += $"Delay: {delay} seconds" + Environment.NewLine;
        }

        /// <summary>
        /// Begins looping the recorded procedure on a different thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Click(object sender, EventArgs e)
        {
            isLooping = true;
            loopThread = new Thread(MouseOperationLoop);
            loopThread.Start();
        }

        /// <summary>
        /// Loops the actions in the list
        /// </summary>
        private void MouseOperationLoop()
        {
            while(isLooping)
            {
                foreach (Point point in positions)
                {
                    DoMouseClick(point.X, point.Y);
                    Thread.Sleep(point.Delay * 1000); //if there is a delay sleep, if not the sum will be 0
                }
            }
        }

        /// <summary>
        /// Perform the mouse event and sleep for the global delay.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void DoMouseClick(int x, int y)
        {
            uint X = (uint)x;
            uint Y = (uint)y;
            Cursor.Position = new System.Drawing.Point(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
            Thread.Sleep(globalDelay);
        }
        #region File Management
        private async void SaveFile()
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                await writer.WriteLineAsync(globalDelay.ToString()); //writes the global delay for this program
                foreach (Point point in positions)
                {
                    await writer.WriteLineAsync(point.X + "," + point.Y + "," + point.Delay);
                }
                writer.Flush();
                writer.Close();
            };
        }

        private async void LoadFile()
        {
            using (StreamReader reader = new StreamReader(ofd.OpenFile()))
            {
                globalDelay = int.Parse(await reader.ReadLineAsync());
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    string[] components = line.Split(new char[] { ',' });
                    int delay = int.Parse(components[2]);
                    positions.Add(new Point(int.Parse(components[0]), int.Parse(components[1]), delay));
                    txtPositions.Text += $"X: {components[0]} + Y: {components[1]}" + Environment.NewLine;
                    if(delay > 0)
                    {
                        txtPositions.Text += $"Delay: {delay} seconds" + Environment.NewLine;
                    }
                }
                reader.Close();
            };
        }

        SaveFileDialog sfd;
        OpenFileDialog ofd;
        private void SaveAs()
        {
            sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = ".map";
            sfd.FileName = "Procedure";
            sfd.Filter = "Procedure Files (*.map)|*.map|All files (*.*)|*.*";
            sfd.OverwritePrompt = true;
            sfd.Title = "Save procedure file";
            sfd.FileOk += Sfd_FileOk;
            sfd.ShowDialog();
        }

        private void Sfd_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            fileName = sfd.FileName;
            base.Text += " " + sfd.FileName;
            SaveFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(fileName))
            {
                SaveAs();
            }
            else
            {
                SaveFile();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ofd = new OpenFileDialog();
            ofd.AddExtension = true;
            ofd.DefaultExt = ".map";
            ofd.Filter = "Procedure Files (*.map)|*.map|All files (*.*)|*.*";
            ofd.Title = "Open procedure file";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Multiselect = false;
            ofd.FileOk += Ofd_FileOk;
            ofd.ShowDialog();
        }

        private void Ofd_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            fileName = ofd.FileName;
            base.Text += " " + ofd.FileName;
            LoadFile();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtPositions.Text = "";
            positions = new List<Point>();
            base.Text = "Kerinova Mouse Animator";
            globalDelay = 0;
        }
        #endregion

        /// <summary>
        /// Opens the settings menu for the global delay settinng and updates with the returned global delay time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void globalDelayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GlobalDelaySettings globalDelaySettings = new GlobalDelaySettings(globalDelay);
            DialogResult dr = globalDelaySettings.ShowDialog(this);
            if(dr == DialogResult.OK)
            {
                globalDelay = globalDelaySettings.GlobalDelay;
            }
        }
    }
}