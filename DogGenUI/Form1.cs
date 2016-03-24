﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Services.Client;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.SharePoint;
using DocGenerator.SDDPServiceReference;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Validation;

namespace DocGenerator
	{

	public partial class Form1 : Form
		{
		/// <summary>
		///	Declare the SharePoint connection as a DataContext
		/// </summary>
		DesignAndDeliveryPortfolioDataContext datacontexSDDP = new DesignAndDeliveryPortfolioDataContext(new
			Uri(Properties.AppResources.SharePointSiteURL + Properties.AppResources.SharePointRESTuri)); //"/_vti_bin/listdata.svc"));
		public string ErrorLogMessage = "";

		public Form1()
			{
			InitializeComponent();
			datacontexSDDP.Credentials = CredentialCache.DefaultCredentials;
			}

		private void btnSDDP_Click(object sender, EventArgs e)
			{
			Cursor.Current = Cursors.WaitCursor;
			Console.WriteLine("Checking the Document Collection Library for any documents to generate...");
			string returnResult = "";
			List<DocumentCollection> docCollectionsToGenerate = new List<DocumentCollection>();
			try
				{
				returnResult = DocumentCollection.GetCollectionsToGenerate(ref docCollectionsToGenerate);
				if (returnResult.Substring(0,4) == "Good")
					{
					Console.WriteLine("\r\nThere are {0} Document Collections to generate.", docCollectionsToGenerate.Count());
					}
				else if(returnResult.Substring(0,5) == "Error")
					{
					Console.WriteLine("\nERROR: There was an error accessing the Document Collections. \n{0}", returnResult);
					}
				}
			catch(InvalidProgramException ex)
				{
				Console.WriteLine("Exception occurred [{0}] \n Inner Exception: {1}", ex.Message, ex.InnerException);
				}
			// Continue here if there are any Document Collections to generate...

			string objectType = "";
			try
				{
				if(docCollectionsToGenerate.Count > 0)
					{
					foreach(DocumentCollection objDocCollection in docCollectionsToGenerate)
						{
						Console.WriteLine("\r\nReady to generate Document Collection: {0} - {1}", objDocCollection.ID.ToString(), objDocCollection.Title);

						// Process each of the documents in the DocumentCollection
						if(objDocCollection.Document_and_Workbook_objects.Count() > 0)
							{
							//objDocCollection.Document_and_Workbook_objects.GetType();
							foreach(dynamic objDocumentWorkbook in objDocCollection.Document_and_Workbook_objects)
								{
								Console.WriteLine("\r Generate ObjectType: {0}", objDocumentWorkbook.ToString());
								objectType = objDocumentWorkbook.ToString();
								objectType = objectType.Substring(objectType.IndexOf(".") + 1, (objectType.Length - objectType.IndexOf(".") - 1));
								switch(objectType)
									{
								case ("Client_Requirements_Mapping_Workbook"):
										{
										Client_Requirements_Mapping_Workbook objCRMworkbook = objDocumentWorkbook;
										if(objCRMworkbook.Generate())
											{
											if(objCRMworkbook.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t *** {0} error(s) occurred during the generation process.",
													objCRMworkbook.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objCRMworkbook.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								case ("Content_Status_Workbook"):
										{
										Content_Status_Workbook objcontentStatus = objDocumentWorkbook;
										if(objcontentStatus.Generate())
											{
											if(objcontentStatus.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t {0} error(s) occurred during the generation process.",
													objcontentStatus.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objcontentStatus.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								case ("Contract_SoW_Service_Description"):
										{
										Contract_SoW_Service_Description objContractSoW = objDocumentWorkbook;
										if(objContractSoW.Generate())
											{
											if(objContractSoW.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t {0} error(s) occurred during the generation process.", objContractSoW.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objContractSoW.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								case ("CSD_based_on_ClientRequirementsMapping"):
										{
										CSD_based_on_ClientRequirementsMapping objCSDbasedCRM = objDocumentWorkbook;
										if(objCSDbasedCRM.Generate())
											{
											if(objCSDbasedCRM.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t {0} error(s) occurred during the generation process.", objCSDbasedCRM.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objCSDbasedCRM.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								case ("CSD_Document_DRM_Inline"):
										{
										CSD_Document_DRM_Inline objCSDdrmInline = objDocumentWorkbook;
										if(objCSDdrmInline.Generate())
											{
											if(objCSDdrmInline.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t {0} error(s) occurred during the generation process.",
													objCSDdrmInline.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objCSDdrmInline.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								case ("CSD_Document_DRM_Sections"):
										{
										CSD_Document_DRM_Sections objCSDdrmSections = objDocumentWorkbook;
										if(objCSDdrmSections.Generate())
											{
											if(objCSDdrmSections.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t {0} error(s) occurred during the generation process.", objCSDdrmSections.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objCSDdrmSections.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								case ("External_Technology_Coverage_Dashboard_Workbook"):
										{
										External_Technology_Coverage_Dashboard_Workbook objExtTechDashboard = objDocumentWorkbook;
										if(objExtTechDashboard.Generate())
											{
											if(objExtTechDashboard.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t *** {0} error(s) occurred during the generation process.",
													objExtTechDashboard.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objExtTechDashboard.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								case ("Internal_Technology_Coverage_Dashboard_Workbook"):
										{
										Internal_Technology_Coverage_Dashboard_Workbook objIntTechDashboard = objDocumentWorkbook;
										if(objIntTechDashboard.Generate())
											{
											if(objIntTechDashboard.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t *** {0} error(s) occurred during the generation process.", objIntTechDashboard.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objIntTechDashboard.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								case ("ISD_Document_DRM_Inline"):
										{
										ISD_Document_DRM_Inline objISDdrmInline = objDocumentWorkbook;
										if(objISDdrmInline.Generate())
											{
											if(objISDdrmInline.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t *** {0} error(s) occurred during the generation process.",
													objISDdrmInline.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objISDdrmInline.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								case ("ISD_Document_DRM_Sections"):
										{
										ISD_Document_DRM_Sections objISDdrmSections = objDocumentWorkbook;
										if(objISDdrmSections.Generate())
											{
											if(objISDdrmSections.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t *** {0} error(s) occurred during the generation process.",
													objISDdrmSections.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objISDdrmSections.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								case ("Pricing_Addendum_Document"):
										{
										Pricing_Addendum_Document objPricingAddendum = objDocumentWorkbook;
										if(objPricingAddendum.Generate())
											{
											if(objPricingAddendum.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t *** {0} error(s) occurred during the generation process.",
													objPricingAddendum.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objPricingAddendum.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								case ("RACI_Matrix_Workbook_per_Deliverable"):
										{
										RACI_Matrix_Workbook_per_Deliverable objRACImatrix = objDocumentWorkbook;
										if(objRACImatrix.Generate())
											{
											if(objRACImatrix.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t *** {0} error(s) occurred during the generation process.",
													objRACImatrix.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objRACImatrix.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								case ("RACI_Workbook_per_Role"):
										{
										RACI_Workbook_per_Role objRACIperRole = objDocumentWorkbook;
										if(objRACIperRole.Generate())
											{
											if(objRACIperRole.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t *** {0} error(s) occurred during the generation process.",
													objRACIperRole.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objRACIperRole.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								case ("Services_Framework_Document_DRM_Inline"):
										{
										Services_Framework_Document_DRM_Inline objSFdrmInline = objDocumentWorkbook;
										if(objSFdrmInline.Generate())
											{
											if(objSFdrmInline.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t *** {0} error(s) occurred during the generation process.",
													objSFdrmInline.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objSFdrmInline.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								case ("Services_Framework_Document_DRM_Sections"):
										{
										Services_Framework_Document_DRM_Sections objSFdrmSections = objDocumentWorkbook;
										if(objSFdrmSections.Generate())
											{
											if(objSFdrmSections.ErrorMessages.Count() > 0)
												{
												Console.WriteLine("\t *** {0} error(s) occurred during the generation process.",
													objSFdrmSections.ErrorMessages.Count);
												Utilities.WriteErrorsToConsole(objSFdrmSections.ErrorMessages);
												}

											Console.WriteLine("\t Completed generation of {0}", objDocumentWorkbook.GetType());
											}
										break;
										}
								default:
									break;
									}
								}
							} // end if ...Count() > 0

						}

					Console.WriteLine("\nDocuments for {0} Document Collection(s) were Generated.", docCollectionsToGenerate.Count);
					}
				else
					{
					Console.WriteLine("Sorry, nothing to generate at this stage.");
					}
				}
			catch(DataServiceTransportException exc)
				{
				if(exc.Message.Contains("timed out"))
					{
					Console.WriteLine("The data connection to SharePoint timed out - and the report couldn't complete in time. The {0} will retry to generate the {1} document", "DocGenerator", "this");
					}
				else
					{
					Console.WriteLine("Exception Error: {0} occurred and means {1}", exc.HResult, exc.Message);
					}
				Console.WriteLine("Exception Error: {0} occurred and means {1}", exc.HResult, exc.Message);
				}
			catch(Exception ex) // if the List is empty - nothing to generate
				{
				Console.WriteLine("Exception Error: {0} occurred and means {1}", ex.HResult, ex.Message);
				}
			finally
				{
				Cursor.Current = Cursors.Default;
				}
			}

		private void btnOpenMSwordDocument(object sender, EventArgs e)
			{
			string parTemplateURL = "https://teams.dimensiondata.com/sites/ServiceCatalogue/DocumentTemplates/ServicesFrameworkDocumentTemplate.dotx";
			enumDocumentTypes parDocumentType = enumDocumentTypes.Service_Framework_Document_DRM_sections;
			int tableCaptionCounter = 0;
			int imageCaptionCounter = 0;
			int hyperlinkCounter = 1;
			
			// define a new objOpenXMLdocument
			oxmlDocument objOXMLdocument = new oxmlDocument();
			// use CreateDocumentFromTemplate method to create a new MS Word Document based on the relevant template
			if(objOXMLdocument.CreateDocWbkFromTemplate(
				parDocumentOrWorkbook: enumDocumentOrWorkbook.Document,
				parTemplateURL: parTemplateURL, 
				parDocumentType: parDocumentType))
				{
				Console.WriteLine("objOXMLdocument:\n" +
				"                + LocalDocumentPath: {0}\n" +
				"                + DocumentFileName.: {1}\n" +
				"                + DocumentURI......: {2}", objOXMLdocument.LocalPath, objOXMLdocument.Filename, objOXMLdocument.LocalURI);
				}
			else
				{
				// if the creation failed.
				Console.WriteLine("An ERROR occurred and the new MS Word Document could not be created due to above stated ERROR conditions.");
				return;
				}
			try
				{
				// Open the MS Word document in Edit mode
				WordprocessingDocument objWPdocument = WordprocessingDocument.Open(path: objOXMLdocument.LocalURI, isEditable: true);

				// Define all open XML objects to use for building the document
				MainDocumentPart objMainDocumentPart = objWPdocument.MainDocumentPart;
				Body objBody = objWPdocument.MainDocumentPart.Document.Body;
				Paragraph objParagraph = new Paragraph();    // Define the objParagraph	
				DocumentFormat.OpenXml.Wordprocessing.Run objRun = new DocumentFormat.OpenXml.Wordprocessing.Run();

				// Now begin to write the content to the document

				objParagraph = oxmlDocument.Construct_Heading(parHeadingLevel: 0, parNoNumberedHeading: true);
				objRun = oxmlDocument.Construct_RunText(
					parText2Write: Properties.AppResources.Document_ColourCodingLedgend_Heading,
					parBold: true);
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);

				objParagraph = oxmlDocument.Construct_Paragraph(parBodyTextLevel: 0);
				objRun = oxmlDocument.Construct_RunText(
					parText2Write: Properties.AppResources.Document_ColourCodingLedgend_Text);
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);

				objParagraph = oxmlDocument.Construct_BulletNumberParagraph(parBulletLevel: 0, parIsBullet: true);
				objRun = oxmlDocument.Construct_RunText(
					parText2Write: Properties.AppResources.Document_ColourCodingLedgend_Layer1,
					parContentLayer: "Layer1");
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);

				objParagraph = oxmlDocument.Construct_BulletNumberParagraph(parBulletLevel: 0, parIsBullet: true);
				objRun = oxmlDocument.Construct_RunText(
					parText2Write: Properties.AppResources.Document_ColourCodingLedgend_Layer2,
					parContentLayer: "Layer2");
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);

				objParagraph = oxmlDocument.Construct_BulletNumberParagraph(parBulletLevel: 0, parIsBullet: true);
				objRun = oxmlDocument.Construct_RunText(
					parText2Write: Properties.AppResources.Document_ColourCodingLedgend_Layer3,
					parContentLayer: "Layer3");
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);

				objParagraph = oxmlDocument.Construct_Paragraph(parBodyTextLevel: 0);
				objRun = oxmlDocument.Construct_RunText(
					parText2Write: " ");
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);

				// Just some text to validate all routines
				objParagraph = oxmlDocument.Construct_Heading(parHeadingLevel: 1);
				objRun = oxmlDocument.Construct_RunText(
					parText2Write: Properties.AppResources.Document_IntruductorySection_HeadingText,
					parIsNewSection: true);
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);
				objParagraph = oxmlDocument.Construct_Heading(parHeadingLevel: 2);
				objRun = oxmlDocument.Construct_RunText(Properties.AppResources.Document_Introduction_HeadingText);
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);
				objParagraph = oxmlDocument.Construct_Paragraph(1);
				objRun = oxmlDocument.Construct_RunText("This is a run of Text with ");
				objParagraph.Append(objRun);
				objRun = oxmlDocument.Construct_RunText(" Bold, ", parBold: true);
				objParagraph.Append(objRun);
				objRun = oxmlDocument.Construct_RunText("Bold Underline, ", parBold: true, parUnderline: true);
				objParagraph.Append(objRun);
				objRun = oxmlDocument.Construct_RunText(" Bold Italic, ", parBold: true, parItalic: true);
				objParagraph.Append(objRun);
				objRun = oxmlDocument.Construct_RunText(" Italic, ", parItalic: true);
				objParagraph.Append(objRun);
				objRun = oxmlDocument.Construct_RunText("Underline,", parUnderline: true);
				objParagraph.Append(objRun);
				objRun = oxmlDocument.Construct_RunText(" and ");
				objParagraph.Append(objRun);
				objRun = oxmlDocument.Construct_RunText("Italic Underline", parItalic: true, parUnderline: true);
				objParagraph.Append(objRun);
				objRun = oxmlDocument.Construct_RunText(" properties.");
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);

				objParagraph = oxmlDocument.Construct_Paragraph(1);
				objRun = oxmlDocument.Construct_RunText("Another paragraph with just normal text.");
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);

				objParagraph = oxmlDocument.Construct_Heading(parHeadingLevel: 2);
				objRun = oxmlDocument.Construct_RunText(parText2Write: "A send level heading (error)", parIsError: true);
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);
				objParagraph = oxmlDocument.Construct_Paragraph(2);
				objRun = oxmlDocument.Construct_RunText("Below is an image of my favourite car. ");
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);

				// Determine the Page Size for the current Body object.
				SectionProperties objSectionProperties = new SectionProperties();
				UInt32 pageWidth = Convert.ToUInt32(Properties.AppResources.DefaultPageWidth);
				UInt32 pageHeight = Convert.ToUInt32(Properties.AppResources.DefaultPageHeight);
			
				if(objBody.GetFirstChild<SectionProperties>() != null)
					{
					objSectionProperties = objBody.GetFirstChild<SectionProperties>();
					PageSize objPageSize = objSectionProperties.GetFirstChild<PageSize>();
					PageMargin objPageMargin = objSectionProperties.GetFirstChild<PageMargin>();
					if(objPageSize != null)
						{
						pageWidth = objPageSize.Width;
						Console.WriteLine("Page Width.: {0}", objPageSize.Width);
						pageHeight = objPageSize.Height;
						Console.WriteLine("Page Height: {0}", objPageSize.Height);
						}
					if(objPageMargin != null)
						{
						if(objPageMargin.Left != null)
							{
							pageWidth -= objPageMargin.Left;
							Console.WriteLine("Left Margin: {0}", objPageMargin.Right);
							}
						if(objPageMargin.Right != null)
							{
							pageWidth -= objPageMargin.Right;
							Console.WriteLine("Right Margin: {0}", objPageMargin.Right);
							}
						if(objPageMargin.Top != null)
							{
							string tempTop = objPageMargin.Top.ToString();
							Console.WriteLine("Top Margin: {0}", tempTop);
							pageHeight -= Convert.ToUInt32(tempTop);
							}
						if(objPageMargin.Bottom != null)
							{
							string tempBottom = objPageMargin.Bottom.ToString();
							Console.WriteLine("Bottom Margin: {0}", tempBottom);
							pageHeight -= Convert.ToUInt32(tempBottom);
							}
						}
	                    }
				Console.WriteLine("Effective pageWidth.: {0}twips", pageWidth);
				Console.WriteLine("Effective pageHeight: {0}twips", pageHeight);

				// Insert and image in the document
				objParagraph = oxmlDocument.Construct_Paragraph(2);
				objRun = oxmlDocument.InsertImage(
					parMainDocumentPart: ref objMainDocumentPart,
					parEffectivePageTWIPSheight: pageHeight,
					parEffectivePageTWIPSwidth: pageWidth,
					parParagraphLevel: 2,
					parPictureSeqNo: 1,
					parImageURL: @Properties.AppResources.TestData_Location + "RS5.jpg");
				if(objRun != null)
					{
					objParagraph.Append(objRun);
					objBody.AppendChild<Paragraph>(objParagraph);
					}
				else
					{
					objRun = oxmlDocument.Construct_RunText("ERROR: Unable to insert the image - an error occurred");
					objBody.Append(objParagraph);
					}
				// Insert the Image Caption
					// First increment the Image Caption Counter with 1
				imageCaptionCounter += 1;
				objParagraph = oxmlDocument.Construct_Caption(
					parCaptionType: "Image", 
					parCaptionText: Properties.AppResources.Document_Caption_Image_Text + imageCaptionCounter + ": " + "An awesome machine.");
				objBody.Append(objParagraph);

				// Insert a Heading for the Table section.
				objParagraph = oxmlDocument.Construct_Heading(parHeadingLevel: 2);
				objRun = oxmlDocument.Construct_RunText(
					parText2Write: "Tables",
					parIsNewSection: true);
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);
				// Insert a paragraph of text
				objParagraph = oxmlDocument.Construct_Paragraph(2);
				objRun = oxmlDocument.Construct_RunText("This demonstrates how tables are handled by the DocGenerator application.", parBold: true);
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);

				//Table Construction code
				// Construct a Table object instance
				DocumentFormat.OpenXml.Wordprocessing.Table objTable = new DocumentFormat.OpenXml.Wordprocessing.Table();
				objTable = oxmlDocument.ConstructTable(
					parPageWidth: pageWidth,
					parFirstRow: true, 
					parFirstColumn: true, 
					parLastColumn: true, 
					parLastRow: true, 
					parNoVerticalBand: true, 
					parNoHorizontalBand: false);
				// Create the Table Row and append it to the Table object
				TableRow objTableRow = new TableRow();
				TableCell objTableCell = new TableCell();
				bool IsFirstRow = false;
				bool IsLastRow = false;
				bool IsFirstColumn = false;
				bool IsLastColumn = false;
				int numberOfRows = 6;
				int numberOfColumns = 4;
				string tableText = "";
				UInt32 columnWidth = pageWidth / Convert.ToUInt32(numberOfColumns);
				// Construct a TableGrid object instance
				TableGrid objTableGrid = new TableGrid();
				List<UInt32> lstTableColumns = new List<UInt32>();
				for(int i = 0; i < numberOfColumns; i++)
					{
					lstTableColumns.Add(columnWidth);
					}
				objTableGrid = oxmlDocument.ConstructTableGrid(lstTableColumns);
				// Append the TableGrid object instance to the Table object instance
				objTable.Append(objTableGrid);
				
				// Create a TableRow object instance
				for(int r = 1; r < numberOfRows+1; r++)
					{
					// Construct a TableRow
					if(r == 1) // the Hear row
						IsFirstRow = true;
					else
						IsFirstRow = false;

					if(r == numberOfRows)
						IsLastRow = true;
					else
						IsLastRow = false;

					objTableRow = oxmlDocument.ConstructTableRow(parIsFirstRow: IsFirstRow, parIsLastRow: IsLastRow);
					// Create the TableCells for each Column
					for(int c = 1; c < numberOfColumns+1; c++)
						{
						objParagraph = oxmlDocument.Construct_Paragraph(parBodyTextLevel: 1, parIsTableParagraph: true);
						if(c == 1)
							IsFirstColumn = true;
						else
							IsFirstColumn = false;

						if(c == numberOfColumns)
							IsLastColumn = true;
						else
							IsLastColumn = false;

						objTableCell = oxmlDocument.ConstructTableCell(
							lstTableColumns[c-1],
							parIsFirstRow: IsFirstRow,
							parIsLastRow: IsLastRow,
							parIsFirstColumn: IsFirstColumn,
							parIsLastColumn: IsLastColumn);

						// Create a Pargaraph for the text to go into the TableCell
						objParagraph = oxmlDocument.Construct_Paragraph(1, parIsTableParagraph: true);
						tableText = "Row " + r + ", Column " + c + " Text";
						objRun = oxmlDocument.Construct_RunText(tableText);
						objParagraph.Append(objRun);
						objTableCell.Append(objParagraph);
						objTableRow.Append(objTableCell);
						} //end For numberOfColumns loop
					objTable.Append(objTableRow);
					} // end For numberOfRows loop
				// Insert the Table object into the document Body
				objBody.Append(objTable);

				// Insert the Table Caption
				// increment the table Caption Counter with 1
				tableCaptionCounter += 1;
				objParagraph = oxmlDocument.Construct_Caption(
					parCaptionType: "Table",
					parCaptionText: Properties.AppResources.Document_Caption_Table_Text + tableCaptionCounter + ": " + "A table generated by the app.");
				objBody.Append(objParagraph);
				
				// Insert a new XML Table based on an HTML table input from a local file.
				objParagraph = oxmlDocument.Construct_Heading(parHeadingLevel: 3);
				objRun = oxmlDocument.Construct_RunText(
					parText2Write: "How HTML content is handled",
					parIsNewSection: true);
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);

				HTMLdecoder objHTMLdecoder = new HTMLdecoder();
				objHTMLdecoder.WPbody = objBody;
				string sCurrentDirectory = Directory.GetCurrentDirectory();
				Console.WriteLine("Current Directory is {0}", sCurrentDirectory);
				string sFile = @Properties.AppResources.TestData_Location + "IntoFromSharePoint.txt";
				string sContent = System.IO.File.ReadAllText(sFile);
				objHTMLdecoder.DecodeHTML(
					parMainDocumentPart: ref objMainDocumentPart,
					parDocumentLevel: 3,
					parPageWidthTwips: pageWidth,
					parPageHeightTwips: pageHeight,
					parHTML2Decode: sContent,
					parTableCaptionCounter: ref tableCaptionCounter,
					parImageCaptionCounter: ref imageCaptionCounter,
					parHyperlinkID: ref hyperlinkCounter);
				hyperlinkCounter += 1;

				// Close the document
				objParagraph = oxmlDocument.Construct_Paragraph(1);
				objRun = oxmlDocument.Construct_RunText("--- end of the document --- ");
				objParagraph.Append(objRun);
				objBody.Append(objParagraph);

				Console.WriteLine("\t\t Document generated, now saving and closing the document.");

				// Save and close the Document
				objWPdocument.Close();

				Console.WriteLine("Document saved and closed!!!");
				} // end Try

			catch(OpenXmlPackageException exc)
				{
				//TODO: add code to catch exception.
				}
			catch(ArgumentNullException exc)
				{
				//TODO: add code to catch exception.
				}
			
			}

		private void Form1_Load(object sender, EventArgs e)
			{
			
               }

		private void buttonTestSpeed_Click(object sender, EventArgs e)
			{
			Console.WriteLine("\n\nButton clicked to begin Access speed comparisson - {0}", DateTime.Now);

			List<int> listDeliverables = new List<int>() {1, 173, 393, 701, 937, 92};
			DateTime timeStarted = DateTime.Now;
			DateTime timeLap;
			//Initialize the Data access to SharePoint
			DesignAndDeliveryPortfolioDataContext datacontexSDDP = new DesignAndDeliveryPortfolioDataContext(new
				Uri(Properties.AppResources.SharePointSiteURL + Properties.AppResources.SharePointRESTuri)); //"/_vti_bin/listdata.svc"));
			datacontexSDDP.Credentials = CredentialCache.DefaultCredentials;
			datacontexSDDP.MergeOption = System.Data.Services.Client.MergeOption.NoTracking;
			// https://msdn.microsoft.com/en-us/library/ff798478.aspx

			//var rsDeliverables1 = from deliverableEntry in datacontexSDDP.Deliverables select deliverableEntry;
			timeStarted = DateTime.Now;

			// Specific entry with WHERE clause
			Console.WriteLine("\nRead specific Deliverables with WHERE clause started at: {0}", timeStarted);
			foreach(var specificID in listDeliverables)
				{
				timeLap = DateTime.Now;
				var rsDeliverables1 = from deliverableEntry in datacontexSDDP.Deliverables
					where deliverableEntry.Id == specificID
					select new
						{
						deliverableEntry.Id,
						deliverableEntry.Title
						};
				try
					{
					var thisEntry = rsDeliverables1.FirstOrDefault();
					Console.WriteLine("{0}sec to retrieve {1} - {2}", DateTime.Now - timeLap, thisEntry.Id, thisEntry.Title);
					}
				catch(DataServiceQueryException exc)
					{
					Console.WriteLine("{0} - NOT FOUND...", specificID);
					Console.WriteLine("Error: {0}, {1} \n{2}", exc.HResult, exc.StackTrace, exc.Message);
					}
				catch(Exception exc) // exceptions other than DataQueryExceptions
					{
					Console.WriteLine("Error: {0}, {1} \n{2}", exc.HResult, exc.StackTrace, exc.Message);
					}
				}
			Console.WriteLine("Total time: {0}sec", DateTime.Now - timeStarted);
			
			// Read ALL entries up fron't and then find individuals entries
			timeStarted = DateTime.Now;
			Console.WriteLine("\n\nRead all Deliverable started at: {0}",timeStarted);
			
			var rsDeliverables2 = from deliverableItem in datacontexSDDP.Deliverables 
				select new
					{
					deliverableItem.Id,
					deliverableItem.Title
					};

			//Console.WriteLine("\n\rFind entry {0}", deliverableID);
			//DeliverablesItem firstMatch = rsDeliverables.AsQueryable().First(DeliverablesItem => DeliverablesItem.Id > deliverableID);
			foreach(var specificID in listDeliverables)
				{
				timeLap = DateTime.Now;
				// var thisEntry = rsDeliverables.First(entries => entries.Id == specificID);
				var thisEntry = (from entry in rsDeliverables2 where entry.Id == specificID select entry).FirstOrDefault();
                    Console.WriteLine("{0}sec to retrieve {1} - {2}", DateTime.Now - timeLap, thisEntry.Id, thisEntry.Title);
				}
			Console.WriteLine("Total time: {0}sec", DateTime.Now - timeStarted);


			// Read ALL entries using CAML
			timeStarted = DateTime.Now;
			Console.WriteLine("\n\nRead all Deliverable with CAML started at: {0}", timeStarted);

			var thisContext = new DesignAndDeliveryPortfolioDataContext(new Uri(Properties.AppResources.SharePointSiteURL + Properties.AppResources.SharePointRESTuri));
			thisContext.Credentials = CredentialCache.DefaultCredentials;
			thisContext.MergeOption = MergeOption.NoTracking;

			foreach(var specificID in listDeliverables)
				{
				timeLap = DateTime.Now;
				var rsDeliverables3 = thisContext.Deliverables
					.Where(entry => entry.Id == specificID)
					.Take(1)
					.ToList()
					.SingleOrDefault();
				// var thisEntry = rsDeliverables.First(entries => entries.Id == specificID);
				Console.WriteLine("{0}sec to retrieve {1} - {2}", DateTime.Now - timeLap, rsDeliverables3.Id, rsDeliverables3.Title);
				}
			Console.WriteLine("Total time: {0}sec", DateTime.Now - timeStarted);

			}

		///%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_GenerateExcel_Click(object sender, EventArgs e)
			{
			string parTemplateURL = "https://teams.dimensiondata.com/sites/ServiceCatalogue/DocumentTemplates/ClientRequirementsMappingOutputTemplate.xltx";
			enumDocumentTypes parDocumentType = enumDocumentTypes.Client_Requirement_Mapping_Workbook;
			int tableCaptionCounter = 0;
			int imageCaptionCounter = 0;
			int hyperlinkCounter = 1;
			

			// define a new objOpenXMLdocument
			Console.WriteLine("Begin to Generate an MS Excel Workbook");
			oxmlWorkbook objOXMLworkbook = new oxmlWorkbook();
			// use CreateDocumentFromTemplate method to create a new MS Excel Workbook Document based on the relevant template
			Console.WriteLine("Creating the new Worksheet from an existing Template");
			if(objOXMLworkbook.CreateDocWbkFromTemplate(
				parDocumentOrWorkbook: enumDocumentOrWorkbook.Workbook,
				parTemplateURL: parTemplateURL,
				parDocumentType: parDocumentType))
				{
				Console.WriteLine("\t objOXMLworkbook:\n" +
				"\t\t + LocalDocumentPath: {0}\n" +
				"\t\t + DocumentFileName.: {1}\n" +
				"\t\t + DocumentURI......: {2}", objOXMLworkbook.LocalPath, objOXMLworkbook.Filename, objOXMLworkbook.LocalURI);
				}
			else
				{
				// if the creation failed.
				Console.WriteLine("An ERROR occurred and the new MS Word Document could not be created due to above stated ERROR conditions.");
				return;
				}
			try
				{
				// Open the MS Excel document in Edit mode
				// https://msdn.microsoft.com/en-us/library/office/hh298534.aspx
				SpreadsheetDocument objSpreadsheetDocument = SpreadsheetDocument.Open(path: objOXMLworkbook.LocalURI, isEditable: true);
				// Obtain the WorkBookPart from the spreadsheet.
				if(objSpreadsheetDocument.WorkbookPart == null)
					{
					throw new ArgumentException(objOXMLworkbook.LocalURI +
						" does not contain a WorkbookPart.");
					}
				WorkbookPart objWorkbookPart = objSpreadsheetDocument.WorkbookPart;

				//Obtain the SharedStringTablePart - If it doesn't exisit, create new one.
				SharedStringTablePart objSharedStringTablePart;
				if(objSpreadsheetDocument.WorkbookPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
					{
					objSharedStringTablePart = objSpreadsheetDocument.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
					}
				else
					{
					objSharedStringTablePart = objSpreadsheetDocument.AddNewPart<SharedStringTablePart>();
					}

				// Get the Matrix worksheet in the Workbook.
				Sheet objMatrixWorksheet = objWorkbookPart.Workbook.Descendants<Sheet>().Where(sht => sht.Name == Properties.AppResources.Workbook_CRM_Worksheet_Name_Matrix).FirstOrDefault();
				if(objMatrixWorksheet == null)
					{
					throw new ArgumentException("The " + Properties.AppResources.Workbook_CRM_Worksheet_Name_Matrix +
						" worksheet could not be loacated in the workbook.");
					}
				// obtain the WorksheetPart of the objMatrixWorksheet
				WorksheetPart objMatrixWorksheetPart = (WorksheetPart)(objWorkbookPart.GetPartById(objMatrixWorksheet.Id));

				// construct the CommentsList to which Comments objects can be appended
				CommentList objMatrixCommentList = new CommentList();

				//WorksheetCommentsPart objMatrixCommentsPart = (WorksheetCommentsPart)(objMatrixWorksheetPart.GetPartsOfType<WorksheetCommentsPart>().First());

				// Copy the Formats from Row 8 into the List of Formats from where it can be applied to every Row at a later stage
				Client_Requirements_Mapping_Workbook objCRMworkbook = new Client_Requirements_Mapping_Workbook();
				List<UInt32Value> listColumnStyles = new List<UInt32Value>();
				int intLastColumn = 15;
				int intStyleSourceRow = 7;
				string strCellAddress = "";
                    for(int i = 0; i < intLastColumn - 1; i++)
					{
					strCellAddress = objCRMworkbook.GetColumnLetter(i) + intStyleSourceRow;
					Cell objSourceCell = objMatrixWorksheetPart.Worksheet.Descendants<Cell>().Where(c => c.CellReference == strCellAddress).FirstOrDefault();
					if(objSourceCell != null)
						{
						listColumnStyles.Add(objSourceCell.StyleIndex);
						}
					else
						listColumnStyles.Add(0U);
                         } // loop

				// Display the List of Formats that was collected.
				Console.WriteLine("Cell Styles..:");
				int col = 0;
				foreach(UInt32Value item in listColumnStyles)
					{
					Console.WriteLine("\t {0} ({1}) = {2} ", col, objCRMworkbook.GetColumnLetter(col), item.Value);
					col += 1;
					}

				UInt16 intRowNumber = 10;
				Cell objCell = new Cell();
				
				//----------------------------------------
				// Write a normal String
				string strColumnLetter = "A";
				objCell = oxmlWorkbook.InsertCellInWorksheet(
					parWorksheetPart: objMatrixWorksheetPart,
					parColumnName: strColumnLetter,
					parRowIndex: intRowNumber
					);
				objCell.DataType = new EnumValue<CellValues>(CellValues.String);
				objCell.CellValue = new CellValue("Tower of Service");
				objCell.StyleIndex = (UInt32Value)(listColumnStyles.ElementAt(objCRMworkbook.GetColumnNumber(strColumnLetter)));

				//----------------------------------------
				strColumnLetter = "B";
				objCell = oxmlWorkbook.InsertCellInWorksheet(
					parWorksheetPart: objMatrixWorksheetPart,
					parColumnName: strColumnLetter,
					parRowIndex: intRowNumber
					);
				objCell.StyleIndex = listColumnStyles.ElementAt(objCRMworkbook.GetColumnNumber(strColumnLetter));

				//----------------------------------------
				strColumnLetter = "C";
				objCell = oxmlWorkbook.InsertCellInWorksheet(
					parWorksheetPart: objMatrixWorksheetPart,
					parColumnName: strColumnLetter,
					parRowIndex: intRowNumber
					);
				objCell.StyleIndex = listColumnStyles.ElementAt(objCRMworkbook.GetColumnNumber(strColumnLetter));

				//----------------------------------------
				strColumnLetter = "D";
				objCell = oxmlWorkbook.InsertCellInWorksheet(
					parWorksheetPart: objMatrixWorksheetPart,
					parColumnName: strColumnLetter,
					parRowIndex: intRowNumber
					);
				objCell.StyleIndex = listColumnStyles.ElementAt(objCRMworkbook.GetColumnNumber(strColumnLetter));

				//----------------------------------------
				strColumnLetter = "E";
				objCell = oxmlWorkbook.InsertCellInWorksheet(
					parWorksheetPart: objMatrixWorksheetPart,
					parColumnName: strColumnLetter,
					parRowIndex: intRowNumber
					);
				objCell.StyleIndex = listColumnStyles.ElementAt(objCRMworkbook.GetColumnNumber(strColumnLetter));

				// Write the NEW as a Shared String value 
				strColumnLetter = "F";
				int intStringIndex = 0;
				intStringIndex = oxmlWorkbook.InsertSharedStringItem(
					parText2Insert: Properties.AppResources.Workbook_CRM_Matrix_NewColumn_Text, parShareStringPart: objSharedStringTablePart);
				// now write the text to the Worksheet.
				objCell = oxmlWorkbook.InsertCellInWorksheet(
					parColumnName: strColumnLetter,
					parRowIndex: intRowNumber,
					parWorksheetPart: objMatrixWorksheetPart);
				objCell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
				objCell.CellValue = new CellValue(intStringIndex.ToString());
				objCell.StyleIndex = listColumnStyles.ElementAt(objCRMworkbook.GetColumnNumber(strColumnLetter));
				//goto Skip_Comments;
				//----------------------------------------------------------------------
				//Insert the comment containing the description of the entry in the row.
				// obtain the WorksheetCommentsPart - required to insert Comments in cells
				WorksheetCommentsPart objMatrixWorksheetCommentsPart;
				
				// Check if any WorkSheetComments exist, if not create the WorksheetComments Part
				if(objMatrixWorksheetPart.WorksheetCommentsPart == null)
					{
					objMatrixWorksheetCommentsPart = objMatrixWorksheetPart.AddNewPart<WorksheetCommentsPart>("rId5");
					}
				else
					{
					objMatrixWorksheetCommentsPart = objMatrixWorksheetPart.WorksheetCommentsPart;
					}

				Console.WriteLine("WorksheetCommentsPart.id: {0}", objMatrixWorksheetCommentsPart);

				// Get the Comments object
				DocumentFormat.OpenXml.Spreadsheet.Comments objComments;
				// If it is null, create a new Comments Collection else use the existing on 
				if(objMatrixWorksheetCommentsPart.Comments == null)
					{
					objComments = new DocumentFormat.OpenXml.Spreadsheet.Comments();
					objMatrixWorksheetCommentsPart.Comments = objComments;
					}
				else
					{
					objComments = objMatrixWorksheetCommentsPart.Comments;
					}

				// Create the Authors collection
				Authors objAuthors;
				// Check if the Authors collection already exist;
				// If not create a new Authors collection
				if(objComments.Authors == null)
					{
					objAuthors = new Authors();
					objComments.Append(objAuthors);
					}
				else
					{
					objAuthors = objComments.Authors;
					}

				// Create the new Author
				Author objAuthor = new Author();
				objAuthor.Text = "DocGenerator";
				UInt32Value intAuthorId = 0U;
				if(objAuthors.Count() == 0)
					intAuthorId = 0U;
				else
					intAuthorId = Convert.ToUInt32(objAuthors.Count()); // use number because it is a zero base value
				Console.WriteLine("AuthorId: {0}", intAuthorId);
				objAuthors.Append(objAuthor);

				// Construct the CommentList which contains the individual comments
				CommentList objCommentList;
				// Check if a CommentList already exist or whether one needs to be created
				if(objComments.CommentList == null)
					{
					objCommentList = new CommentList();
					objComments.Append(objCommentList);
					}
				else
					{
					objCommentList = objComments.CommentList;
					}


				// Add the new Comment
				DocumentFormat.OpenXml.Spreadsheet.Comment objComment = new DocumentFormat.OpenXml.Spreadsheet.Comment();
				objComment.Reference = strColumnLetter + intRowNumber;
				objComment.AuthorId = 0U; //intAuthorId;
				objComment.ShapeId = (UInt32Value)0U;

				CommentText objCommentText = new CommentText();
				DocumentFormat.OpenXml.Spreadsheet.Run objRun = new DocumentFormat.OpenXml.Spreadsheet.Run();
				DocumentFormat.OpenXml.Spreadsheet.RunProperties objRunProperties = new DocumentFormat.OpenXml.Spreadsheet.RunProperties();
				DocumentFormat.OpenXml.Spreadsheet.FontSize objFontSize = new DocumentFormat.OpenXml.Spreadsheet.FontSize();
				objFontSize.Val = Convert.ToDouble(Convert.ToInt32(Properties.AppResources.Workbooks_Comments_FontSize));
				DocumentFormat.OpenXml.Spreadsheet.Color objColor = new DocumentFormat.OpenXml.Spreadsheet.Color();
				objColor.Indexed = (UInt32)Convert.ToInt32(Properties.AppResources.Workbook_Comments_FontColor);
                    DocumentFormat.OpenXml.Spreadsheet.RunFont objRunFont = new DocumentFormat.OpenXml.Spreadsheet.RunFont();
				objRunFont.Val = Properties.AppResources.Workbook_Comments_RunFont;
                    DocumentFormat.OpenXml.Spreadsheet.FontFamily objFontFamily = new DocumentFormat.OpenXml.Spreadsheet.FontFamily();
				objFontFamily.Val = Convert.ToInt32(Properties.AppResources.Workbooks_Comments_FontFamily);
				DocumentFormat.OpenXml.Spreadsheet.FontScheme objFontScheme = new DocumentFormat.OpenXml.Spreadsheet.FontScheme();
				objFontScheme.Val = FontSchemeValues.Minor;

				objRunProperties.Append(objFontSize);
				objRunProperties.Append(objColor);
				objRunProperties.Append(objRunFont);
				objRunProperties.Append(objFontFamily);
				objRunProperties.Append(objFontScheme);

				DocumentFormat.OpenXml.Spreadsheet.Text objText = new DocumentFormat.OpenXml.Spreadsheet.Text();
				objText.Text = "This comment box contains the description of the the relevant Row Type that is visible in the corresponding left column." +
					"It is not a comprehensive description.\nTo access the detailed descriptions, open the relevant entries by clicking " + 
					"on the corresponding cell in the References columns (the 3 columns on the right)";
				objRun.Append(objRunProperties);
				objRun.Append(objText);
				objCommentText.Append(objRun);
				objComment.Append(objCommentText);

				objCommentList.Append(objComment);

				objComments.Save();
Skip_Comments:
				// Write the ROW TYPE as a Shared String value
				strColumnLetter = "G";
				intStringIndex = oxmlWorkbook.InsertSharedStringItem(
					parText2Insert: Properties.AppResources.Workbook_CRM_Matrix_RowType_TowerOfService, parShareStringPart: objSharedStringTablePart);
				// now write the text to the Worksheet.
				objCell = oxmlWorkbook.InsertCellInWorksheet(
					parColumnName: strColumnLetter,
					parRowIndex: intRowNumber,
					parWorksheetPart: objMatrixWorksheetPart);
				objCell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
				objCell.CellValue = new CellValue(intStringIndex.ToString());
				objCell.StyleIndex = listColumnStyles.ElementAt(objCRMworkbook.GetColumnNumber(strColumnLetter));

				//-----------------------------------------------
				strColumnLetter = "H";
				intStringIndex = oxmlWorkbook.InsertSharedStringItem(
					parText2Insert: "Fully Compliant", 
					parShareStringPart: objSharedStringTablePart);
				// now write the text to the Worksheet.
				objCell = oxmlWorkbook.InsertCellInWorksheet(
					parColumnName: strColumnLetter,
					parRowIndex: intRowNumber,
					parWorksheetPart: objMatrixWorksheetPart);
				objCell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
				objCell.CellValue = new CellValue(intStringIndex.ToString());
				objCell.StyleIndex = listColumnStyles.ElementAt(objCRMworkbook.GetColumnNumber(strColumnLetter));

				//----------------------------------------
				strColumnLetter = "I";
				objCell = oxmlWorkbook.InsertCellInWorksheet(
					parWorksheetPart: objMatrixWorksheetPart,
					parColumnName: strColumnLetter,
					parRowIndex: intRowNumber
					);
				objCell.StyleIndex = listColumnStyles.ElementAt(objCRMworkbook.GetColumnNumber(strColumnLetter));

				//----------------------------------------
				// Write a normal String
				strColumnLetter = "J";
				objCell = oxmlWorkbook.InsertCellInWorksheet(
					parWorksheetPart: objMatrixWorksheetPart,
					parColumnName: strColumnLetter,
					parRowIndex: intRowNumber
					);
				objCell.DataType = new EnumValue<CellValues>(CellValues.String);
				objCell.CellValue = new CellValue("RFP Page " + intRowNumber + " Paragraph: " + strColumnLetter);
				objCell.StyleIndex = listColumnStyles.ElementAt(objCRMworkbook.GetColumnNumber(strColumnLetter));


				// Write the MAPPING Reference as a numeric value
				strColumnLetter = "L";
				objCell = oxmlWorkbook.InsertCellInWorksheet(
					parWorksheetPart: objMatrixWorksheetPart,
					parColumnName: strColumnLetter,
					parRowIndex: intRowNumber);
				objCell.DataType = new EnumValue<CellValues>(CellValues.Number);
				objCell.CellValue = new CellValue("101");
				objCell.StyleIndex = listColumnStyles.ElementAt(objCRMworkbook.GetColumnNumber(strColumnLetter));
				// Insert the Hyperlink
				int intHyperlinkCounter = 101;
				oxmlWorkbook.InsertHyperlink(
					parWorksheetPart: objMatrixWorksheetPart,
					parCellReference: strColumnLetter + intRowNumber,
					parHyperLinkID: "Hyp" + intHyperlinkCounter,
					parHyperlinkURL: Properties.AppResources.SharePointURL +
						Properties.AppResources.List_MappingServiceTowers +
						Properties.AppResources.EditFormURI + 101);

				// Write the MAPPING Reference as a numeric value
				strColumnLetter = "M";
				objCell = oxmlWorkbook.InsertCellInWorksheet(
					parWorksheetPart: objMatrixWorksheetPart,
					parColumnName: strColumnLetter,
					parRowIndex: intRowNumber);
				objCell.DataType = new EnumValue<CellValues>(CellValues.Number);
				objCell.CellValue = new CellValue("2");
				objCell.StyleIndex = listColumnStyles.ElementAt(objCRMworkbook.GetColumnNumber(strColumnLetter));
				// Insert the Hyperlink
				intHyperlinkCounter = 2;
				oxmlWorkbook.InsertHyperlink(
					parWorksheetPart: objMatrixWorksheetPart,
					parCellReference: strColumnLetter + intRowNumber,
					parHyperLinkID: "Hyp" + intHyperlinkCounter,
					parHyperlinkURL: Properties.AppResources.SharePointURL +
						Properties.AppResources.List_DeliverablesURI +
						Properties.AppResources.EditFormURI + 2);

				// Write the MAPPING Reference as a numeric value
				strColumnLetter = "N";
				objCell = oxmlWorkbook.InsertCellInWorksheet(
					parWorksheetPart: objMatrixWorksheetPart,
					parColumnName: strColumnLetter,
					parRowIndex: intRowNumber);
				objCell.DataType = new EnumValue<CellValues>(CellValues.Number);
				objCell.CellValue = new CellValue("1");
				objCell.StyleIndex = listColumnStyles.ElementAt(objCRMworkbook.GetColumnNumber(strColumnLetter));
				// Insert the Hyperlink
				intHyperlinkCounter = 1;
				oxmlWorkbook.InsertHyperlink(
					parWorksheetPart: objMatrixWorksheetPart,
					parCellReference: strColumnLetter + intRowNumber,
					parHyperLinkID: "Hyp" + intHyperlinkCounter,
					parHyperlinkURL: Properties.AppResources.SharePointURL +
						Properties.AppResources.List_ServiceLevelsURI +
						Properties.AppResources.EditFormURI + 1);

				// Close the document
				//Validate the document with OpenXML validator
				OpenXmlValidator objOXMLvalidator = new OpenXmlValidator(fileFormat: FileFormatVersions.Office2010);
				int errorCount = 0;
				Console.WriteLine("\n\rValidating document....");
				foreach(ValidationErrorInfo validationError in objOXMLvalidator.Validate(objSpreadsheetDocument))
					{
					errorCount += 1;
					Console.WriteLine("------------- # {0} -------------", errorCount);
					Console.WriteLine("Error ID...........: {0}", validationError.Id);
					Console.WriteLine("Description........: {0}", validationError.Description);
					Console.WriteLine("Error Type.........: {0}", validationError.ErrorType);
					Console.WriteLine("Error Part.........: {0}", validationError.Part.Uri);
					Console.WriteLine("Error Related Part.: {0}", validationError.RelatedPart);
					Console.WriteLine("Error Path.........: {0}", validationError.Path.XPath);
					Console.WriteLine("Error Path PartUri.: {0}", validationError.Path.PartUri);
					Console.WriteLine("Error Node.........: {0}", validationError.Node);
					Console.WriteLine("Error Related Node.: {0}", validationError.RelatedNode);
					Console.WriteLine("Node Local Name....: {0}", validationError.Node.LocalName);
					}

				Console.WriteLine("Workbook generation completed, saving and closing the document.");
				// Save and close the Document
				objSpreadsheetDocument.Close();

				Console.WriteLine("Workbook saved and closed!!!");
				} // end Try

			catch(OpenXmlPackageException exc)
				{
				//TODO: add code to catch exception.
				}
			catch(ArgumentNullException exc)
				{
				//TODO: add code to catch exception.
				}
			}
		}
	}
