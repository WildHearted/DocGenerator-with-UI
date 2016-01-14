﻿using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;

// Reference sources:
// https://msdn.microsoft.com/en-us/library/office/ff478255.aspx (Baic Open XML Documents)
// https://msdn.microsoft.com/en-us/library/dd469465%28v=office.12%29.aspx (Examples with merging and Presentations)
// http://blogs.msdn.com/b/vsod/archive/2012/02/18/how-to-create-a-document-from-a-template-dotx-dotm-and-attach-to-it-using-open-xml-sdk.aspx (Example of creating a new document based on a .dotx template.)

// (Example to Replace text in a document) http://www.codeproject.com/Tips/666751/Use-OpenXML-to-Create-a-Word-Document-from-an-Exis
// (Structure of an oXML document) https://msdn.microsoft.com/en-us/library/office/gg278308.aspx
namespace DogGenUI
	{
	public class oxmlDocument
		{
		// Object Variables
		private const string localTemplatePath = @"DocGenerator\Templates";
		private const string localDocumentPath = @"DocGenerator\Documents";
		// Object Properties
		private string _localDocumentPath = "";
		public string LocalDocumentPath
			{
			get
				{
				return this._localDocumentPath;
				}
			private set
				{
				this._localDocumentPath = value;
				}
			}
		private string _documentFileName = "";
		public string DocumentFilename
			{
			get
				{
				return this._documentFileName;
				}
			private set
				{
				this._documentFileName = value;
				}
			}
		private string _localDocumentURI = "";
		public string LocalDocumentURI
			{
			get
				{
				return this._localDocumentURI;
				}
			private set
				{
				this._localDocumentURI = value;
				}
			}
		/// <summary>
		/// Use this method to create the new document object with which to work.
		/// It will create the new document based on the specified Tempate and Document Type. Upon creation, the LocalDocument
		/// </summary>
		/// <param name="parTemplateURL">
		/// This value must be the web URI of the template residing in the Document Templates List in SharePoint</param>
		/// <param name="parDocumentType">
		/// This value is the enumerated Document Type</param>
		/// <returns>
		/// Returns a bool with true if the creatin of the oxmlDoument object was successful and false if it failed.
		/// Validate that the bool is TRUE on return of the method.
		/// </returns>
		public bool CreateDocumentFromTemplate(string parTemplateURL, enumDocumentTypes parDocumentType)
			{
			string ErrorLogMessage = "";
			//string localTemplatePath = "";
			//string localDocumentPath = "";
			//Derive the file name of the template document
			//			Console.WriteLine(" Template URL: [{0}] \r\n" +
			//"         1         2         3         4         5         6         7         8         9        11        12        13        14        15\r\n" +
			//"12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890 \r\n" ,parTemplateURL);

			string templateFileName = parTemplateURL.Substring(parTemplateURL.LastIndexOf("/") + 1, (parTemplateURL.Length - parTemplateURL.LastIndexOf("/"))-1);

			// Check if the DocGenerator Template Directory Exist and that it is accessable
			// Configure and validate for the relevant Template
			string templateDirectory = Path.GetFullPath("\\") + localTemplatePath;
			try
				{
				if(Directory.Exists(@templateDirectory))
					{
					Console.WriteLine("The templateDirectory [" + templateDirectory + "] exist and are ready to be used.");
					}
				else
					{
					DirectoryInfo templateDirInfo = Directory.CreateDirectory(@templateDirectory);
					Console.WriteLine("The templateDirectory [" + templateDirectory + "] was created and are ready to be used.");
					}
				}
			catch(UnauthorizedAccessException exc)
				{
				ErrorLogMessage = "The current user: [" + System.Security.Principal.WindowsIdentity.GetCurrent().Name +
				"] does not have the required security permissions to access the template directory at: " + templateDirectory +
				"\r\n " + exc.Message + " in " + exc.Source;
				Console.WriteLine(ErrorLogMessage);
				return false;
				}
			catch(NotSupportedException exc)
				{
				ErrorLogMessage = "The path of template directory [" + templateDirectory + "] contains invalid characters. Ensure that the path is valid and  contains legible path characters only. \r\n " + exc.Message + " in " + exc.Source;
				Console.WriteLine(ErrorLogMessage);
				return false;
				}
			catch(DirectoryNotFoundException exc)
				{
				ErrorLogMessage = "The path of template directory [" + templateDirectory + "] is invalid. Check that the drive is mapped and exist /r/n " + exc.Message + " in " + exc.Source;
				Console.WriteLine(ErrorLogMessage);
				return false;
				}
			// Check if the template file exist in the template directory
			if( File.Exists(templateDirectory + "\\" + templateFileName))
				{
				// If the the template exist just proceed...
				Console.WriteLine("The template to use:" + templateDirectory + "\\" + templateFileName);
				}
			else
				{
				// Download the relevant template from SharePoint
				WebClient objWebClient = new WebClient();
				objWebClient.UseDefaultCredentials = true;
				//objWebClient.Credentials = CredentialCache.DefaultCredentials;
				try
					{
					objWebClient.DownloadFile(parTemplateURL, templateDirectory + "\\" + templateFileName);
					}
				catch(WebException exc)
					{
					ErrorLogMessage = "The template file could not be downloaded from SharePoint List [" + parTemplateURL + "]. " +
						"\n - Check that the template exist in SharePoint \n - that it is accessible \n - " + 
						"and that the network connection is working. \n " + exc.Message + " in " + exc.Source;
					Console.WriteLine(ErrorLogMessage);
					return false;
					}
				}
			Console.WriteLine("Template: {0} exist in directory: {1}? {2}", templateFileName, templateDirectory, File.Exists(templateDirectory + "\\" + templateFileName));

			// Check if the DocGenerator\Documents Directory exist and that it is accessable
			string documentDirectory = Path.GetFullPath("\\") + localDocumentPath;
			if(!Directory.Exists(documentDirectory))
				{
				try
					{
					Directory.CreateDirectory(@documentDirectory);
					}
				catch(UnauthorizedAccessException exc)
					{
					ErrorLogMessage = "The current user: [" + System.Security.Principal.WindowsIdentity.GetCurrent().Name +
						"] does not have the required security permissions to access the Document Directory at: " + documentDirectory +
						"\r\n " + exc.Message + " in " + exc.Source;
					Console.WriteLine(ErrorLogMessage);
					return false;
					}
				catch(NotSupportedException exc)
					{
					ErrorLogMessage = "The path of Document Directory [" + documentDirectory + "] contains invalid characters." +
						" Ensure that the path is valid and consist of legible path characters only. \r\n " + exc.Message + " in " + exc.Source;
					Console.WriteLine(ErrorLogMessage);
					return false;
					}
				catch(DirectoryNotFoundException exc)
					{
					ErrorLogMessage = "The path of Document Directory [" + documentDirectory + "] is invalid. Check that the drive is mapped and exist \r\n " + exc.Message + " in " + exc.Source;
					Console.WriteLine(ErrorLogMessage);
					return false;
					}
				}
			Console.WriteLine("The documentDirectory [" + documentDirectory + "] exist and are ready to be used.");
			// Set the object's LocalDocumentPath property
			this.LocalDocumentPath = documentDirectory;

			// Construct a name for the New Document
			string documentFilename = DateTime.Now.ToShortDateString();
			documentFilename = documentFilename.Replace("/", "-") + "_" + DateTime.Now.ToLongTimeString();
			Console.WriteLine("filename: [{0}]", documentFilename);
			documentFilename = documentFilename.Replace(":", "-");
			documentFilename = documentFilename.Replace(" ", "_");
			documentFilename = parDocumentType + "_" + documentFilename + ".docx";
			Console.WriteLine("Target filename: [{0}]", documentFilename);
			// Set the object's Filename property
			this.DocumentFilename = documentFilename;

			// Create the file based on a template.
			try
				{
				File.Copy(sourceFileName: templateDirectory + "\\" + templateFileName, destFileName: documentDirectory + "\\" + documentFilename, overwrite: true);
				}
			catch(FileNotFoundException exc)
				{
				ErrorLogMessage = "The template file: [" + templateDirectory + "\\" + templateFileName + "] does not exist. \r\n " + exc.Message + " in " + exc.Source;
				Console.WriteLine(ErrorLogMessage);
				return false;
				}
			catch(DirectoryNotFoundException exc)
				{
				ErrorLogMessage = "Either template or document directory could not be found. \r\n - Template Dir: [" + templateDirectory + "] " +
					"\r\n - Document Dir: [" + documentDirectory + "] \r\n " + exc.Message + " in " + exc.Source;
				Console.WriteLine(ErrorLogMessage);
				return false;
				}
			catch(UnauthorizedAccessException exc)
				{
				ErrorLogMessage = "The DocGenerator process doesn't have the required permissions to access the template or to create the new document. " +
					"\r\n " + exc.Message + " in " + exc.Source;
				Console.WriteLine(ErrorLogMessage);
				return false;
				}
			catch(IOException exc)
				{
				ErrorLogMessage = "An IO error occurred while attempting to copy the Template file for the new Document. \r\n " + exc.Message + " in " + exc.Source;
				Console.WriteLine(ErrorLogMessage);
				return false;
				}

			// Open the new document which is still in .dotx format to save it as a docx file
			try
				{
				WordprocessingDocument objDocument = WordprocessingDocument.Open(path: documentDirectory + "\\" + documentFilename, isEditable: true);
				// Change the document Type from .dotx to docx format.
				objDocument.ChangeDocumentType(newType: DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
				objDocument.Close();
				}
			catch(OpenXmlPackageException exc)
				{
				ErrorLogMessage = "Unable to open new Document: [" + documentDirectory + "\\" + documentFilename + "] \r\n " + exc.Message + " in " + exc.Source;
				Console.WriteLine(ErrorLogMessage);
				return false;
				}

			Console.WriteLine("Successfully created the new document: {0}", documentDirectory + "\\" + documentFilename);
			// Set the object's DocumentURI property
			this.LocalDocumentURI = documentDirectory + "\\" + documentFilename;
               return true;
			}
		}
	class oxmlWorkbook
		{
		}
	}