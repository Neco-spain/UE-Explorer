﻿using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace UEExplorer.UI.Dialogs
{
	public partial class AboutForm : Form
	{
		public AboutForm()
		{	
			InitializeComponent();
			Text = "About " + Application.ProductName;
		}	

		private void AboutForm_Load( object sender, EventArgs e )
		{
			label4.Text = Application.ProductName;
		 	VersionLabel.Text = "Version " + ProgramForm.Version;
			CopyrightLabel.Text = AssemblyCopyright;
			LinkLabel.Text = Program.WEBSITE_URL;		
			InitializeDonators();
		}

		private const string DONATORS_URL = "http://eliotvu.com/files/donators.txt"; 
		private void InitializeDonators()
		{
			var buffer = new StreamReader( Program.ReadRemoteFile( DONATORS_URL ) );
			buffer.BaseStream.Position = 0;

			DonatorsSet.ReadXml( buffer );
			DonatorsGrid.AutoGenerateColumns = true;
			DonatorsGrid.DataSource = DonatorsSet;
			DonatorsGrid.DataMember = "Donators";


			DonateLink.Text = Program.Donate_URL;		
		}

		private string AssemblyCopyright
		{
			get
			{
				var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyCopyrightAttribute ), false );
				return attributes.Length == 0 ? "" : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}
		}

		private void LicenseLink_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
		{
			System.Diagnostics.Process.Start( "file:///" + Path.Combine( Application.StartupPath, "license.html" ) );
		}
	}
}
