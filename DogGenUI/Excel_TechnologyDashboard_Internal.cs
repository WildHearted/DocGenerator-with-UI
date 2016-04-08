﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Xl2010 = DocumentFormat.OpenXml.Office2010.Excel;
using Excel = DocumentFormat.OpenXml.Office.Excel;
using DocumentFormat.OpenXml.Validation;
using DocGenerator.SDDPServiceReference;

namespace DocGenerator
	{
	/// <summary>
	/// This class handles the Internal Technology coverage Dashbord Workbook
	/// </summary>
	class Internal_Technology_Coverage_Dashboard_Workbook:aWorkbook
		{
		public bool Generate()
			{
			Console.WriteLine("\t\t Begin to generate {0}", this.DocumentType);
			DateTime timeStarted = DateTime.Now;
			//string hyperlinkImageRelationshipID = "";
			string strDocumentCollection_HyperlinkURL = "";
			int intHyperlinkCounter = 9;
			string strCurrentHyperlinkViewEditURI = "";
			Cell objCell = new Cell();
			JobRole objJobRole = new JobRole();
			string strCheckDuplicate;
			int intCheckDuplicate;
			//Text Workstrings
			string strText = "";
			string strErrorText = "";

			//Worksheet Row Index Variables (one Row less than the First row that needs to be populated
			UInt16 intRowIndex = 3;

			if(this.HyperlinkEdit)
				{
				strDocumentCollection_HyperlinkURL = Properties.AppResources.SharePointSiteURL +
					Properties.AppResources.List_DocumentCollectionLibraryURI +
					Properties.AppResources.EditFormURI + this.DocumentCollectionID;
				strCurrentHyperlinkViewEditURI = Properties.AppResources.EditFormURI;
				}

			if(this.HyperlinkView)
				{
				strDocumentCollection_HyperlinkURL = Properties.AppResources.SharePointSiteURL +
					Properties.AppResources.List_DocumentCollectionLibraryURI +
					Properties.AppResources.DisplayFormURI + this.DocumentCollectionID;
				strCurrentHyperlinkViewEditURI = Properties.AppResources.DisplayFormURI;
				}

			//Initialize the Data access to SharePoint
			DesignAndDeliveryPortfolioDataContext datacontexSDDP = new DesignAndDeliveryPortfolioDataContext(new
				Uri(Properties.AppResources.SharePointSiteURL + Properties.AppResources.SharePointRESTuri));
			datacontexSDDP.Credentials = CredentialCache.DefaultCredentials;
			datacontexSDDP.MergeOption = MergeOption.NoTracking;

			//Get all Portfolios to start the Data Cache process...

			// Decalre all the object to be used during processing
			ServicePortfolio objPortfolio = new ServicePortfolio();
			ServiceFamily objFamily = new ServiceFamily();
			ServiceProduct objServiceProduct = new ServiceProduct();
			ServiceElement objServiceElement = new ServiceElement();
			Deliverable objDeliverable = new Deliverable();
			TechnologyProduct objTechnologyProduct = new TechnologyProduct();
			DeliverableTechnology objDeliverableTechnology = new DeliverableTechnology();
			List<ServicePortfolio> dslistPortfolios = new List<ServicePortfolio>();


			Console.WriteLine("Caching Data Set");
			foreach(Hierarchy itemHierarchy in this.SelectedNodes.Where(ih => ih.NodeType == enumNodeTypes.POR))
				{
				Console.WriteLine("\t - Portfolio: {0}", itemHierarchy.NodeID);
				objPortfolio = new ServicePortfolio();
				objPortfolio.PopulateObject(datacontexSDDP, itemHierarchy.NodeID);

				if(objPortfolio != null)
					dslistPortfolios.Add(objPortfolio);
				}

			// define a new objOpenXMLworksheet
			oxmlWorkbook objOXMLworkbook = new oxmlWorkbook();
			// use CreateDocumentFromTemplate method to create a new MS Word Document based on the relevant template
			if(objOXMLworkbook.CreateDocWbkFromTemplate(
				parDocumentOrWorkbook: enumDocumentOrWorkbook.Workbook,
				parTemplateURL: this.Template,
				parDocumentType: this.DocumentType))
				{
				Console.WriteLine("\t\t\t objOXMLdocument:\n" +
				"\t\t\t+ LocalDocumentPath: {0}\n" +
				"\t\t\t+ DocumentFileName.: {1}\n" +
				"\t\t\t+ DocumentURI......: {2}", objOXMLworkbook.LocalPath, objOXMLworkbook.Filename, objOXMLworkbook.LocalURI);
				}
			else
				{
				// if the creation failed.
				strErrorText = "An ERROR occurred and the new MS Excel Workbook could not be created due to above stated ERROR conditions.";
				Console.WriteLine(strErrorText);
				this.ErrorMessages.Add(strErrorText);
				return false;
				}

			if(this.SelectedNodes.Count < 1)
				{
				strErrorText = "The user didn't select any Nodes to populate the Workbook.";
				Console.WriteLine("\t\t\t ***" + strErrorText);
				this.ErrorMessages.Add(strErrorText);
				return false;
				}

			// Open the MS Excel Workbook 
			try
				{
				// Open the MS Excel document in Edit mode
				SpreadsheetDocument objSpreadsheetDocument = SpreadsheetDocument.Open(path: objOXMLworkbook.LocalURI, isEditable: true);
				// Obtain the WorkBookPart from the spreadsheet.
				if(objSpreadsheetDocument.WorkbookPart == null)
					{
					throw new ArgumentException(objOXMLworkbook.LocalURI + " does not contain a WorkbookPart. There is a problem with the template file.");
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

				// obtain the RoadMap Worksheet in the Workbook.
				Sheet objWorksheet = objWorkbookPart.Workbook.Descendants<Sheet>().
					Where(sht => sht.Name == Properties.AppResources.Workbook_TechnologyCoverageDashboard_WorksheetName).FirstOrDefault();
				if(objWorksheet == null)
					{
					throw new ArgumentException("The " + Properties.AppResources.Workbook_ContentStatus_WorksheetName +
						" worksheet could not be loacated in the workbook.");
					}
				// obtain the WorksheetPart of the objMatrixWorksheet
				WorksheetPart objWorksheetPart = (WorksheetPart)(objWorkbookPart.GetPartById(objWorksheet.Id));

				// Copy the Cell Formats  for the Rows
				// --- Style for Column A4 to D4
				List<UInt32Value> listColumnStylesA4_D4 = new List<UInt32Value>();
				int intLastColumn = 3;
				string strCellAddress = "";

				for(int i = 0; i < 4; i++)
					{
					strCellAddress = aWorkbook.GetColumnLetter(i) + 4;
					Cell objSourceCell = objWorksheetPart.Worksheet.Descendants<Cell>().Where(c => c.CellReference == strCellAddress).FirstOrDefault();
					if(objSourceCell != null)
						{
						listColumnStylesA4_D4.Add(objSourceCell.StyleIndex);
						Console.WriteLine("\t\t\t\t + {0} - {1}", i, objSourceCell.StyleIndex);
						}
					else
						listColumnStylesA4_D4.Add(0U);
					} // loop

				// --- StyleId for Column D1:D3
				List<UInt32Value> listColumnStylesD1_D3 = new List<UInt32Value>();

				for(int r = 1; r < 4; r++)
					{
					strCellAddress = "D" + r;
					Cell objSourceCell = objWorksheetPart.Worksheet.Descendants<Cell>().Where(c => c.CellReference == strCellAddress).FirstOrDefault();
					if(objSourceCell != null)
						{
						listColumnStylesD1_D3.Add(objSourceCell.StyleIndex);
						Console.WriteLine("\t\t\t\t + {0} - {1}", r, objSourceCell.StyleIndex);
						}
					else
						listColumnStylesD1_D3.Add(0U);
					} // loop

				// Store the StyleID for the Matrix Cell
				UInt32Value uintMatrixColumnStyleID = listColumnStylesA4_D4.ElementAt(intLastColumn);

				// If Hyperlinks need to be inserted, add the 
				Hyperlinks objHyperlinks = new Hyperlinks();

				
				// Define the Dictionaries 
				// --- This Dictionary represent the Deliverable Systems Comments
				// --- --- Key = Row number Value=Systems
				Dictionary<ushort, String> dictDelivSupportSystemComments = new Dictionary<ushort, string>();
				string strSystemComment;
				// --- This Dictionary represent the TechnologyConsideration Comments
				Dictionary<string, String> dictDelivTecConsiderationComments = new Dictionary<string, string>();
				// --- This Dictionary will contain the TechnologyProduct Objects
				Dictionary<int, TechnologyProduct> dictTechProducts = new Dictionary<int, TechnologyProduct>();
				// --- This Dictionary will contain DeliverableTechnology object
				// --- --- The Key will contain the ID of the DeliverableTechnology ID as Key
				// --- --- The Value will contain the DeliverableTechnology object.
				Dictionary<int, DeliverableTechnology> dictDeliverableTechnology = new Dictionary<int, DeliverableTechnology>();
				// --- This Dictionary links the DeliverableTechnology entries to the Row Index
				// --- --- Key = string consisting of DeliverableTechnology ID + "|" + Row Index (to ensure it is always unique)
				// --- --- Value = RowIndex
				Dictionary<string, int> dictDeliverableRows = new Dictionary<string, int>();

				// --- List that is used to collect all the Deliverable Technology entries as objects for a particular Deliverable.
				List<DeliverableTechnology> listDeliverbleTechnologies = new List<DeliverableTechnology>();

				foreach(Hierarchy itemHierarchy in this.SelectedNodes)
					{
					switch(itemHierarchy.NodeType)
						{
					// Ignore the Service Portfolio and Service Family because it is not reflected in the Workbook
					//-----------------------
					case (enumNodeTypes.PRO):
						//-----------------------
							{
							//--- RoadMap --- Populate the styles for column A to B ---
							intRowIndex += 1;
							objServiceProduct.PopulateObject(parDatacontexSDDP: datacontexSDDP, parID: itemHierarchy.NodeID);
							if(objServiceProduct.ID == 0) // the entry could not be found
								{
								// If the entry is not found - write an error in the document and record an error in the error log.
								strErrorText = "Error: The Service Product ID " + itemHierarchy.NodeID +
									" doesn't exist in SharePoint and couldn't be retrieved.";
								this.LogError(strErrorText);
								strErrorText = "Error: Service Product " + itemHierarchy.NodeID + " is missing.";
								strText = strErrorText;
								}
							else
								{
								strText = objServiceProduct.Title;
								}
							Console.WriteLine("\t\t\t + Product: {0} - {1}", objServiceProduct.ID, strText);
							oxmlWorkbook.PopulateCell(
								parWorksheetPart: objWorksheetPart,
								parColumnLetter: "A",
								parRowNumber: intRowIndex,
								parStyleId: (UInt32Value)(listColumnStylesA4_D4.ElementAt(aWorkbook.GetColumnNumber("A"))),
								parCellDatatype: CellValues.String,
								parCellcontents: strText);

							//--- RoadMap --- Populate the styles for column F to G ---
							for(int i = 1; i <= intLastColumn; i++)
								{
								oxmlWorkbook.PopulateCell(
									parWorksheetPart: objWorksheetPart,
									parColumnLetter: aWorkbook.GetColumnLetter(parColumnNo: i),
									parRowNumber: intRowIndex,
									parStyleId: (UInt32Value)(listColumnStylesA4_D4.ElementAt(i)),
									parCellDatatype: CellValues.String);
								}
							break;
							}
					//-----------------------
					case (enumNodeTypes.ELE):
						//-----------------------
							{
							//--- RoadMap --- Populate the styles for column A ---
							intRowIndex += 1;
							oxmlWorkbook.PopulateCell(
								parWorksheetPart: objWorksheetPart,
								parColumnLetter: "A",
								parRowNumber: intRowIndex,
								parStyleId: (UInt32Value)(listColumnStylesA4_D4.ElementAt(0)),
								parCellDatatype: CellValues.String);

							objServiceElement.PopulateObject(parDatacontexSDDP: datacontexSDDP, parID: itemHierarchy.NodeID);
							if(objServiceElement.ID == 0) // the entry could not be found
								{
								// If the entry is not found - write an error in the document and record an error in the error log.
								strErrorText = "Error: The Service Element ID " + itemHierarchy.NodeID +
									" doesn't exist in SharePoint and couldn't be retrieved.";
								this.LogError(strErrorText);
								strErrorText = "Error: Service Element " + itemHierarchy.NodeID + " is missing.";
								strText = strErrorText;
								}
							else
								{
								strText = objServiceElement.Title;
								}
							Console.WriteLine("\t\t\t\t + Element: {0} - {1}", objServiceElement.ID, strText);
							oxmlWorkbook.PopulateCell(
								parWorksheetPart: objWorksheetPart,
								parColumnLetter: "B",
								parRowNumber: intRowIndex,
								parStyleId: (UInt32Value)(listColumnStylesA4_D4.ElementAt(aWorkbook.GetColumnNumber("B"))),
								parCellDatatype: CellValues.String,
								parCellcontents: strText);

							//--- RoadMap --- Populate the styles for column C to D ---
							for(int i = 2; i < 4; i++)
								{
								oxmlWorkbook.PopulateCell(
									parWorksheetPart: objWorksheetPart,
									parColumnLetter: aWorkbook.GetColumnLetter(parColumnNo: i),
									parRowNumber: intRowIndex,
									parStyleId: (UInt32Value)(listColumnStylesA4_D4.ElementAt(i)),
									parCellDatatype: CellValues.String);
								}
							break;
							}
					//-----------------------
					case (enumNodeTypes.ELD):
					case (enumNodeTypes.ELR):
					case (enumNodeTypes.ELM):
						//-----------------------
							{
							//--- RoadMap --- Populate the styles for column A to B ---
							intRowIndex += 1;
							for(int i = 0; i < 2; i++)
								{
								oxmlWorkbook.PopulateCell(
									parWorksheetPart: objWorksheetPart,
									parColumnLetter: aWorkbook.GetColumnLetter(parColumnNo: i),
									parRowNumber: intRowIndex,
									parStyleId: (UInt32Value)(listColumnStylesA4_D4.ElementAt(i)),
									parCellDatatype: CellValues.String);
								}

							objDeliverable.PopulateObject(parDatacontexSDDP: datacontexSDDP, parID: itemHierarchy.NodeID, parGetRACI: true);
							if(objDeliverable.ID == 0) // the entry could not be found
								{
								// If the entry is not found - write an error in the document and record an error in the error log.
								strErrorText = "Error: The Deliverable ID " + itemHierarchy.NodeID +
									" doesn't exist in SharePoint and couldn't be retrieved.";
								this.LogError(strErrorText);
								strErrorText = "Error: Deliverable " + itemHierarchy.NodeID + " is missing.";
								strText = strErrorText;
								}
							else
								{
								strText = objDeliverable.Title;
								}
							Console.WriteLine("\t\t\t\t\t + Deliverable: {0} - {1}", objDeliverable.ID, strText);
							oxmlWorkbook.PopulateCell(
								parWorksheetPart: objWorksheetPart,
								parColumnLetter: "C",
								parRowNumber: intRowIndex,
								parStyleId: (UInt32Value)(listColumnStylesA4_D4.ElementAt(aWorkbook.GetColumnNumber("C"))),
								parCellDatatype: CellValues.String,
								parCellcontents: strText);

							// --- Populate Column D with Style 
							oxmlWorkbook.PopulateCell(
								parWorksheetPart: objWorksheetPart,
								parColumnLetter: "D",
								parRowNumber: intRowIndex,
								parStyleId: (UInt32Value)(listColumnStylesA4_D4.ElementAt(3)),
								parCellDatatype: CellValues.String);

							if(objDeliverable.SupportingSystems.Count > 0)
								{
								strSystemComment = "";
								foreach(String systemItem in objDeliverable.SupportingSystems)
									{
									strSystemComment += ("- " + systemItem + "\n");
									}
								dictDelivSupportSystemComments.Add(key: intRowIndex, value: strSystemComment);
								}

							// --- obtain a list of all the DeliverableTechnology objects associated with this Deliverable
							listDeliverbleTechnologies.Clear();
							listDeliverbleTechnologies = DeliverableTechnology.ObtainListOfTechnologyProducts_Summary(
								parDatacontextSDDP: datacontexSDDP,
								parDeliverableID: objDeliverable.ID);

							// -- Populate the respective Dictionaries with the values
							if(listDeliverbleTechnologies.Count > 0)
								{
								foreach(DeliverableTechnology entryDelvTech in listDeliverbleTechnologies)
									{
									// only process entries which has a complete DeliverableTechnology object.
									if(entryDelvTech.TechnologyProduct != null)
										{
										if(entryDelvTech.TechnologyProduct.Category != null
										&& entryDelvTech.TechnologyProduct.Vendor != null)
											{
											// add an entry to the dictionary of Technology Products
											if(!dictTechProducts.TryGetValue(
												key: entryDelvTech.TechnologyProduct.ID, value: out objTechnologyProduct))
												{
												dictTechProducts.Add(
													key: entryDelvTech.TechnologyProduct.ID,
													value: entryDelvTech.TechnologyProduct);
												}

											// check if there are any Prerequisites to record
											if(entryDelvTech.Considerations != null)
												{
												dictDelivTecConsiderationComments.Add(key: intRowIndex + "|" + entryDelvTech.TechnologyProduct.ID,
													value: entryDelvTech.Considerations);
												}

											// add an entry to the dictionary of Deliverable Technologies
											if(!dictDeliverableTechnology.TryGetValue(
												key: entryDelvTech.ID, value: out objDeliverableTechnology))
												{
												dictDeliverableTechnology.Add(
													key: entryDelvTech.ID,
													value: entryDelvTech);
												}
											// add an entry to the dictionary Deliverable Rows											
											if(!dictDeliverableRows.TryGetValue(
												key: entryDelvTech.ID + "|" + intRowIndex, value: out intCheckDuplicate))
												{
												dictDeliverableRows.Add(key: entryDelvTech.ID + "|" + intRowIndex, value: intRowIndex);
												}
											else
												{
												Console.WriteLine("Duplicate found in dictDeliverable");
												}
											} // if(entryDelvTech.TechnologyProduct.Category != null && entryDelvTech.TechnologyProduct.Vendor != null)
										} // if(entryDelvTech.TechnologyProduct != null)
									} // foreach(DeliverableTechnology entryDelvTech in listDeliverbleTechnologies)
								} // if(listDeliverbleTechnologies.Count > 0)
							break;
							}
						} // end of Switch(itemHierarchy.NodeType)
					} // end


				// Now Populate the Columns from Column D until the point where the Technology Products end.
				Console.WriteLine("\r\n Polulating the RoadMap in the Worksheet...");
				// First sort the JobRoles in the dictJobRoles dictionary according to the Values.
				int intColumnsStartNumber = 2; // Column F  - because columns use a 0 based reference
				int intColumnNumber = intColumnsStartNumber;
				string strComment = "";
				string strColumnFound = "";
				string strRow1mergeTopLeft = "D1";
				string strRow2mergeTopLeft = "D2";
				string strMatricCellValue = "";
				string strColumnLetter;
				string strRowTechProductSearchKey = "";
				string strBreakonTechCategory = "";
				string strBreakonTechVendor = "";
				RowColumnNumber objRowColumnNoValidateKey = new RowColumnNumber();
				// dictionary to store the Merge cells in order to sort them in the correct order when adding to the workbook.
				Dictionary<string, string> dictMergeCells = new Dictionary<string, string>();
				// dictionary containing all the comments to be inserted Key= RowColumnNumberObject Value=Comment
				Dictionary<RowColumnNumber, string> dictComments = new Dictionary<RowColumnNumber, string>();

				// Process the dictTechProducts based on Category->Vendor->Product
				foreach(var entryTechProduct in dictTechProducts
					.OrderBy(so => so.Value.Category.Title)
						.ThenBy(so => so.Value.Vendor.Title)
							.ThenBy(so => so.Value.Title))
					{
					intColumnNumber += 1;
					strColumnLetter = aWorkbook.GetColumnLetter(intColumnNumber);

					//Console.Write("\n Column {2}: {0} \t Id: {1}", entryTechProduct.Value.Title, entryTechProduct.Key, strColumnLetter);
					// Iterate through the rows for each column
					for(ushort row = 1; row < intRowIndex + 1; row++)
						{
						//Console.Write("\n\t + Row {0} - ", row);
						if(row < 4) // exception of the first 3 Rows which is the Heading Rows
							{
							// Row 1 heading need to be poulated with the Technology Category
							if(row == 1)
								{
								// Break processing for Technology Categories
								if(entryTechProduct.Value.Category.Title != strBreakonTechCategory)
									{
									// Check if column merge is required
									if((strColumnLetter + row) == strRow1mergeTopLeft)
										{
										// skip merge with same column breaks
										strRow1mergeTopLeft = strColumnLetter + row;
										}
									else if(aWorkbook.GetColumnLetter(intColumnNumber - 1) + row != strRow1mergeTopLeft)
										{
										dictMergeCells.Add(strRow1mergeTopLeft + ":" + aWorkbook.GetColumnLetter(intColumnNumber - 1) + row,
											aWorkbook.GetColumnLetter(intColumnNumber - 1) + row);
										//Console.Write("\n\t\t + Merge Cells: {0} - {1}", strRow1mergeTopLeft, aWorkbook.GetColumnLetter(intColumnNumber - 1) + row);
										oxmlWorkbook.MergeCell(parWorksheetPart: objWorksheetPart,
											parTopLeftCell: strRow1mergeTopLeft,
											parBottomRightCell: aWorkbook.GetColumnLetter(intColumnNumber - 1) + row);
										strRow1mergeTopLeft = strColumnLetter + row;
										}
									strBreakonTechCategory = entryTechProduct.Value.Category.Title;
									// Populate the column with Category Title 
									oxmlWorkbook.PopulateCell(
										parWorksheetPart: objWorksheetPart,
										parColumnLetter: strColumnLetter,
										parRowNumber: row,
										parStyleId: listColumnStylesD1_D3.ElementAt(row - 1),
										parCellDatatype: CellValues.String,
										parCellcontents: entryTechProduct.Value.Category.Title);
									}
								else // not break processing insert just the in the Cell
									{
									oxmlWorkbook.PopulateCell(
									parWorksheetPart: objWorksheetPart,
									parColumnLetter: strColumnLetter,
									parRowNumber: row,
									parStyleId: listColumnStylesD1_D3.ElementAt(row - 1),
									parCellDatatype: CellValues.String);
									}
								//Console.Write(" + styleID: [{0}] + Column Heading: {1}", listColumnStylesD1_D3.ElementAt(row - 1), entryTechProduct.Value.Category.Title);
								continue;
								} // end if(row == 1)

							// Row 2 heading need to be poulated with the Technology Vendor
							if(row == 2)
								{
								// Break processing for Technology Vendors
								if(entryTechProduct.Value.Vendor.Title != strBreakonTechVendor)
									{
									// Check if column merge is required
									if((strColumnLetter + row) == strRow2mergeTopLeft)
										{
										// skip merge for same column breaks
										strRow2mergeTopLeft = strColumnLetter + row;
										}
									else if(aWorkbook.GetColumnLetter(intColumnNumber - 1) + row != strRow2mergeTopLeft)
										{
										dictMergeCells.Add(strRow2mergeTopLeft + ":" + aWorkbook.GetColumnLetter(intColumnNumber - 1) + row,
											aWorkbook.GetColumnLetter(intColumnNumber - 1) + row);
										strRow2mergeTopLeft = strColumnLetter + row;
										}
									else
										{
										strRow2mergeTopLeft = strColumnLetter + row;
										}
									strBreakonTechVendor = entryTechProduct.Value.Vendor.Title;
									// Populate the column with Vendor Title 
									oxmlWorkbook.PopulateCell(
										parWorksheetPart: objWorksheetPart,
										parColumnLetter: strColumnLetter,
										parRowNumber: row,
										parStyleId: listColumnStylesD1_D3.ElementAt(row - 1),
										parCellDatatype: CellValues.String,
										parCellcontents: entryTechProduct.Value.Vendor.Title);
									}
								else // not break processing insert just the Cell Format
									{
									oxmlWorkbook.PopulateCell(
									parWorksheetPart: objWorksheetPart,
									parColumnLetter: strColumnLetter,
									parRowNumber: row,
									parStyleId: listColumnStylesD1_D3.ElementAt(row - 1),
									parCellDatatype: CellValues.String);
									}
								//Console.Write(" + styleID: [{0}] + Column Heading: {1}", listColumnStylesD1_D3.ElementAt(row - 1), entryTechProduct.Value.Vendor.Title);
								continue;
								} // end if(row == 1)

							if(row == 3)
								{
								// Populate the Technology Products heading with Technology Product Title 
								oxmlWorkbook.PopulateCell(
									parWorksheetPart: objWorksheetPart,
									parColumnLetter: strColumnLetter,
									parRowNumber: row,
									parStyleId: listColumnStylesD1_D3.ElementAt(row - 1),
									parCellDatatype: CellValues.String,
									parCellcontents: entryTechProduct.Value.Title);

								//Console.Write(" + styleID: [{0}] + Column Heading: {1}", listColumnStylesD1_D3.ElementAt(row - 1), entryTechProduct.Value.Title);
								// check if there is a presrequisite that need to be inserted as a comment
								if(entryTechProduct.Value.Prerequisites != null)
									{
									RowColumnNumber objRowColumnOfPrerequisite = new RowColumnNumber();
									objRowColumnOfPrerequisite.RowNumber = row;
									objRowColumnOfPrerequisite.ColumnNumber = intColumnNumber;
									dictComments.Add(objRowColumnOfPrerequisite, entryTechProduct.Value.Prerequisites);
									}
								continue;
								} // end if(row == 1)
							} // if(row < 4) // exception of the first 3 Rows which is the Heading Rows
						else   // row > 3
							{
							strMatricCellValue = null;

							// check if there is Supporting System Comments pertaining to the row and add to the dictComments if there is one
							if(dictDelivSupportSystemComments.TryGetValue(key: row, value: out strComment))
								{
								if(strComment != null && strComment != "")
									{
									objRowColumnNoValidateKey.RowNumber = row;
									objRowColumnNoValidateKey.ColumnNumber = 2; // column C
									strColumnFound = dictComments.Where(dc => dc.Key.RowNumber == row && dc.Key.ColumnNumber == 2).FirstOrDefault().Value;
									//if(!dictComments.TryGetValue(key: objRowColumnNoValidateKey, value: out strComment))
									if(strColumnFound == null) // not found
										{
										RowColumnNumber objSystemCommentRC = new RowColumnNumber();
										objSystemCommentRC.RowNumber = row;
										objSystemCommentRC.ColumnNumber = 2; // Column C
										dictComments.Add(objSystemCommentRC, strComment);
										//Console.Write(" + Supporting System Comment {0} ", strComment.Substring(0, strComment.Length - 1));
										}
									}
								}

							// Process all the Deliverables for the DeliverableTechnology Key match
							foreach(var entryDelvTechnology in dictDeliverableTechnology.Where(dt => dt.Value.TechnologyProduct.ID == entryTechProduct.Key))
								{
								//Console.Write(" - Found Deliverable: {0} for Tech: {1} - {2}", entryDelvTechnology.Key, entryDelvTechnology.Value.TechnologyProduct.ID, entryDelvTechnology.Value.TechnologyProduct.Title);

								foreach(var entryDeliverableRow in dictDeliverableRows.Where(dr => dr.Key == entryDelvTechnology.Value.ID + "|" + row))
									{
									//Console.Write(" - row {0} is a match.", entryDeliverableRow.Value);
									if(entryDeliverableRow.Value == row) // The rows match for the DeliverableTechnology 
										{
										switch(entryDelvTechnology.Value.RoadmapStatus)
											{
										case ("Supported"):
												{
												strMatricCellValue = "4";
												break;
												}
										case ("Next Release"):
												{
												strMatricCellValue = "3";
												break;
												}
										case ("Pipeline"):
												{
												strMatricCellValue = "2";
												break;
												}
											}
										// populate workscheet cell
										oxmlWorkbook.PopulateCell(
											parWorksheetPart: objWorksheetPart,
											parColumnLetter: strColumnLetter,
											parRowNumber: row,
											parStyleId: uintMatrixColumnStyleID,
											parCellDatatype: CellValues.Number,
											parCellcontents: strMatricCellValue);
										//Console.Write("\t + Value: {0} - {1}", entryDelvTechnology.Value.RoadmapStatus, strMatricCellValue);

										// Check if there is a Technology consideration for the cell
										// check if there is System Capabilities Comment pertaining to the row and add to the dictComments if there is one
										strRowTechProductSearchKey = row + "|" + entryDelvTechnology.Value.ID;
										if(dictDelivTecConsiderationComments.TryGetValue(key: strRowTechProductSearchKey, value: out strComment))
											{
											if(strComment != null && strComment != "")
												{
												RowColumnNumber objSystemCommentRC = new RowColumnNumber();
												objSystemCommentRC.RowNumber = row;
												objSystemCommentRC.ColumnNumber = intColumnNumber;
												dictComments.Add(objSystemCommentRC, strComment);
												//Console.Write(" + Tech Consideration Comments: {0}", strComment);
												}
											}

										break;
										} // if(entryDeliverableRow.Value == row)
									if(strMatricCellValue != null)
										break;
									} // foreach(var entryDeliverableRow in dictDeliverableRows.Where(dr => dr.Key == entryDelvTechnology.Value.ID + "|"...
								if(strMatricCellValue != null)
									break;
								} //foreach(var entryDeliverableRow in dictDeliverableRows.Where(dr => dr.Key == entryDelvTechnology.Value.ID + "|" + row))
							if(strMatricCellValue == null)
								{
								oxmlWorkbook.PopulateCell(
									parWorksheetPart: objWorksheetPart,
									parColumnLetter: strColumnLetter,
									parRowNumber: row,
									parStyleId: uintMatrixColumnStyleID,
									parCellDatatype: CellValues.String);
								//Console.Write("\t + only Formatted...");
								}
							}
						} // end loop for row = 1; row < intRowIndex
					} // foreach dictTechnologyProduct loop

				Console.Write("\n Merging worskeet Cells");
				// add all Merging of the heading columns in topLeft to bottom right order....
				foreach(var mergeItem in dictMergeCells) //.OrderBy(mi => mi.Key).ThenBy(mi => mi.Value))
					{
					Console.Write("\n\t\t + Merge Cells: {0} - {1}", mergeItem.Key, mergeItem.Value);
					oxmlWorkbook.MergeCell(parWorksheetPart: objWorksheetPart,
						parTopLeftCell: mergeItem.Key.Substring(0, mergeItem.Key.IndexOf(":", 0)),
						parBottomRightCell: mergeItem.Value);

					}

				// add the Conditional formatting 
				Console.Write("\n\n Update the Conditional formatting for the matrix");
				strColumnLetter = aWorkbook.GetColumnLetter(intColumnNumber);
				WorksheetExtensionList objWorkSheetExtensionList = objWorksheetPart.Worksheet.Descendants<WorksheetExtensionList>().First();
				if(objWorkSheetExtensionList == null)
					{
					objWorkSheetExtensionList = new WorksheetExtensionList();
					}

				WorksheetExtension objWorksheetExtension = objWorkSheetExtensionList.Descendants<WorksheetExtension>().FirstOrDefault();
				if(objWorksheetExtension == null)
					{
					objWorksheetExtension = new WorksheetExtension();
					objWorksheetExtension.Uri = "{78C0D931 - 6437 - 407d - A8EE - F0AAD7539E65}";
					objWorksheetExtension.AddNamespaceDeclaration("x14", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/main");
					}

				Xl2010.ConditionalFormattings objConditionalFormattings =
					objWorksheetExtension.Descendants<Xl2010.ConditionalFormattings>().FirstOrDefault();
				if(objConditionalFormattings == null)
					{
					objConditionalFormattings = new Xl2010.ConditionalFormattings();
					}

				Xl2010.ConditionalFormatting objConditionalFormatting =
					objConditionalFormattings.Descendants<Xl2010.ConditionalFormatting>().FirstOrDefault();
				if(objConditionalFormatting == null)
					{
					objConditionalFormatting = new Xl2010.ConditionalFormatting();
					objConditionalFormatting.AddNamespaceDeclaration("xm", "http://schemas.microsoft.com/office/excel/2006/main");
					}

				Xl2010.ConditionalFormattingRule objConditionalFormattingRule =
					objConditionalFormatting.Descendants<Xl2010.ConditionalFormattingRule>().FirstOrDefault();
				if(objConditionalFormattingRule == null)
					{
					objConditionalFormattingRule = new Xl2010.ConditionalFormattingRule();
					objConditionalFormattingRule.Type = ConditionalFormatValues.IconSet;
					objConditionalFormattingRule.Priority = 67;
					objConditionalFormattingRule.Id = "{2BAD41AE-FDA8-445C-9D1C-A3FC13701D67}";

					Xl2010.IconSet objIconSet = new Xl2010.IconSet();
					objIconSet.IconSetTypes = Xl2010.IconSetTypeValues.FourTrafficLights;
					objIconSet.ShowValue = false;
					objIconSet.Custom = true;

					Xl2010.ConditionalFormattingValueObject objConditionalFormattingValueObject1 =
						new Xl2010.ConditionalFormattingValueObject();
					objConditionalFormattingValueObject1.Type = Xl2010.ConditionalFormattingValueObjectTypeValues.Numeric;
					Excel.Formula objFormula1 = new Excel.Formula();
					objFormula1.Text = "0";
					objConditionalFormattingValueObject1.Append(objFormula1);

					Xl2010.ConditionalFormattingValueObject objConditionalFormattingValueObject2 =
						new Xl2010.ConditionalFormattingValueObject();
					objConditionalFormattingValueObject2.Type = Xl2010.ConditionalFormattingValueObjectTypeValues.Numeric;
					objConditionalFormattingValueObject2.GreaterThanOrEqual = false;
					Excel.Formula objFormula2 = new Excel.Formula();
					objFormula2.Text = "1";
					objConditionalFormattingValueObject2.Append(objFormula2);

					Xl2010.ConditionalFormattingValueObject objConditionalFormattingValueObject3 =
						new Xl2010.ConditionalFormattingValueObject();
					objConditionalFormattingValueObject3.Type = Xl2010.ConditionalFormattingValueObjectTypeValues.Numeric;
					objConditionalFormattingValueObject3.GreaterThanOrEqual = false;
					Excel.Formula objFormula3 = new Excel.Formula();
					objFormula3.Text = "2";
					objConditionalFormattingValueObject3.Append(objFormula3);

					Xl2010.ConditionalFormattingValueObject objConditionalFormattingValueObject4 =
						new Xl2010.ConditionalFormattingValueObject();
					objConditionalFormattingValueObject4.Type = Xl2010.ConditionalFormattingValueObjectTypeValues.Numeric;
					objConditionalFormattingValueObject4.GreaterThanOrEqual = false;
					Excel.Formula objFormula4 = new Excel.Formula();
					objFormula3.Text = "3";
					objConditionalFormattingValueObject4.Append(objFormula4);

					Xl2010.ConditionalFormattingIcon objConditionalFormattingIcon1 = new Xl2010.ConditionalFormattingIcon();
					objConditionalFormattingIcon1.IconSet = Xl2010.IconSetTypeValues.ThreeSymbols2;
					objConditionalFormattingIcon1.IconId = (UInt32Value)0U;

					Xl2010.ConditionalFormattingIcon objConditionalFormattingIcon2 = new Xl2010.ConditionalFormattingIcon();
					objConditionalFormattingIcon2.IconSet = Xl2010.IconSetTypeValues.ThreeSymbols2;
					objConditionalFormattingIcon2.IconId = (UInt32Value)2U;

					Xl2010.ConditionalFormattingIcon objConditionalFormattingIcon3 = new Xl2010.ConditionalFormattingIcon();
					objConditionalFormattingIcon3.IconSet = Xl2010.IconSetTypeValues.ThreeSymbols;
					objConditionalFormattingIcon3.IconId = (UInt32Value)1U;

					Xl2010.ConditionalFormattingIcon objConditionalFormattingIcon4 = new Xl2010.ConditionalFormattingIcon();
					objConditionalFormattingIcon4.IconSet = Xl2010.IconSetTypeValues.ThreeSymbols;
					objConditionalFormattingIcon4.IconId = (UInt32Value)2U;

					objIconSet.Append(objConditionalFormattingValueObject1);
					objIconSet.Append(objConditionalFormattingValueObject2);
					objIconSet.Append(objConditionalFormattingValueObject3);
					objIconSet.Append(objConditionalFormattingValueObject4);

					objIconSet.Append(objConditionalFormattingIcon1);
					objIconSet.Append(objConditionalFormattingIcon2);
					objIconSet.Append(objConditionalFormattingIcon3);
					objIconSet.Append(objConditionalFormattingIcon4);

					objConditionalFormattingRule.Append(objIconSet);
					}

				Console.WriteLine("ConditionalFormatting Rule with {0} exist...", objConditionalFormattingRule.Type);

				// Check if a ReferenceSequences exist for D4:D4
				Excel.ReferenceSequence objReferenceSequence =
					objConditionalFormatting.Descendants<Excel.ReferenceSequence>().Where(rs => rs.Text.Contains("D4")).FirstOrDefault();
				if(objReferenceSequence == null) // A reference Sequence starting at cell D4 doesn't exist.
					{
					// insert a new reference
					Excel.ReferenceSequence objNewReferenceCSequence = new Excel.ReferenceSequence();
					objNewReferenceCSequence.Text = "D4:" + strColumnLetter + intRowIndex;
					objConditionalFormatting.Append(objConditionalFormattingRule);
					objConditionalFormatting.Append(objNewReferenceCSequence);
					objConditionalFormattings.Append(objConditionalFormatting);
					objWorksheetExtension.Append(objConditionalFormattings);
					objWorkSheetExtensionList.Append(objWorksheetExtension);
					objWorksheetPart.Worksheet.Append(objWorkSheetExtensionList);
					}
				else
					{
					// update the refrence to extend to end of matrix
					objReferenceSequence.Text = "D4:" + strColumnLetter + intRowIndex;
					}
				objWorksheetPart.Worksheet.Save();

				// Insert the Comments...
				// First sort the Comments in row then column sequence...
				Console.WriteLine("\nInsert the Comments into the Worksheet...");
				if(dictComments.Count > 0)
					{
					Dictionary<string, string> dictFinalComments = new Dictionary<string, string>();
					Console.WriteLine("\t Sorting the Comments...");
					foreach(var entryComment in dictComments.OrderBy(dc => dc.Key.RowNumber).ThenBy(dc => dc.Key.ColumnNumber))
						{
						strCheckDuplicate = aWorkbook.GetColumnLetter(entryComment.Key.ColumnNumber) + "|" + entryComment.Key.RowNumber;
						if(!dictFinalComments.TryGetValue(key: strCheckDuplicate, value: out strComment))
							{
							dictFinalComments.Add(
							key: strCheckDuplicate,
							value: entryComment.Value);
							//Console.WriteLine("\t\t + {0}:{1} - {2} ", entryComment.Key.RowNumber, entryComment.Key.ColumnNumber, entryComment.Value);
							}
						}
					// Insert all the comments into the workbook
					aWorkbook.InsertWorksheetComments(
						parWorksheetPart: objWorksheetPart,
						parDictionaryOfComments: dictFinalComments);
					}
				Console.WriteLine("\n\rWorksheet populated....");

Save_and_Close_Document:
//===============================================================

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

				Console.WriteLine(
					"Generation started...: {0} \nGeneration completed: {1} \n Durarion..........: {2}",
					timeStarted, DateTime.Now, (DateTime.Now - timeStarted));

				} // end Try
			catch(ArgumentException exc)
				{
				Console.WriteLine("Exception: {0} - {1}", exc.HResult, exc.Message);
				return false;
				//TODO: raise the error
				}
			catch(Exception exc)
				{
				Console.WriteLine("Exception: {0} - {1}", exc.HResult, exc.Message);
				return false;
				}

			Console.WriteLine("\t\t Complete the generation of {0}", this.DocumentType);
			return true;
			}
		}
	}