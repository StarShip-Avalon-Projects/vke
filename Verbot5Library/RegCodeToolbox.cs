using System;
using Microsoft.Win32;
using System.Net;
using System.IO;

namespace Conversive.Verbot4
{
	/// <summary>
	/// Summary description for Toolbox.
	/// </summary>
	public class RegCodeToolbox
	{
		private RegistryKey verbotKey;
		private string regCode = "regcode";

		public RegCodeToolbox()
		{
			Registry.CurrentUser.CreateSubKey("Software\\Conversive\\Verbot");
			this.verbotKey = Registry.CurrentUser.OpenSubKey("Software\\Conversive\\Verbot", true);
		}

		public void SaveRegistrationCode(string stRegCode)
		{
			this.verbotKey.SetValue(regCode, stRegCode);
		}

		public string GetRegistrationCode()
		{
			return (string)this.verbotKey.GetValue(regCode);
		}

		public bool RegistrationCodeChecksumPassed()
		{
			bool bRet = false;

			string stRegCode = this.GetRegistrationCode();
			if(stRegCode != null && stRegCode != "")
			{
				char[] dash = {'-'};
				string[] stRegCodeParts = stRegCode.Split(dash);

				if(stRegCodeParts.Length == 4 && stRegCodeParts[0].Length == 5 && stRegCodeParts[1].Length == 5 && stRegCodeParts[2].Length == 5 && stRegCodeParts[3].Length == 5)
				{
					try
					{
						int a = Int32.Parse(stRegCodeParts[0], System.Globalization.NumberStyles.HexNumber);
						int b = Int32.Parse(stRegCodeParts[1], System.Globalization.NumberStyles.HexNumber);
						int c = Int32.Parse(stRegCodeParts[2], System.Globalization.NumberStyles.HexNumber);
						int d = Int32.Parse(stRegCodeParts[3], System.Globalization.NumberStyles.HexNumber);

						int d2 = 1997 ^ a ^ b ^ c;
						if(d == d2)
							bRet = true;
						else//if checksum failed delete the reg code
						{
							this.verbotKey.DeleteValue(regCode);
						}
					}
					catch{}
				}	
			}																																		
				
			return bRet;
		}

		public void ConnectToRegistrationCodeServerSync()
		{
			string stRegCode = this.GetRegistrationCode();
			try
			{
				//For using untrusted SSL Certificates
				System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();

				string stURI = "https://update.conversive.com/registration/is_key_valid.php?key=" + stRegCode + "&prodid=1";
				System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(stURI);

				System.Net.HttpWebResponse res = (HttpWebResponse)req.GetResponse();

				// Gets the stream associated with the response.
				Stream receiveStream = res.GetResponseStream();
				System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
				// Pipes the stream to a higher level stream reader with the required encoding format. 
				StreamReader readStream = new StreamReader( receiveStream, encode );
				char[] chars = new char[1];
				readStream.Read(chars, 0, 1);

				if(chars[0] == '0')//if we got a false response from the server, let's clear out their registration code
				{
					this.verbotKey.DeleteValue(regCode);
				}
			}
			catch{}
		}

		public void ValidateRegistrationCodeAgainstServer()
		{
			System.Threading.Thread regCodeThread = new System.Threading.Thread(new System.Threading.ThreadStart(ConnectToRegistrationCodeServerSync));
			regCodeThread.IsBackground = true;
			regCodeThread.Start();
		}
	}

	public class TrustAllCertificatePolicy : System.Net.ICertificatePolicy
	{
		public TrustAllCertificatePolicy()
		{}

		public bool CheckValidationResult(ServicePoint sp,
			System.Security.Cryptography.X509Certificates.X509Certificate cert,WebRequest req, int problem)
		{
			return true;
		}
	}//class TrustAllCertificatePolicy
}
