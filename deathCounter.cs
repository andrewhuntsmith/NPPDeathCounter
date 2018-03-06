//int levelID : 0x0004B698, 0xC, 0x88, 0x1C, 0xA0;
//int resets: "npp.dll", 0xE914C4;
//int deaths: "npp.dll", 0xE91414;

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public class MemoryRead
{
	// offsets from npp.dll to find resets and deaths
	public static int resetOffset = (int)0xE914C4;
	public static int deathOffset = (int)0xE91414;

	// variables that store memory access information
	public static Process nppProcess;
	public static IntPtr nppProcessHandle;
	public static ProcessModule nppProcessModule;
	public static ProcessModuleCollection nppProcessModuleCollection;
	public static IntPtr nppdllBaseAddress;
	
	// labels for the main window
	public static Label totalDeathCount;
	public static Label totalDeathLabel;
	public static Label todayDeathCount;
	public static Label todayDeathLabel;
	public static Label totalResetCount;
	public static Label totalResetLabel;
	public static Label todayResetCount;
	public static Label todayResetLabel;
	public static Label CurRunResetCount;
	public static Label CurRunResetLabel;
	public static Label CurRunDeathCount;
	public static Label CurRunDeathLabel;
	public static Button resetCountsButton;
	public static Button quitButton;
	public static Button optionsButton;
	
	public static ComboBox numStats;
	public static ComboBox[] statsArray;
	
	public static Form form;
	
	public static int resets;		// total resets in user's save
	public static int deaths;		// total deaths in user's save
	public static int resetsToday;	// total resets during current instance of application
	public static int deathsToday;	// total deaths during current instance of application
	public static int resetsCurRun;	// total resets during current run, can be reset at any time by button
	public static int deathsCurRun; // total deaths during current run, can be reset at any time by button

	public static bool loop;
	
	
	const int PROCESS_VM_READ = 0x0010;
	
	[DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

	
	public static void Main()
	{		
		int bytesRead = 0;
		byte[] resetsBuffer = new byte[8];
		byte[] deathsBuffer = new byte[8];

		if (!FindProcessNPP()) {return;}
		
		nppProcessHandle = OpenProcess(PROCESS_VM_READ,false,nppProcess.Id);
		// if OpenProcess failed
		if (nppProcessHandle==(IntPtr)0) {
			string errorMessage = "Cannot access N++ process!";
			string caption = "Error in accessing application";
			MessageBox.Show(errorMessage,caption);
			return;
		}

		if (!FindnppModule()) {return;}
		
		ReadProcessMemory((int)nppProcessHandle, (int)(nppdllBaseAddress+resetOffset), resetsBuffer, resetsBuffer.Length, ref bytesRead);
		ReadProcessMemory((int)nppProcessHandle, (int)(nppdllBaseAddress+deathOffset), deathsBuffer, deathsBuffer.Length, ref bytesRead);
		
		resets = BitConverter.ToInt32(resetsBuffer, 0);
		deaths = BitConverter.ToInt32(deathsBuffer, 0);
		resetsToday = resets;
		deathsToday = deaths;		
		resetsCurRun = resets;
		deathsCurRun = deaths;
		
		using (form = new Form())
		{
			
			// Font myFont = new Font(form.Font.FontFamily,16);
			
			/* form.Text = "N++ Session Stats";
			form.BackColor = Color.Gray;
			form.Size = new Size(300,400); */

			totalDeathCount = new Label();
			/* totalDeathCount.Text = deaths.ToString();
			totalDeathCount.Width = 100;
            totalDeathCount.Height = 50;
            totalDeathCount.BackColor = Color.LightGray;
			totalDeathCount.Font = myFont;
			totalDeathCount.Location = new Point(200,0); */
			
			totalDeathLabel = new Label();
			/* totalDeathLabel.Text = "Total Deaths";
			totalDeathLabel.Font = myFont;
			totalDeathLabel.Height = 50;
			totalDeathLabel.Width = 200;
			totalDeathLabel.BackColor = Color.Gray;
			totalDeathLabel.Location = new Point(0,0); */
			
			totalResetCount = new Label();
			/* totalResetCount.Text = resets.ToString();
			totalResetCount.Width = 100;
            totalResetCount.Height = 50;
            totalResetCount.BackColor = Color.Gray;
			totalResetCount.Font = myFont;
			totalResetCount.Location = new Point(200,50); */
			
			totalResetLabel = new Label();
			/* totalResetLabel.Text = "Total Resets";
			totalResetLabel.Font = myFont;
			totalResetLabel.Height = 50;
			totalResetLabel.Width = 200;
			totalResetLabel.BackColor = Color.LightGray;
			totalResetLabel.Location = new Point(0,50); */
			
			todayDeathCount = new Label();
			/* todayDeathCount.Text = (deaths-deathsToday).ToString();
			todayDeathCount.Width = 100;
            todayDeathCount.Height = 50;
            todayDeathCount.BackColor = Color.LightGray;
			todayDeathCount.Font = myFont;
			todayDeathCount.Location = new Point(200,100); */
			
			todayDeathLabel = new Label();
			/* todayDeathLabel.Text = "Today's Deaths";
			todayDeathLabel.Font = myFont;
			todayDeathLabel.Height = 50;
			todayDeathLabel.Width = 200;
			todayDeathLabel.BackColor = Color.Gray;
			todayDeathLabel.Location = new Point(0,100); */
			
			todayResetCount = new Label();
			/* todayResetCount.Text = (resets-resetsToday).ToString();
			todayResetCount.Width = 100;
            todayResetCount.Height = 50;
            todayResetCount.BackColor = Color.Gray;
			todayResetCount.Font = myFont;
			todayResetCount.Location = new Point(200,150); */
			
			todayResetLabel = new Label();
			/* todayResetLabel.Text = "Today's Resets";
			todayResetLabel.Font = myFont;
			todayResetLabel.Height = 50;
			todayResetLabel.Width = 200;
			todayResetLabel.BackColor = Color.LightGray;
			todayResetLabel.Location = new Point(0,150); */
			
			CurRunDeathCount = new Label();
			/* CurRunDeathCount.Text = (deaths-deathsCurRun).ToString();
			CurRunDeathCount.Width = 100;
			CurRunDeathCount.Height = 50;
			CurRunDeathCount.BackColor = Color.LightGray;
			CurRunDeathCount.Font = myFont;
			CurRunDeathCount.Location = new Point(200,200); */
			
			CurRunDeathLabel = new Label();
			/* CurRunDeathLabel.Text = "Current Deaths";
			CurRunDeathLabel.Width = 200;
			CurRunDeathLabel.Height = 50;
			CurRunDeathLabel.BackColor = Color.Gray;
			CurRunDeathLabel.Font = myFont;
			CurRunDeathLabel.Location = new Point(0,200); */
			
			CurRunResetCount = new Label();
			/* CurRunResetCount.Text = (resets-resetsCurRun).ToString();
			CurRunResetCount.Width = 100;
			CurRunResetCount.Height = 50;
			CurRunResetCount.BackColor = Color.Gray;
			CurRunResetCount.Font = myFont;
			CurRunResetCount.Location = new Point(200,250); */
			
			CurRunResetLabel = new Label();
			/* CurRunResetLabel.Text = "Current Resets";
			CurRunResetLabel.Width = 200;
			CurRunResetLabel.Height = 50;
			CurRunResetLabel.BackColor = Color.LightGray;
			CurRunResetLabel.Font = myFont;
			CurRunResetLabel.Location = new Point(0,250); */
			
			/* resetCountsButton = new Button();
			resetCountsButton.Text = "Reset";
			resetCountsButton.BackColor = Color.LightGray;
			resetCountsButton.Location = new Point(0,320);
			resetCountsButton.Click += new System.EventHandler(resetCountsButtonClick);
			
			quitButton = new Button();
			quitButton.Text = "Quit!";
			quitButton.BackColor = Color.LightGray;
			quitButton.Location = new Point(200,320);
			quitButton.Click += new System.EventHandler(quitButtonClick);
			
			optionsButton = new Button();
			optionsButton.Text = "Options";
			optionsButton.BackColor = Color.LightGray;
			optionsButton.Location = new Point(100,320);
			optionsButton.Click += new System.EventHandler(optionsButtonClick); */
			
			makeForm();
			
			/* form.Controls.Add(totalDeathCount);
			form.Controls.Add(totalDeathLabel);
			form.Controls.Add(todayDeathCount);
			form.Controls.Add(todayDeathLabel);
			form.Controls.Add(totalResetCount);
			form.Controls.Add(totalResetLabel);
			form.Controls.Add(todayResetCount);
			form.Controls.Add(todayResetLabel);
			form.Controls.Add(CurRunDeathCount);
			form.Controls.Add(CurRunDeathLabel);
			form.Controls.Add(CurRunResetCount);
			form.Controls.Add(CurRunResetLabel); */
			/* form.Controls.Add(resetCountsButton);
			form.Controls.Add(quitButton);
			form.Controls.Add(optionsButton); */
			
			Thread t = new Thread( UpdateThread );
			t.Start();
			
			form.ShowDialog();
			
			
		}
		
	}
	
	static void UpdateThread()
	{
		
		loop = true;
		while (loop) 
		{			
			int bytesRead = 0;
			byte[] resetsBuffer = new byte[8];
			byte[] deathsBuffer = new byte[8];
			
			ReadProcessMemory((int)nppProcessHandle, (int)(nppdllBaseAddress+resetOffset), resetsBuffer, resetsBuffer.Length, ref bytesRead);
			ReadProcessMemory((int)nppProcessHandle, (int)(nppdllBaseAddress+deathOffset), deathsBuffer, deathsBuffer.Length, ref bytesRead);

			resets = BitConverter.ToInt32(resetsBuffer, 0);
			deaths = BitConverter.ToInt32(deathsBuffer, 0);
						
			totalDeathCount.Text = deaths.ToString();
			totalResetCount.Text = resets.ToString();
			todayResetCount.Text = (resets-resetsToday).ToString();
			todayDeathCount.Text = (deaths-deathsToday).ToString();
			CurRunResetCount.Text = (resets-resetsCurRun).ToString();
			CurRunDeathCount.Text = (deaths-deathsCurRun).ToString();
			
		}
	
	}
	
	static void quitButtonClick(object sender, EventArgs e)
	{
		loop = false;
		Console.WriteLine("Quit!");
		Environment.Exit(0);
	}
	
	static void resetCountsButtonClick(object sender, EventArgs e)
	{
		resetsCurRun = resets;
		deathsCurRun = deaths;
		Console.WriteLine("Reset!");
	}
	
	static void optionsButtonClick(object sender, EventArgs e)
	{
		using (Form optionsForm = new Form()) 
		{
			Font myFont = new Font(optionsForm.Font.FontFamily,16);
			
			optionsForm.Text = "Configuration Options";
			optionsForm.BackColor = Color.Gray;
			optionsForm.Size = new Size(500,600);
			
			numStats = new ComboBox();
			numStats.Location = new Point(200,0);
			numStats.Size = new Size(100,20);
			numStats.Name = "NumStats";
			numStats.Font = myFont;
			numStats.Items.AddRange(new string[]{"1","2","3","4","5","6"});
			numStats.SelectedIndex = 5; //hardcoding numbers is bad practice toaster, why are you such a shit programmer
			optionsForm.Controls.Add(numStats);
			
			Label numStatsLabel = new Label();
			numStatsLabel.Text = "Number of stats";
			numStatsLabel.Location = new Point(0,0);
			numStatsLabel.Size = new Size(200,50);
			numStatsLabel.Font = myFont;
			optionsForm.Controls.Add(numStatsLabel);
			
			statsArray = new ComboBox[] {new ComboBox(),new ComboBox(),new ComboBox(),
										new ComboBox(),new ComboBox(),new ComboBox()};
			for (int curStat = 0; curStat<6; curStat+=1) {
				Label statLabel = new Label();
				statLabel.Text = "Stat "+(curStat+1);
				statLabel.Font = myFont;
				statLabel.Location = new Point(0,(curStat*50)+100);
				statLabel.Size = new Size(200,50);
				optionsForm.Controls.Add(statLabel);
				
				statsArray[curStat].Location = new Point(200,(curStat*50)+100);
				statsArray[curStat].Items.AddRange(new string[]{"Total Deaths","Total Resets",
														"Today's Deaths","Today's Resets",
														"Current Deaths","Current Resets"});
				statsArray[curStat].Font = myFont;
				statsArray[curStat].SelectedIndex = curStat;
				statsArray[curStat].Size = new Size(200,50);
				optionsForm.Controls.Add(statsArray[curStat]);
			}
			
			Button saveChanges = new Button();
			saveChanges.Text = "Save Changes";
			saveChanges.Location = new Point(50,450);
			saveChanges.Size = new Size(200,50);
			saveChanges.Font = myFont;
			saveChanges.Click += new System.EventHandler(saveChangesButtonClick);
			optionsForm.Controls.Add(saveChanges);
			
			optionsForm.ShowDialog();
		}
	}
	
	static void saveChangesButtonClick(object sender, EventArgs e){
		var configStream = File.Open("deathCounterConfig.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
		var configWriter = new StreamWriter(configStream);
		string toBeWritten = "";
		// Console.WriteLine(numStats.SelectedItem.ToString());
		toBeWritten += numStats.SelectedItem.ToString();
		for (int curStat=0; curStat<(numStats.SelectedIndex+1); curStat+=1) {
			// Console.WriteLine(statsArray[curStat].SelectedItem);
			if (statsArray[curStat].SelectedItem.ToString() == "Total Deaths") { toBeWritten += "td"; }
			if (statsArray[curStat].SelectedItem.ToString() == "Total Resets") { toBeWritten += "tr"; }
			if (statsArray[curStat].SelectedItem.ToString() == "Today's Deaths") { toBeWritten += "sd"; }
			if (statsArray[curStat].SelectedItem.ToString() == "Today's Resets") { toBeWritten += "sr"; }
			if (statsArray[curStat].SelectedItem.ToString() == "Current Deaths") { toBeWritten += "cd"; }
			if (statsArray[curStat].SelectedItem.ToString() == "Current Resets") { toBeWritten += "cr"; }
		}
		
		configWriter.Write(toBeWritten);
		configWriter.Close();
		string errorMessage = "Restart program for changes to take effect";
		string caption = "Changes Saved!";
		MessageBox.Show(errorMessage,caption);		
		
		// makeForm(); // next goal is to update the window, not need to reset program
	}
	
	// finds the N++ process
	// returns true if process is found
	// returns false if process is not found and user quits application
	static bool FindProcessNPP() {
		while (Process.GetProcessesByName("N++").Length==0) {
			// if GetProcessesByName failed
			string errorMessage = "Cannot find application N++!\nOpen the game and click 'Yes', or give up and click 'No'.";
			string caption = "Error in finding application";
			var result = MessageBox.Show(errorMessage,caption,MessageBoxButtons.YesNo);
			if ( result == DialogResult.No ) {
				return false;
			}
		}
		nppProcess = Process.GetProcessesByName("N++")[0];
		return true;
	}
	
	// finds the base address for npp.dll
	// returns true if found
	// returns false if error
	static bool FindnppModule() {
		
		string nppdllFilePath = "npp.dll";
		nppdllBaseAddress = (IntPtr)0;
		nppProcessModuleCollection = nppProcess.Modules;
		
		//Console.WriteLine("Base addresses of modules associated with N++ are:");
		for( int i=0; i <nppProcessModuleCollection.Count; i++)
		{
			nppProcessModule = nppProcessModuleCollection[i];
			//Console.WriteLine(nppProcessModule.FileName+" : "+nppProcessModule.BaseAddress);
			if (nppProcessModule.FileName.Contains(nppdllFilePath)) {
				nppdllBaseAddress = nppProcessModule.BaseAddress;
			}
		}
		
		// if the npp.dll module was not found
		if (nppdllBaseAddress==(IntPtr)0) {
			string errorMessage = "Cannot access npp.dll module!";
			string caption = "Error in accessing memory";
			MessageBox.Show(errorMessage,caption);
			return false;
		}
		return true;
	}
	
	static void makeForm() {
		var configStream = File.Open("deathCounterConfig.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
		var configReader = new StreamReader(configStream);
		int numLabelsChar = configReader.Peek();
		Console.WriteLine(numLabelsChar);
		if (numLabelsChar == -1 || numLabelsChar < 49 || numLabelsChar > 54) {
			configReader.Close();
			var configWriter = new StreamWriter("deathCounterConfig.txt",false);
			configWriter.Write("6tdtrsdsrcdcr");
			configWriter.Close();
			configReader.Close();
			configStream = File.Open("deathCounterConfig.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			configReader = new StreamReader(configStream);
		}
		int numLabels = (int)char.GetNumericValue((char)configReader.Read());
		//Console.WriteLine(numLabels);
		
		for (int cur = 0; cur<numLabels; cur+=1){		
			char[] charBuffer = new char[2];
			configReader.Read(charBuffer,0,2);
			//Console.WriteLine(charBuffer);
			
			Font myFont = new Font(form.Font.FontFamily,16);
			Label curLabel = null;
			Label curCount = null;
			string lName = "";
			int lVar = 0;
			
			if (new string(charBuffer) == "td") {
				curLabel = totalDeathLabel; curCount = totalDeathCount;
				lName = "Total Deaths"; lVar = deaths;}
			if (new string(charBuffer) == "tr") {
				curLabel = totalResetLabel; curCount = totalResetCount;
				lName = "Total Resets"; lVar = resets;}
			if (new string(charBuffer) == "sd") {
				curLabel = todayDeathLabel; curCount = todayDeathCount;
				lName = "Today's Deaths"; lVar = deathsToday;}
			if (new string(charBuffer) == "sr") {
				curLabel = todayResetLabel; curCount = todayResetCount;
				lName = "Today's Resets"; lVar = resetsToday;}
			if (new string(charBuffer) == "cd") {
				curLabel = CurRunDeathLabel; curCount = CurRunDeathCount;
				lName = "Current Deaths"; lVar = deathsCurRun;}
			if (new string(charBuffer) == "cr") {
				curLabel = CurRunResetLabel; curCount = CurRunResetCount;
				lName = "Current Resets"; lVar = resetsCurRun;}
			
			curCount.Text = lVar.ToString();
			curCount.Width = 100;
            curCount.Height = 50;
            if (cur%2==0) {curCount.BackColor = Color.LightGray;}
			else {curCount.BackColor = Color.Gray;}
			curCount.Font = myFont;
			curCount.Location = new Point(200,50*cur);
			
			curLabel.Text = lName;
			curLabel.Font = myFont;
			curLabel.Height = 50;
			curLabel.Width = 200;
			if (cur%2==0) {curLabel.BackColor = Color.Gray;}
			else {curLabel.BackColor = Color.LightGray;}
			curLabel.Location = new Point(0,50*cur);
			
			form.Controls.Add(curLabel);
			form.Controls.Add(curCount);
		}
		
		resetCountsButton = new Button();
		resetCountsButton.Text = "Reset";
		resetCountsButton.BackColor = Color.LightGray;
		resetCountsButton.Location = new Point(0,(numLabels*50)+20);
		resetCountsButton.Click += new System.EventHandler(resetCountsButtonClick);
		
		quitButton = new Button();
		quitButton.Text = "Quit!";
		quitButton.BackColor = Color.LightGray;
		quitButton.Location = new Point(200,(numLabels*50)+20);
		quitButton.Click += new System.EventHandler(quitButtonClick);
		
		optionsButton = new Button();
		optionsButton.Text = "Options";
		optionsButton.BackColor = Color.LightGray;
		optionsButton.Location = new Point(100,(numLabels*50)+20);
		optionsButton.Click += new System.EventHandler(optionsButtonClick);
		
		form.Controls.Add(resetCountsButton);
		form.Controls.Add(quitButton);
		form.Controls.Add(optionsButton);
		
		form.Text = "N++ Session Stats";
		form.BackColor = Color.Gray;
		form.Size = new Size(300,(numLabels*50)+100);
		
		configReader.Close();
	}
	
}