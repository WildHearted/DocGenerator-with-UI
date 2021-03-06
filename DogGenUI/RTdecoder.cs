﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mshtml;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Threading.Tasks;

namespace DocGeneratorCore
	{
	/// <summary>
	/// The RTdecoder or Rich Text Decoder is used to decode Rich Text (not Enhanced Rich Text - use the HTMLdecoder to decode Enhanced RichText).
	/// RTdecoder will not process images and tables.
	/// </summary>
	class RTdecoder
		{
		// ------------------
		// Object Properties
		// ------------------
		private List<Paragraph> _paragraphList = new List<Paragraph>();
		public List<Paragraph> ParagraphList
			{
			get{return this._paragraphList;}
			set{this._paragraphList = value;}
			}

		/// <summary>
		/// The Document Hierarchical Level provides the stating Hierarchical level at which new content will be added to the document.
		/// </summary>	
		private int _hierarchyLevel = 0;
		public int HierachyLevel
			{
			get{return this._hierarchyLevel;}
			set{this._hierarchyLevel = value;}
			}
		/// <summary>
		/// The Additional Hierarchical Level property contains the number of additional levels that need to be added to the 
		/// Document Hierarchical Level when processing the HTML contained in a Enhanced Rich Text column/field.
		/// </summary>
		private int _additionalHierarchicalLevel = 0;
		private int AdditionalHierarchicalLevel
			{
			get{return this._additionalHierarchicalLevel;}
			set{this._additionalHierarchicalLevel = value;}
			}

		private bool _isTableText = false;
		public bool IsTableText
			{
			get{return this._isTableText;}
			set{this._isTableText = value;}
			}
		private string _contentLayer = "None";
		public string ContentLayer
			{
			get{return this._contentLayer;}
			set{this._contentLayer = value;}
			}

		// Procedures/Methods
		public List<Paragraph> DecodeRichText(
			string parRT2decode,
			string parContentLayer = "None",
			int parHierarchicalLevel = 0,
			bool parIsTableText = false)
			{
			this.ContentLayer = parContentLayer;
			this.HierachyLevel = parHierarchicalLevel;
			this.AdditionalHierarchicalLevel = 0;
			this.IsTableText = parIsTableText;

			try
				{
				// move the content to be decoded into a IHTMLDocument object in order to process the HTML structure.
				IHTMLDocument2 objHTMLDocument2 = (IHTMLDocument2)new HTMLDocument();
				objHTMLDocument2.write(parRT2decode);

				// Process the HTML contained in the RT and validate whether it was successfull.
				ProcessElements(objHTMLDocument2.body.children);

				return this.ParagraphList;
				}

			catch(InvalidTableFormatException exc)
				{
				Console.WriteLine("\n\nException: {0} - {1}", exc.Message, exc.Data);
				throw new InvalidRichTextFormatException(exc.Message);
				}

			catch(InvalidImageFormatException exc)
				{
				Console.WriteLine("\n\nException: {0} - {1}", exc.Message, exc.Data);
				throw new InvalidRichTextFormatException(exc.Message);
				}

			catch(GeneralException exc)
				{
				Console.WriteLine("\n\nException: {0} - {1}", exc.Message, exc.Data);
				throw new InvalidRichTextFormatException("Unexpected Error occurred.\nError Detail: " + exc.Message);
				}

			catch(Exception exc)
				{
				Console.WriteLine("\n**** Exception **** \n\t{0} - {1}\n\t{2}", exc.HResult, exc.Message, exc.StackTrace);
				throw new InvalidRichTextFormatException("An unexpected error occurred at this point, in the document generation. \nError detail: " + exc.Message);
				}

			}

		private void ProcessElements(IHTMLElementCollection parHTMLelements)
			{
			Paragraph objParagraph = new Paragraph();
			Run objRun = new Run();
			try
				{
				Console.WriteLine("parHTMLElements.length = {0}", parHTMLelements.length);
				if(parHTMLelements.length < 1) // there are no cascading HTML content
					{
					objRun = oxmlDocument.Construct_RunText(parText2Write: " ");
					objParagraph = oxmlDocument.Construct_Paragraph(parBodyTextLevel: this.HierachyLevel, parIsTableParagraph: this.IsTableText);
					objParagraph.Append(objRun);
					this.ParagraphList.Add(objParagraph);
					return;
					}

				foreach(IHTMLElement objHTMLelement in parHTMLelements)
					{
					Console.WriteLine("HTMLlevel: {0} - html.tag=<{1}>\n\t|{2}|", this.AdditionalHierarchicalLevel,
						objHTMLelement.tagName,
						objHTMLelement.innerHTML);
					switch(objHTMLelement.tagName)
						{
					//-----------
					case "DIV":
						//-----------
						if(objHTMLelement.children.length > 0)
							ProcessElements(objHTMLelement.children);
						else
							{
							if(objHTMLelement.innerText != null)
								{
								objParagraph = oxmlDocument.Construct_Paragraph(
									parBodyTextLevel: this.HierachyLevel + this.AdditionalHierarchicalLevel,
									parIsTableParagraph: this.IsTableText);
								objRun = oxmlDocument.Construct_RunText(parText2Write: objHTMLelement.innerText);
								objParagraph.Append(objRun);
								this.ParagraphList.Add(objParagraph);
								}
							}
						break;
					// ---------------------------
					case "P": // Paragraph Tag
							//---------------------------
						if(objHTMLelement.innerText != null)
							{
							objParagraph = oxmlDocument.Construct_Paragraph(this.HierachyLevel + this.AdditionalHierarchicalLevel, this.IsTableText);
							if(objHTMLelement.children.length > 0) // check if there are more html tags in the HTMLelement
								{
								// use the DissectHTMLstring method to process the paragraph.
								List<TextSegment> listTextSegments = new List<TextSegment>();
								listTextSegments = TextSegment.DissectHTMLstring(objHTMLelement.innerHTML);
								// Process the list to insert the content into Paragraph List
								foreach(TextSegment objTextSegment in listTextSegments)
									{
									if(objTextSegment.Image) // Check if it is an image
										{
										if(this.IsTableText)
											throw new InvalidTableFormatException("Attempted to insert a image into a table.");
										else
											throw new InvalidImageFormatException("Rich Text is not suppose to contain an Image");
										}
									else // not an image
										{
										objRun = oxmlDocument.Construct_RunText
											(parText2Write: objTextSegment.Text,
											parContentLayer: this.ContentLayer,
											parBold: objTextSegment.Bold,
											parItalic: objTextSegment.Italic,
											parUnderline: objTextSegment.Undeline,
											parSubscript: objTextSegment.Subscript,
											parSuperscript: objTextSegment.Superscript);
										objParagraph.Append(objRun);
										}
									} // foreach loop end
								this.ParagraphList.Add(objParagraph);
								}
							else  // there are no cascading tags, just write the text if there are any
								{
								if(objHTMLelement.innerText.Length > 0)
									{
									if(!objHTMLelement.outerHTML.Contains("<P></P>"))
										{
										objRun = oxmlDocument.Construct_RunText(parText2Write:
											objHTMLelement.innerText,
											parContentLayer: this.ContentLayer);
										objParagraph.Append(objRun);
										this.ParagraphList.Add(objParagraph);
										}
									}
								} // there are no cascading tags
							} // if(objHTMLelement.innerText != null)
						break;
					//-----------------
					case "TABLE":
						//-----------------
						if(this.IsTableText)
							{
							throw new InvalidTableFormatException("Attempted to insert a table into a table (no cascading tables allowed). Please remove the table from the content.");
							}
						else
							{
							throw new InvalidTableFormatException("Rich Text is not suppose to contain a Table. Please remove the table from the content.");
							}
					//----------------------------
					case "TBODY": // Table Body
					case "TR":     // Table Row
					case "TH":     // Table Header
					case "TD":     // Table Cell
								//----------------------------
						Console.WriteLine("Ingnore all Table related tags.");
						break;
					//------------------------------------
					case "OL": // Orginised List (numbered list) Tag
							 //-----------------------------------
							 //Console.WriteLine("Tag: ORGANISED LIST\n{0}", objHTMLelement.outerHTML);
						if(objHTMLelement.children.length > 0)
							{
							ProcessElements(objHTMLelement.children);
							}
						break;
					//----------------------
					case "LI":    // List Item (an entry from a organised or unorginaised list
							    //----------------------
						if(objHTMLelement.parentElement.tagName == "OL") // number list
							{
							objParagraph = oxmlDocument.Construct_BulletNumberParagraph(
								parIsBullet: this.IsTableText,
								parBulletLevel: this.HierachyLevel + this.AdditionalHierarchicalLevel);
							}
						else // "UL" == Unorganised/Bullet list item
							{
							objParagraph = oxmlDocument.Construct_BulletNumberParagraph(
								parIsBullet: this.IsTableText,
								parBulletLevel: this.HierachyLevel + this.AdditionalHierarchicalLevel);
							}
						if(objHTMLelement.children.length > 0) // check if there are more html tags in the HTMLelement
							{
							// use the DissectHTMLstring method to process the paragraph.
							List<TextSegment> listTextSegments = new List<TextSegment>();
							listTextSegments = TextSegment.DissectHTMLstring(objHTMLelement.innerHTML);
							foreach(TextSegment objTextSegment in listTextSegments)
								{
								objRun = oxmlDocument.Construct_RunText
									(parText2Write: objTextSegment.Text,
									parContentLayer: this.ContentLayer,
									parBold: objTextSegment.Bold,
									parItalic: objTextSegment.Italic,
									parUnderline: objTextSegment.Undeline,
									parSubscript: objTextSegment.Subscript,
									parSuperscript: objTextSegment.Superscript);
								objParagraph.Append(objRun);
								}
							this.ParagraphList.Add(objParagraph);
							}
						else  // there are no cascading tags, just write the text if there are any
							{
							if(objHTMLelement.innerText.Length > 0)
								{
								objRun = oxmlDocument.Construct_RunText(parText2Write: objHTMLelement.innerText);
								objParagraph.Append(objRun);
								this.ParagraphList.Add(objParagraph);
								}
							}

						break;
					// -------------------------
					//+ Image Tag

					case "IMG":
						throw new InvalidImageFormatException("Rich Text is not suppose to contain any Images. Please remove the image form the content.");

					// ----------------------------------
					//+ Bold Text
					case "STRONG": 
						if(objHTMLelement.innerText != null)
							{
							objParagraph = oxmlDocument.Construct_Paragraph(this.HierachyLevel + this.AdditionalHierarchicalLevel, this.IsTableText);
							if(objHTMLelement.children.length > 0) // check if there are more html tags in the HTMLelement
								{
								// use the DissectHTMLstring method to process the paragraph.
								List<TextSegment> listTextSegments = new List<TextSegment>();
								listTextSegments = TextSegment.DissectHTMLstring(objHTMLelement.innerHTML);
								// Process the list to insert the content into Paragraph List
								foreach(TextSegment objTextSegment in listTextSegments)
									{
									if(objTextSegment.Image) // Check if it is an image
										{
										throw new InvalidImageFormatException("Rich Text is not suppose to contain an Image");
										}
									else // not an image
										{
										objRun = oxmlDocument.Construct_RunText
											(parText2Write: objTextSegment.Text,
											parContentLayer: this.ContentLayer,
											parBold: true,
											parItalic: objTextSegment.Italic,
											parUnderline: objTextSegment.Undeline,
											parSubscript: objTextSegment.Subscript,
											parSuperscript: objTextSegment.Superscript);
										objParagraph.Append(objRun);
										}
									} // foreach loop end
								this.ParagraphList.Add(objParagraph);
								}
							else  // there are no cascading tags, just write the text if there are any
								{
								if(objHTMLelement.innerText.Length > 0)
									{
									if(!objHTMLelement.outerHTML.Contains("<P></P>"))
										{
										objRun = oxmlDocument.Construct_RunText(
											parText2Write: objHTMLelement.innerText,
											parContentLayer: this.ContentLayer,
											parBold: true);
										objParagraph.Append(objRun);
										this.ParagraphList.Add(objParagraph);
										}
									}
								} // there are no cascading tags
							} // if(objHTMLelement.innerText != null)
						break;
					// --------------------------
					//+ Italic Tag
					case "EM":
						if(objHTMLelement.innerText != null)
							{
							objParagraph = oxmlDocument.Construct_Paragraph(this.HierachyLevel + this.AdditionalHierarchicalLevel, this.IsTableText);
							if(objHTMLelement.children.length > 0) // check if there are more html tags in the HTMLelement
								{
								// use the DissectHTMLstring method to process the paragraph.
								List<TextSegment> listTextSegments = new List<TextSegment>();
								listTextSegments = TextSegment.DissectHTMLstring(objHTMLelement.innerHTML);
								// Process the list to insert the content into Paragraph List
								foreach(TextSegment objTextSegment in listTextSegments)
									{
									if(objTextSegment.Image) // Check if it is an image
										{
										throw new InvalidImageFormatException("Rich Text is not suppose to contain an Image");
										}
									else // not an image
										{
										objRun = oxmlDocument.Construct_RunText
											(parText2Write: objTextSegment.Text,
											parContentLayer: this.ContentLayer,
											parBold: objTextSegment.Bold,
											parItalic: true,
											parUnderline: objTextSegment.Undeline,
											parSubscript: objTextSegment.Subscript,
											parSuperscript: objTextSegment.Superscript);
										objParagraph.Append(objRun);
										}
									} // foreach loop end
								this.ParagraphList.Add(objParagraph);
								}
							else  // there are no cascading tags, just write the text if there are any
								{
								if(objHTMLelement.innerText.Length > 0)
									{
									if(!objHTMLelement.outerHTML.Contains("<P></P>"))
										{
										objRun = oxmlDocument.Construct_RunText(
											parText2Write: objHTMLelement.innerText,
											parContentLayer: this.ContentLayer,
											parItalic: true);
										objParagraph.Append(objRun);
										this.ParagraphList.Add(objParagraph);
										}
									}
								} // there are no cascading tags
							} // if(objHTMLelement.innerText != null)
						break;
					//------------------------
					case "SUB":  // Subscript
							   //------------------------
						if(objHTMLelement.innerText != null)
							{
							objParagraph = oxmlDocument.Construct_Paragraph(this.HierachyLevel + this.AdditionalHierarchicalLevel, this.IsTableText);
							if(objHTMLelement.children.length > 0) // check if there are more html tags in the HTMLelement
								{
								// use the DissectHTMLstring method to process the paragraph.
								List<TextSegment> listTextSegments = new List<TextSegment>();
								listTextSegments = TextSegment.DissectHTMLstring(objHTMLelement.innerHTML);
								// Process the list to insert the content into Paragraph List
								foreach(TextSegment objTextSegment in listTextSegments)
									{
									if(objTextSegment.Image) // Check if it is an image
										{
										throw new InvalidImageFormatException("Rich Text is not suppose to contain an Image");
										}
									else // not an image
										{
										objRun = oxmlDocument.Construct_RunText
											(parText2Write: objTextSegment.Text,
											parContentLayer: this.ContentLayer,
											parBold: objTextSegment.Bold,
											parItalic: objTextSegment.Italic,
											parUnderline: objTextSegment.Undeline,
											parSubscript: true,
											parSuperscript: objTextSegment.Superscript);
										objParagraph.Append(objRun);
										}
									} // foreach loop end
								this.ParagraphList.Add(objParagraph);
								}
							else  // there are no cascading tags, just write the text if there are any
								{
								if(objHTMLelement.innerText.Length > 0)
									{
									if(!objHTMLelement.outerHTML.Contains("<P></P>"))
										{
										objRun = oxmlDocument.Construct_RunText(
											parText2Write: objHTMLelement.innerText,
											parContentLayer: this.ContentLayer,
											parSubscript: true);
										objParagraph.Append(objRun);
										this.ParagraphList.Add(objParagraph);
										}
									}
								} // there are no cascading tags
							} // if(objHTMLelement.innerText != null)
						break;
					//------------------------
					case "SUP":  // Superscript
							   //------------------------
						if(objHTMLelement.innerText != null)
							{
							objParagraph = oxmlDocument.Construct_Paragraph(this.HierachyLevel + this.AdditionalHierarchicalLevel, this.IsTableText);
							if(objHTMLelement.children.length > 0) // check if there are more html tags in the HTMLelement
								{
								// use the DissectHTMLstring method to process the paragraph.
								List<TextSegment> listTextSegments = new List<TextSegment>();
								listTextSegments = TextSegment.DissectHTMLstring(objHTMLelement.innerHTML);
								// Process the list to insert the content into Paragraph List
								foreach(TextSegment objTextSegment in listTextSegments)
									{
									if(objTextSegment.Image) // Check if it is an image
										{
										throw new InvalidImageFormatException("Rich Text is not suppose to contain an Image");
										}
									else // not an image
										{
										objRun = oxmlDocument.Construct_RunText
											(parText2Write: objTextSegment.Text,
											parContentLayer: this.ContentLayer,
											parBold: objTextSegment.Bold,
											parItalic: objTextSegment.Italic,
											parUnderline: objTextSegment.Undeline,
											parSubscript: objTextSegment.Subscript,
											parSuperscript: true);
										objParagraph.Append(objRun);
										}
									} // foreach loop end
								this.ParagraphList.Add(objParagraph);
								}
							else  // there are no cascading tags, just write the text if there are any
								{
								if(objHTMLelement.innerText.Length > 0)
									{
									if(!objHTMLelement.outerHTML.Contains("<P></P>"))
										{
										objRun = oxmlDocument.Construct_RunText(
											parText2Write: objHTMLelement.innerText,
											parContentLayer: this.ContentLayer,
											parSuperscript: true);
										objParagraph.Append(objRun);
										this.ParagraphList.Add(objParagraph);
										}
									}
								} // there are no cascading tags
							} // if(objHTMLelement.innerText != null)
						break;
					//-----------------------------------------------------
					case "SPAN":   // Underline is embedded in the Span tag
								//-----------------------------------------------------
						if(objHTMLelement.innerText != null)
							{
							Console.WriteLine("innerText.Length: {0} - [{1}]", objHTMLelement.innerText.Length, objHTMLelement.innerText);
							if(objHTMLelement.id.Contains("rangepaste"))
								{
								Console.WriteLine("Tag: SPAN - rangepaste ignored [{0}]", objHTMLelement.innerText);
								}
							else if(objHTMLelement.style.color != null)
								{
								Console.WriteLine("Tag: SPAN Style COLOR ignored [{0}]", objHTMLelement.innerText);
								}
							else if(objHTMLelement.id.Contains("underline"))
								{
								objParagraph = oxmlDocument.Construct_Paragraph(
									parBodyTextLevel: this.HierachyLevel + this.AdditionalHierarchicalLevel,
									parIsTableParagraph: this.IsTableText);
								if(objHTMLelement.children.length > 0) // check if there are more html tags in the HTMLelement
									{
									// use the DissectHTMLstring method to process the paragraph.
									List<TextSegment> listTextSegments = new List<TextSegment>();
									listTextSegments = TextSegment.DissectHTMLstring(objHTMLelement.innerHTML);
									// Process the list to insert the content into Paragraph List
									foreach(TextSegment objTextSegment in listTextSegments)
										{
										if(objTextSegment.Image) // Check if it is an image
											{
											throw new InvalidImageFormatException("Rich Text is not suppose to contain an Image");
											}
										else // not an image
											{
											objRun = oxmlDocument.Construct_RunText
												(parText2Write: objTextSegment.Text,
												parContentLayer: this.ContentLayer,
												parBold: objTextSegment.Bold,
												parItalic: objTextSegment.Italic,
												parUnderline: true,
												parSubscript: objTextSegment.Subscript,
												parSuperscript: objTextSegment.Superscript);
											objParagraph.Append(objRun);
											}
										} // foreach loop end
									this.ParagraphList.Add(objParagraph);
									}
								else  // there are no cascading tags, just write the text if there are any
									{
									if(objHTMLelement.innerText.Length > 0)
										{
										if(!objHTMLelement.outerHTML.Contains("<P></P>"))
											{
											objRun = oxmlDocument.Construct_RunText(
												parText2Write: objHTMLelement.innerText,
												parContentLayer: this.ContentLayer,
												parUnderline: true);
											objParagraph.Append(objRun);
											this.ParagraphList.Add(objParagraph);
											}
										}
									} // there are no cascading tags
								} //if(objHTMLelement.id.Contains("underline"))
							}
						break;
					//--------------------------
					case "H1":     // Heading 1
					case "H2":     // Heading 2
					case "H3":     // Heading 3
					case "H4":     // Heading 4

						//Console.WriteLine("Tag: H1\n{0}", objHTMLelement.outerHTML);
						if(this.IsTableText)
							{
							this.AdditionalHierarchicalLevel = 0;
							objParagraph = oxmlDocument.Construct_Heading(
								parHeadingLevel: this.HierachyLevel + this.AdditionalHierarchicalLevel);
							objRun = oxmlDocument.Construct_RunText(
								parText2Write: objHTMLelement.innerText,
								parContentLayer: this.ContentLayer,
								parBold: true);
							}
						else
							{
							this.AdditionalHierarchicalLevel = Convert.ToInt16(objHTMLelement.tagName.Substring(1, 1));
							objParagraph = oxmlDocument.Construct_Heading(
								parHeadingLevel: this.HierachyLevel + this.AdditionalHierarchicalLevel);
							objRun = oxmlDocument.Construct_RunText(
								parText2Write: objHTMLelement.innerText,
								parContentLayer: this.ContentLayer);
							}

						objParagraph.Append(objRun);
						this.ParagraphList.Add(objParagraph);
						break;
						} // end switch
					} // end foreach loop
				}//Try
			catch(InvalidTableFormatException exc)
				{
				Console.WriteLine("\n\nException: {0}", exc.Message);
				throw new InvalidTableFormatException(exc.Message);
				}

			catch(InvalidImageFormatException exc)
				{
				Console.WriteLine("\n\nException: {0}", exc.Message);
				throw new InvalidImageFormatException(exc.Message);
				}

			catch(Exception exc)
				{
				Console.WriteLine("\n\nException ERROR: {0} - {1} - {2} - {3}", exc.HResult, exc.Source, exc.Message, exc.Data);
				throw new GeneralException (exc.Message);
				}
			} // end of method/procedure
		}  // end of class
	} // end of Namespace
