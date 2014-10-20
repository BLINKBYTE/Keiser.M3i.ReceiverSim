/*using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

namespace Keiser.M3i.ReceiverSimulator
{
	public partial class MainForm : Form
	{
		private Relay relay;
		private Log log;

		public MainForm ()
		{
			InitializeComponent ();
			log = new Log (this.outputBox);
			relay = new Relay (log);
		}

		private void setRunState ()
		{
			if (relay.running) {
				this.threadToggleButton.Text = "Stop";
				this.statusBarCurrentState.BackColor = System.Drawing.Color.Green;
				this.statusBarCurrentState.Text = "Running";
			} else {
				this.threadToggleButton.Text = "Start";
				this.statusBarCurrentState.BackColor = System.Drawing.Color.Red;
				this.statusBarCurrentState.Text = "Stopped";
			}
		}

		private void DisableControls (Control con)
		{
			foreach (Control c in con.Controls) {
				DisableControls (c);
			}
			if (con is CheckBox || con is TextBox)
				con.Enabled = false;
		}

		private void EnableControls (Control con)
		{
			foreach (Control c in con.Controls) {
				EnableControls (c);
			}
			if (con is CheckBox || con is TextBox)
				con.Enabled = true;
		}

		private void statusBarSaveButton_Click (object sender, EventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog ();

			saveFileDialog.Filter = "txt files (*.txt)|*.txt|log files (*.log)|*.log";
			saveFileDialog.FilterIndex = 1;
			saveFileDialog.RestoreDirectory = true;

			if (saveFileDialog.ShowDialog () == DialogResult.OK) {
				using (StreamWriter sw = new StreamWriter (saveFileDialog.FileName))
					sw.Write (log.get ());
			}
		}

		private void statusBarClearButton_Click (object sender, EventArgs e)
		{
			log.clear ();
		}

		private void toolStripStatusLabel1_Click (object sender, EventArgs e)
		{
			ProcessStartInfo sInfo = new ProcessStartInfo ("http://manuals.keiser.com/downloads/m3relay/Keiser_M3_Relay_Transmission.pdf");
			Process.Start (sInfo);
		}

	}

	public class Log
	{
		private string _log = "";
		private ListBox _outputBox;
		Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

		public Log (ListBox realBox)
		{
			_outputBox = realBox;
		}

		public string get ()
		{
			return _log;
		}

		public void add (string message, bool timeEncode = false)
		{
			if (!dispatcher.CheckAccess ()) {
				dispatcher.BeginInvoke (DispatcherPriority.Normal, (ThreadStart)delegate() {
					add (message, timeEncode);
				});
			} else {
				if (timeEncode)
					message = "[ " + message + ": " + DateTime.Now + " ]";
				_outputBox.Items.Add (message);
				_outputBox.SelectedIndex = _outputBox.Items.Count - 1;
				_log += message + "\n";
				checkLog ();
			}
		}

		public void clear ()
		{
			_outputBox.Items.Clear ();
			_log = "";
		}

		public void checkLog ()
		{
			int logLength = 1000000;
			if (_log.Length > logLength * 1.5) {
				char[] chars = new char[logLength];
				_log.CopyTo (_log.Length - logLength, chars, 0, logLength);
				_log = chars.ToString ();
			}
		}

	}
}
*/