﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mshtml;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DrwWp = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DrwWp2010 = DocumentFormat.OpenXml.Office2010.Word.Drawing;
using Drw = DocumentFormat.OpenXml.Drawing;
using Drw2010 = DocumentFormat.OpenXml.Office2010.Drawing;
using Pic = DocumentFormat.OpenXml.Drawing.Pictures;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Threading.Tasks;

namespace DogGenUI
	{
	class HTMLdecoder
		{
		// ------------------
		// Object Properties
		// ------------------

		private Body _wpbody;
		public Body WPbody
			{
			get { return this._wpbody; }
			set { this._wpbody = value; }
			}
		/// <summary>
		/// The Additional Hierarchical Level property contains the number of additional levels that need to be added to the Document Hierarchical Level when processing the HTML contained in a Enhanced Rich Text column/field.
		/// </summary>
		private int _additionalHierarchicalLevel;
		private int AdditionalHierarchicalLevel
			{
			get { return this._additionalHierarchicalLevel; }
			set { this._additionalHierarchicalLevel = value; }
			}

		/// <summary>
		/// The Document Hierarchical Level provides the stating Hierarchical level at which new content will be added to the document.
		/// </summary>
		private int _documentHierarchyLevel;
		public int DocumentHierachyLevel
			{
			get { return this._documentHierarchyLevel; }
			set { this._documentHierarchyLevel = value; }
			}

		private UInt32 _pageWidth;
		private UInt32 PageWidth
			{
			get { return this._pageWidth; }
			set { this._pageWidth = value; }
			}

		// ----------------
		// Object Methods
		// ---------------
		/// <summary>
		/// Use this method once a new HTMLdecoder object is initialised and the 
		/// EndodedHTML property was set to the value of the HTML that has to be decoded.
		/// </summary>
		/// <param name="parDocumentLevel">
		/// Provide the document's hierarchical level at which the HTML has to be inserted.
		/// </param>
		/// <param name="parPageWidth">
		/// </param>
		/// <param name="parHTML2Decode">
		/// </param>
		/// <returns>
		/// returns a boolean value of TRUE if insert was successfull and FALSE if there was any form of failure during the insertion.
		/// </returns>
		public bool DecodeHTML(int parDocumentLevel, UInt32 parPageWidth, string parHTML2Decode)
			{
			Console.WriteLine("HTML to decode: \n\r{0}", parHTML2Decode);
			this.DocumentHierachyLevel = parDocumentLevel;
			this.AdditionalHierarchicalLevel = 0;
			this.PageWidth = parPageWidth;
			// http://stackoverflow.com/questions/11250692/how-can-i-parse-this-html-to-get-the-content-i-want
			IHTMLDocument2 objHTMLDocument2 = (IHTMLDocument2) new HTMLDocument();
			objHTMLDocument2.write(parHTML2Decode);

			//objHTMLDocument.body.innerHTML = this.EncodedHTML;
			//Console.WriteLine("{0}", objHTMLDocument2.body.innerHTML);
			Paragraph objParagraph = new Paragraph();
			objParagraph = oxmlDocument.Construct_Paragraph(1, false);
			ProcessHTMLelements(objHTMLDocument2.body.children, ref objParagraph, false);
			return true;
			}

		private void ProcessHTMLelements(IHTMLElementCollection parHTMLElements, ref Paragraph parExistingParagraph, bool parAppendToExistingParagraph)
			{
			Paragraph objNewParagraph = new Paragraph();
			DocumentFormat.OpenXml.Wordprocessing.Table objTable = new DocumentFormat.OpenXml.Wordprocessing.Table();
			List<UInt32> listTableColumnWidths = new List<UInt32>();

			if(parAppendToExistingParagraph)
				objNewParagraph = parExistingParagraph;
			
			DocumentFormat.OpenXml.Wordprocessing.Run objRun = new DocumentFormat.OpenXml.Wordprocessing.Run();

			if(parHTMLElements.length > 0)
				{
				foreach(IHTMLElement objHTMLelement in parHTMLElements)
					{
					Console.WriteLine("HTMLlevel: {0} - html.tag=<{1}>", this.AdditionalHierarchicalLevel, objHTMLelement.tagName);
					switch(objHTMLelement.tagName)
						{
						//-----------------------
						case "DIV":
						//-----------------------
							if(objHTMLelement.children.length > 0)
								ProcessHTMLelements(objHTMLelement.children, ref objNewParagraph, false);
							else
								{
								objRun = oxmlDocument.Construct_RunText
									(parText2Write: objHTMLelement.innerText);
								}
							break;
						//---------------------------
						case "P": // Paragraph Tag
						//---------------------------
							objNewParagraph = oxmlDocument.Construct_Paragraph(this.DocumentHierachyLevel + this.AdditionalHierarchicalLevel);
							if(objHTMLelement.children.length > 0) // check if there are more html tags in the HTMLelement
								{
								Console.WriteLine("\t{0} child nodes to process", objHTMLelement.children.length);
								// use the DissectHTMLstring method to process the paragraph.
								List<TextSegment> listTextSegments = new List<TextSegment>();
								listTextSegments = TextSegment.DissectHTMLstring (objHTMLelement.innerHTML);
								foreach(TextSegment objTextSegment in listTextSegments)
									{
									objRun = oxmlDocument.Construct_RunText
											(parText2Write: objTextSegment.Text, 
											parBold: objTextSegment.Bold, 
											parItalic: objTextSegment.Italic,
											parUnderline: objTextSegment.Undeline,
											parSubscript: objTextSegment.Subscript,
											parSuperscript: objTextSegment.Superscript);
									objNewParagraph.Append(objRun);
									}
								}
							else  // there are no cascading tags, just write the text if there are any
								{
								if(objHTMLelement.innerText.Length > 0)
									{
									objRun = oxmlDocument.Construct_RunText(parText2Write: objHTMLelement.innerText);
									objNewParagraph.Append(objRun);
									}
								}
							if(parAppendToExistingParagraph)
								//ignore because only a new Paragraph needs to be appended to the body
								Console.WriteLine("Skip the appending of the existing paragraph to the Body");
							else
								{
								this.WPbody.Append(objNewParagraph);
								}
							break;
						//------------------------------------
						case "TABLE":
							Console.WriteLine("Tag: TABLE\n{0}", objHTMLelement.outerHTML);
							// Check for cascading tables
							Single iiTableWidthValue = 0;
							string TableWithUnit = "";
							if(objHTMLelement.outerHTML.IndexOf("WIDTH", 1) >= 0)
								{
								TableWithUnit = objHTMLelement.style.width;
								if(TableWithUnit.IndexOf("%", 1) > 0)
									{
									if(!Single.TryParse(TableWithUnit.Substring(0, (TableWithUnit.Length - TableWithUnit.IndexOf("%", 1)) - 1), out iiTableWidthValue))
										{
										iiTableWidthValue = 100;
										}
									}
								else
									{
									iiTableWidthValue = 100;
									}
								}
							else
								{
								iiTableWidthValue = 100;
								}

							// Calculate the width of the table on the page.
							Console.WriteLine("Pagewidth: {0}", this.PageWidth);
							Console.WriteLine("Table Width: {0}%", iiTableWidthValue);
							// the constant of 50 used below; is the equivelent of the 50ths of 1% width giving a Pct value
							UInt32 tableWidth = Convert.ToUInt32(iiTableWidthValue * 50);
							objTable = oxmlDocument.ConstructTable(parTableWidth: tableWidth, 
								parFirstRow: true, 
								parFirstColumn: true, 
								parLastColumn: true, 
								parLastRow: true, 
								parNoVerticalBand: true, 
								parNoHorizontalBand: false);
							
							if(objHTMLelement.children.length > 0)
								ProcessHTMLelements(objHTMLelement.children, ref objNewParagraph, false);
							WPbody.Append(objTable);
							break;
						//------------------------------------
						case "TBody": // Table Body
							Console.WriteLine("Tag: TABLE Body \n\r{0}", objHTMLelement.outerHTML);
							if(objHTMLelement.children.length > 0)
								ProcessHTMLelements(parHTMLElements: objHTMLelement.children, parExistingParagraph: ref objNewParagraph, parAppendToExistingParagraph: false);
							break;
						//------------------------------------
						case "TR":     // Table Row
							Console.WriteLine("Tag: TABLE Row\n\r{0}", objHTMLelement.outerHTML);
							Console.WriteLine("TableRow class: {0}", objHTMLelement.className);
							//Check the type of Table row
							// If HeaderRow
							if(objHTMLelement.className.Contains("HeaderRow"))
								{
								TableProperties objTableProperties = objTable.GetFirstChild<TableProperties>();
								TableLook objTableLook = objTableProperties.GetFirstChild<TableLook>();
								objTableLook.FirstRow = true;
								}
							// If FooterRow
							else if(objHTMLelement.className.Contains("FooterRow"))
								{
								TableProperties objTableProperties = objTable.GetFirstChild<TableProperties>();
								TableLook objTableLook = objTableProperties.GetFirstChild<TableLook>();
								objTableLook.LastRow = true;
								}
							if(objHTMLelement.children.length > 0)
								ProcessHTMLelements(parHTMLElements: objHTMLelement.children, parExistingParagraph: ref objNewParagraph, parAppendToExistingParagraph: false);
							if(objHTMLelement.className.Contains("HeaderRow"))
								if(listTableColumnWidths.Count > 0) //the table actually contains columns
									{
									Single sngTotalColumnWidth = 0;
									foreach(Single columnWidthItem in listTableColumnWidths)
										{
										sngTotalColumnWidth += columnWidthItem;
										}
									// Create table grid
									

									TableGrid objTableGrid = new TableGrid();
									objTableGrid = oxmlDocument.ConstructTableGrid(listTableColumnWidths);

									}
								break;
						//------------------------------------
						case "TH":     // Table Header
							Console.WriteLine("Tag: TABLE Header\n\r{0}", objHTMLelement.outerHTML);
							Console.WriteLine("\tAttribute<colspan>: {0}", objHTMLelement.getAttribute(strAttributeName: "colspan"));
							Console.WriteLine("\tAttribute<colspan>: {0}", objHTMLelement.getAttribute(strAttributeName: "rowspan"));
							Console.WriteLine("\tStyle=width: {0}", objHTMLelement.style.width);
							Console.WriteLine("\tStyle=height: {0}", objHTMLelement.style.height);
							listTableColumnWidths.Add(Convert.ToSingle(objHTMLelement.style.width));


							break;
						//------------------------------------
						case "TD":     // Table Cell
							Console.WriteLine("Tag: TABLE Cell\n\r{0}", objHTMLelement.outerHTML);

							break;
						//------------------------------------
						case "UL":     // Unorganised List (Bullets to follow) Tag
							Console.WriteLine("Tag: UNORGANISED LIST\n\r{0}", objHTMLelement.outerHTML);

							break;
						//------------------------------------
						case "OL":     // Orginised List (numbered list) Tag
							Console.WriteLine("Tag: ORGANISED LIST\n\r{0}", objHTMLelement.outerHTML);
							break;
						//------------------------------------
						case "LI":     // List Item (an entry from a organised or unorginaised list
							Console.WriteLine("Tag: LIST ITEM\n\r{0}", objHTMLelement.outerHTML);
							break;
						//------------------------------------
						case "IMG":    // Image Tag
							Console.WriteLine("Tag:IMAGE \n\r{0}", objHTMLelement.outerHTML);
                                   break;
						case "STRONG": // Bold Tag
							Console.WriteLine("TAG: BOLD\n\r{0}", objHTMLelement.outerHTML);
							//this.BoldOn = true;
							//if(objHTMLelement.children.length > 0)
							//	{
							//	// use the DissectHTMLstring method to process the paragraph.
							//	List<TextSegment> listTextSegments = new List<TextSegment>();
							//	listTextSegments = TextSegment.DissectHTMLstring(objHTMLelement.innerHTML);
							//	foreach(TextSegment objTextSegment in listTextSegments)
							//		{
							//		objRun = oxmlDocument.Construct_RunText
							//				(parText2Write: objTextSegment.Text,
							//				parBold: objTextSegment.Bold,
							//				parItalic: objTextSegment.Italic,
							//				parUnderline: objTextSegment.Undeline,
							//				parSubscript: objTextSegment.Subscript,
							//				parSuperscript: objTextSegment.Superscript);
							//		objNewParagraph.Append(objRun);
							//		}
							//	}
							//else  // there are no cascading tags, just append the text to an existing paragrapg object
							//	{
							//	if(objHTMLelement.innerText.Length > 0)
							//		{
							//		objRun = oxmlDocument.Construct_RunText
							//			(parText2Write: objHTMLelement.innerText,
							//			parBold: this.BoldOn,
							//			parItalic: this.ItalicsOn,
							//			parUnderline: this.UnderlineOn);
							//		objNewParagraph.Append(objRun);
							//		}
							//	}
							//this.BoldOn = false;
							break;
						//------------------------------------
						case "SPAN":   // Underline is embedded in the Span tag
							Console.WriteLine("Tag: Span\n\r{0}", objHTMLelement.outerHTML);
							//if (objHTMLelement.outerHTML.IndexOf("TEXT-DECORATION: underline") > 0) 
							//	//  == "span style=" + "" + "text-styleTextDecoration;underline;" + "" + ">" )
							//	{

								//this.UnderlineOn = true;
								//if(objHTMLelement.children.length > 0)
								//	{
								//	// use the DissectHTMLstring method to process the paragraph.
								//	List<TextSegment> listTextSegments = new List<TextSegment>();
								//	listTextSegments = TextSegment.DissectHTMLstring(objHTMLelement.innerHTML);
								//	foreach(TextSegment objTextSegment in listTextSegments)
								//		{
								//		objRun = oxmlDocument.Construct_RunText
								//				(parText2Write: objTextSegment.Text,
								//				parBold: objTextSegment.Bold,
								//				parItalic: objTextSegment.Italic,
								//				parUnderline: objTextSegment.Undeline,
								//				parSubscript: objTextSegment.Subscript,
								//				parSuperscript: objTextSegment.Superscript);
								//		objNewParagraph.Append(objRun);
								//		}
								//	}
								//else  // there are no cascading tags, just append the text to an existing paragrapg object
								//	{
								//	if(objHTMLelement.innerText.Length > 0)
								//		{
								//		objRun = oxmlDocument.Construct_RunText
								//			(parText2Write: objHTMLelement.innerText,
								//			parBold: this.BoldOn,
								//			parItalic: this.ItalicsOn,
								//			parUnderline: this.UnderlineOn);
								//		objNewParagraph.Append(objRun);
								//		}
								//	}
								//this.UnderlineOn = false;
								//}
							break;
						//------------------------------------
						case "EM":     // Italic Tag
							Console.WriteLine("Tag: ITALIC\n\r{0}", objHTMLelement.outerHTML);
//							this.ItalicsOn = true;
//							if(objHTMLelement.children.length > 0)
//								{
//								// use the DissectHTMLstring method to process the paragraph.
//								List<TextSegment> listTextSegments = new List<TextSegment>();
//								listTextSegments = TextSegment.DissectHTMLstring(objHTMLelement.innerHTML);
//								foreach(TextSegment objTextSegment in listTextSegments)
//									{
//									objRun = oxmlDocument.Construct_RunText
//											(parText2Write: objTextSegment.Text,
//											parBold: objTextSegment.Bold,
//											parItalic: objTextSegment.Italic,
//											parUnderline: objTextSegment.Undeline,
//											parSubscript: objTextSegment.Subscript,
//											parSuperscript: objTextSegment.Superscript);
//									objNewParagraph.Append(objRun);
//									}

//}
//							else  // there are no cascading tags, just append the text to an existing paragrapg object
//								{
//								if(objHTMLelement.innerText.Length > 0)
//									{
//									objRun = oxmlDocument.Construct_RunText
//										(parText2Write: objHTMLelement.innerText,
//										parBold: this.BoldOn,
//										parItalic: this.ItalicsOn,
//										parUnderline: this.UnderlineOn);
//									objNewParagraph.Append(objRun);
//									}
//								}
//							this.ItalicsOn = false;
							break;
						//------------------------------------
						case "SUB":    // Subscript Tag
							Console.WriteLine("Tag: SUPERSCRIPT\n\r{0}", objHTMLelement.outerHTML);
							break;
						//------------------------------------
						case "SUP":    // Super Script Tag
							Console.WriteLine("Tag: SUPERSCRIPT\n\r{0}", objHTMLelement.outerHTML);
							break;
						//------------------------------------
						case "H1":     // Heading 1
						case "H1A":    // Alternate Heading 1
							Console.WriteLine("Tag: H1\n\r{0}", objHTMLelement.outerHTML);
							this.AdditionalHierarchicalLevel = 1;
							objNewParagraph = oxmlDocument.Insert_Heading(
								parHeadingLevel: this.DocumentHierachyLevel + this.AdditionalHierarchicalLevel, 
								parText2Write: objHTMLelement.innerText,
								parRestartNumbering: false);
							this.WPbody.Append(objNewParagraph);
							break;
						//------------------------------------
						case "H2":     // Heading 2
						case "H2A":    // Alternate Heading 2
							Console.WriteLine("Tag: H2\n\r{0}", objHTMLelement.outerHTML);
							this.AdditionalHierarchicalLevel = 2;
							objNewParagraph = oxmlDocument.Insert_Heading(
								parHeadingLevel: this.DocumentHierachyLevel + this.AdditionalHierarchicalLevel, 
								parText2Write: objHTMLelement.innerText,
								parRestartNumbering: false);
							this.WPbody.Append(objNewParagraph);
							break;
						//------------------------------------
						case "H3":     // Heading 3
						case "H3A":    // Alternate Heading 3
							Console.WriteLine("Tag: H3\n\r{0}", objHTMLelement.outerHTML);
							this.AdditionalHierarchicalLevel = 3;
							objNewParagraph = oxmlDocument.Insert_Heading(
								parHeadingLevel: this.DocumentHierachyLevel + this.AdditionalHierarchicalLevel, 
								parText2Write: objHTMLelement.innerText,
								parRestartNumbering: false);
							this.WPbody.Append(objNewParagraph);
							break;
						//------------------------------------
						case "H4":     // Heading 4
						case "H4A":    // Alternate Heading 4
							Console.WriteLine("Tag: H4\n\r{0}", objHTMLelement.outerHTML);
							this.AdditionalHierarchicalLevel = 4;
							objNewParagraph = oxmlDocument.Insert_Heading(
								parHeadingLevel: this.DocumentHierachyLevel + this.AdditionalHierarchicalLevel, 
								parText2Write: objHTMLelement.innerText,
								parRestartNumbering: false);
							this.WPbody.Append(objNewParagraph);
							break;
						default:
							Console.WriteLine("**** ignoring tag: {0}", objHTMLelement.tagName);
							break;

						} // switch(objHTMLelement.tagName)


					} // foreach(IHTMLElement objHTMLelement in parHTMLElements)


				} // if (parHTMLElements.length > 0)


			}

		}    // end of Class
	class TextSegment
		{
		private bool _bold;
		public bool Bold
			{
			get {return this._bold;}
			set{this._bold = value;}
			}
		private bool _italic;
		public bool Italic
			{
			get{return this._italic;}
			set{this._italic = value;}
			}
		private bool _undeline;
		public bool Undeline
			{
			get{return this._undeline;}
			set{this._undeline = value;}
			}
		private bool _subscript;
		public bool Subscript
			{
			get{return this._subscript;}
			set{this._subscript = value;}
			}
		private bool _superscript;
		public bool Superscript
			{
			get{return this._superscript;}
			set{this._superscript = value;}
			}
		private string _text;
		public string Text
			{
			get{return this._text;}
			set{this._text = value;}
			}

		public static List<TextSegment> DissectHTMLstring(string parTextString)
			{
			int i = 0;
			int iPointer = 0;
			int iOpenTagStart = 0;
			int iOpenTagEnds = 0;
			string sOpenTag = "";
			int iCloseTagStart = 0;
			int iCloseTagEnds = 0;
			string sCloseTag = "";
			bool bBold = false;
			bool bItalic = false;
			bool bUnderline = false;
			bool bSuperScript = false;
			bool bSubscript = false;
			int iNextTagStart = 0;
			int iNextTagEnds = 0;
			string sNextTag = "";
			List<TextSegment> listTextSegments = new List<TextSegment>();

			//-----------------------------------------------------------
			// replace and/or remove special strings before processing the Text Segment... 
			parTextString = parTextString.Replace(oldValue: "&quot;", newValue: Convert.ToString(value: (char) 22));
			parTextString = parTextString.Replace(oldValue: "&nbsp;", newValue: "");
			parTextString = parTextString.Replace(oldValue: "&#160;", newValue: "");
			parTextString = parTextString.Replace(oldValue: "  ", newValue: " ");
			Console.WriteLine("\t\t\tString to examine:\r\t\t\t|{0}|", parTextString);

			do
				{
				iNextTagStart = parTextString.IndexOf("<", iPointer);
				if(iNextTagStart < 0) // Check if there are any tags left to process
					break;
				iNextTagEnds = parTextString.IndexOf(">", iPointer);
				sNextTag = parTextString.Substring(iNextTagStart, (iNextTagEnds - iNextTagStart) + 1);
				if(sNextTag.IndexOf("/") < 0) // it is an Open tag
					{
					// Check if there are any text BEFORE the tag
					if(iNextTagStart > iPointer)
						{
						//extract the text before the first tag and place it in the List of TextSegments
						TextSegment objTextSegment = new TextSegment();
						objTextSegment.Text = parTextString.Substring(iPointer, (iNextTagStart - iPointer));
						objTextSegment.Bold = bBold;
						objTextSegment.Italic = bItalic;
						objTextSegment.Undeline = bUnderline;
						objTextSegment.Subscript = bSubscript;
						objTextSegment.Superscript = bSuperScript;
						listTextSegments.Add(objTextSegment);
						Console.WriteLine("\t\t\t** {0}", objTextSegment.Text);
						iPointer = iNextTagStart;
						}
					// Determine the START
					iOpenTagStart = iNextTagStart;
					iOpenTagEnds = iNextTagEnds;
					sOpenTag = sNextTag;
					Console.WriteLine("\t\t\t\t- OpenTag: {0} = {1} - {2}", sOpenTag, iOpenTagStart, iOpenTagEnds);
					// Define the corresponding closing tag
					if(sOpenTag.IndexOf("STRONG") > 0)
						{
						sCloseTag = "</STRONG>";
						bBold = true;
						}
					else if(sOpenTag.IndexOf("EM>") > 0)
						{
						sCloseTag = "</EM>";
						bItalic = true;
						}
					else if(sOpenTag.IndexOf("underline") > 0)
						{
						sCloseTag = "</SPAN>";
						bUnderline = true;
						}
					else if(sOpenTag.IndexOf("SUB") > 0)
						{
						sCloseTag = "</SUB>";
						bSubscript = true;
						}
					else if(sOpenTag.IndexOf("SUP") > 0)
						{
						sCloseTag = "</SUP>";
						bSuperScript = true;
						}
					else
						sCloseTag = "";

					iCloseTagStart = parTextString.IndexOf(value: sCloseTag, startIndex: iOpenTagStart + sOpenTag.Length);
					if(iCloseTagStart < 0)
						// what if the close tag is not found?
						Console.WriteLine("ERROR: {0} - not found!", sCloseTag);
					else
						{
						iCloseTagEnds = iCloseTagStart + sCloseTag.Length - 1;
						Console.WriteLine("\t\t\t\t- CloseTag: {0} = {1} - {2}", sCloseTag, iCloseTagStart, iCloseTagEnds);
						//iPointer = iOpenTagEnds + 1;
						}
					iPointer = iOpenTagEnds + 1;
					}
				else  // it is a CLOSE tag
					{
					// Check if there are any text BEFORE the tag
					if(iNextTagStart > iPointer)
						{
						//extract the text before the first tag and place it in the List of TextSegments
						TextSegment objTextSegment = new TextSegment();
						objTextSegment.Text = parTextString.Substring(iPointer, (iNextTagStart - iPointer));
						objTextSegment.Bold = bBold;
						objTextSegment.Italic = bItalic;
						objTextSegment.Undeline = bUnderline;
						objTextSegment.Subscript = bSubscript;
						objTextSegment.Superscript = bSuperScript;
						listTextSegments.Add(objTextSegment);
						Console.WriteLine("\t\t\t** {0}", objTextSegment.Text);
						}
					// Obtain the Close Tag
					iCloseTagStart = iNextTagStart;
					iCloseTagEnds = iNextTagEnds;
					sCloseTag = sNextTag;
					Console.WriteLine("\t\t\t\t- CloseTag: {0} = {1} - {2}", sCloseTag, iCloseTagStart, iCloseTagEnds);
					// Depending on the closing tag set the text emphasis off
					if(sCloseTag.IndexOf("/STRONG") > 0)
						bBold = false;
					if(sCloseTag.IndexOf("/EM") > 0)
						bItalic = false;
					if(sCloseTag.IndexOf("/SPAN") > 0)
						bUnderline = false;
					if(sCloseTag.IndexOf("/SUB") > 0)
						bSubscript = false;
					if(sCloseTag.IndexOf("/SUP") > 0)
						bSuperScript = false;
					iPointer = iNextTagEnds + 1;
					} // if it is a Close Tag

				} while(iPointer < parTextString.Length);

			//checked if there are trailing characters that need to be processed.
			if(iPointer < parTextString.Length)
				{
				//extract the text pointer until the end of the string place it in the List of TextSegments
				TextSegment objTextSegment = new TextSegment();
				objTextSegment.Text = parTextString.Substring(iPointer, (parTextString.Length - iPointer));
				objTextSegment.Bold = bBold;
				objTextSegment.Italic = bItalic;
				objTextSegment.Undeline = bUnderline;
				objTextSegment.Subscript = bSubscript;
				objTextSegment.Superscript = bSuperScript;
				listTextSegments.Add(objTextSegment);
				iPointer = parTextString.Length;
				Console.WriteLine("\t\t\t** {0}", objTextSegment.Text);
				}

			i = 0;
			foreach(TextSegment objTextSegmentItem in listTextSegments)
				{
				i += 1;
				Console.WriteLine("\t\t+ {0}: {1} (Bold:{2} Italic:{3} Underline:{4} Subscript:{5} Superscript:{6}",
					i, objTextSegmentItem.Text, objTextSegmentItem.Bold, objTextSegmentItem.Italic, objTextSegmentItem.Undeline, objTextSegmentItem.Subscript,
					objTextSegmentItem.Subscript);
				}

			return listTextSegments;

			} // end method

		} // end class
	}
