using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator {
	/*
		An utility class for logging messages.
		Abstracts away what we log to, so replacing all print statements later will be easier if we change how it works.
	*/
	public static class Logger {

		/*
			The output target for the print.
		*/
		public enum PrintOutput {
			Console, Unity, File,
		}

		// The output target for the print.
		public static PrintOutput Output = PrintOutput.Unity;


		/*
			Print the data in the desired output.
		*/
		public static void Log(object obj) => Log(obj.ToString());

		/*
			Print the data in the desired output.
		*/
		public static void Log(string txt) {
			switch(Output) {
				case PrintOutput.Console: LogToConsole(txt); break;
				case PrintOutput.Unity:   LogToUnity(txt);   break;
				case PrintOutput.File:    LogToFile(txt);    break;
			}
		}

		/*
			Print to the terminal.
		*/
		private static void LogToConsole(string txt) {
			Console.WriteLine(txt);
		}

		/*
			Print to the unity console.
		*/
		private static void LogToUnity(string txt) {
			Debug.Log(txt);
		}

		/*
			Print to the log file.
			[NOT IMPLEMENTED YET]
		*/
		private static void LogToFile(string txt) {
			throw new NotImplementedException();
		}

	}
}
