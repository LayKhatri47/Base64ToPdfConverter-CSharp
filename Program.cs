using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

internal class Program
{
	static void Main(string[] args)
	{
		string outputFolder = "OutputPdf";

		// Create the output directory if it doesn't exist, and clear it if it does
		Directory.CreateDirectory(outputFolder);
		ClearOutputFolder(outputFolder);

		// Prompt user for the file path
		Console.WriteLine("Please enter the path to the file containing the xml:");
		string? filePath = Console.ReadLine();

		// Validate the file path
		if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
		{
			Console.WriteLine("File not found");
			return;
		}

		var fileData = File.ReadAllText(filePath);

		// Load XML document using XmlReader for efficiency
		using (var stringReader = new StringReader(fileData))
		using (var xmlReader = XmlReader.Create(stringReader))
		{
			var doc = XDocument.Load(xmlReader);

			// Filter attachments to only include those with MimeTypeTC containing "PDF"
			var attachments = doc.Descendants("Attachment")
								 
								 .ToList();

			// If no PDF attachments are found, notify the user and exit
			if (!attachments.Any())
			{
				Console.WriteLine("No PDF attachments found in the XML file.");
				return;
			}

			var pdfFileNames = attachments.Select(attachment =>
			{
				var attachmentData = attachment.Element("AttachmentData")?.Value ?? string.Empty;
				var pdfFileName = attachment.Element("Description")?.Value ?? "output.pdf";
				return ConvertAttachmentToFile(attachmentData, pdfFileName, outputFolder);
			}).Where(fileName => !string.IsNullOrEmpty(fileName)).ToList();

			// Output the saved PDF file names
			Console.WriteLine("PDF files have been saved to:");
			Console.WriteLine("===========================");
			pdfFileNames.ForEach(Console.WriteLine);
		}

		Console.WriteLine("Press any key to exit.");
		Console.ReadLine();
	}

	// Converts a base64 string to a PDF file and saves it to the specified output folder
	public static string ConvertAttachmentToFile(string base64String, string fileName, string outputFolder)
	{
		try
		{
			byte[] pdfBytes = Convert.FromBase64String(base64String);
			string pdfFilePath = Path.Combine(outputFolder, fileName);

			// Write the byte array to a file using FileStream for better performance
			using (var fileStream = new FileStream(pdfFilePath, FileMode.Create, FileAccess.Write))
			{
				fileStream.Write(pdfBytes, 0, pdfBytes.Length);
			}

			return pdfFilePath;
		}
		catch (FormatException)
		{
			Console.WriteLine($"Failed to convert base64 string for file {fileName}. The input is not a valid Base-64 string.");
			return string.Empty;
		}
	}

	// Clears all files in the specified output folder
	public static void ClearOutputFolder(string folderPath)
	{
		if (Directory.Exists(folderPath))
		{
			foreach (var file in Directory.GetFiles(folderPath))
			{
				File.Delete(file);
			}
		}
	}
}
