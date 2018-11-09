using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Xml;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Script.Serialization;
using Concur.Core;
using CESMidtier = Concur.Spend.CESMidtier;
using Concur.Utils;

namespace Snowbird
{
    [DataContract(Namespace = "", Name = "ActionStatus")]
	public class PctStatus : ActionStatus
	{
		[DataMember(EmitDefaultValue = false)]
		public string PctKey { get; set; }

		public PctStatus() { }

	}

	[DataContract(Namespace = "", Name = "PersonalCardTransaction")]
	public class PersonalCardTransaction
	{
		[DataMember(EmitDefaultValue = false)]
		public string PctKey;
		[DataMember(EmitDefaultValue = false)]
		public DateTime DatePosted;
		[DataMember(EmitDefaultValue = false)]
		public string Description;
		[DataMember(EmitDefaultValue = true)]
		public decimal Amount;
		[DataMember(EmitDefaultValue = false)]
		public string Status;
		[DataMember(EmitDefaultValue = false)]
		public string Category;
		[DataMember(EmitDefaultValue = false)]
		public string ExpKey;
		[DataMember(EmitDefaultValue = false)]
		public string ExpName;
		[DataMember(EmitDefaultValue = false)]
		public string RptKey;
		[DataMember(EmitDefaultValue = false)]
		public string RptName;
		[DataMember(EmitDefaultValue = false)]
		public MobileEntry MobileEntry;

		[DataMember(EmitDefaultValue = false)]
		public String SmartExpense;
		
		public PersonalCardTransaction(string pctKey, DateTime posted, string desc, decimal amount,
										string status, string cat, string expKey, string expName,
										string rptKey, string rptName)
		{
			PctKey = pctKey;
			DatePosted = posted;
			Description = desc;
			Amount = amount;
			Status = status;
			Category = cat;
			ExpKey = expKey;
			ExpName = expName;
			RptKey = rptKey;
			RptName = rptName;
		}

		public static ActionStatus AddToReport(ExpenseSessionUtils su, AddToReportMap m, AddToReportStatus status)
		{
			string hierXml = su.getHierarchyPathXML("EXP", "EXP");
			OTProtect protect = su.getSessionIndependentProtect();

			String rptKey = protect.UnprotectNonBlankValue("RptKey", m.RptKey);

			List<string> pctKeys = null;
			if (m.PctKeys != null)
			{
				pctKeys = new List<string>(m.PctKeys);
			}

			SmartExpensesAddMap[] smartExpenses = m.SmartExpenses;

			// Do the charges
			if (!string.IsNullOrEmpty(rptKey) && ((pctKeys != null && pctKeys.Count > 0) || m.ContainsPctSmartExpense()))
			{
				object[] rptKeyInfo = Report.getReportKeyInfo(rptKey, su);
				int cPolKey = (int)rptKeyInfo[Report.IDX_RPT_KEY_INFO_POL_KEY];
				Dictionary<string, ExpenseType> expDict = ExpenseType.GetExpTypeDictByExpKey(su, cPolKey);

				// Init overall status
				status.Status = ActionStatus.SUCCESS;
				status.RptKey = protect.ProtectNonBlankValue("RptKey", rptKey);

				// Process the smart expenses
				List<String> sePctKeys = null;
				List<PctStatus> seStatus = null;
				if (smartExpenses != null && smartExpenses.Length > 0)
				{
					sePctKeys = new List<string>(smartExpenses.Length);
					seStatus = new List<PctStatus>(smartExpenses.Length);

					// Iterate each smart expense
					for (int i = 0; i < smartExpenses.Length; i++)
					{
						SmartExpensesAddMap seam = smartExpenses[i];

						if (m.matchedPctKeys.ContainsKey(seam.PctKey) || m.matchedMeKeys.ContainsKey(seam.MeKey))
						{
							// MOB-14421 Support CTE SmartExpense matching logic
							// Ignore those already smart matched.
						}
						// Make sure someone isn't sending confused data.  Ignore confused data.
						else if (seam.PctKey != null && seam.CctKey == null && seam.MeKey != null)
						{
							PctStatus seamStatus = new PctStatus();
							seamStatus.PctKey = seam.PctKey;
							seamStatus.Status = ActionStatus.SUCCESS;

							// If a PCT smarty then convert the ME to a linked ME
							MobileEntry me = MobileEntry.GetMobileEntry(su, seam.MeKey);
							if (me != null)
							{
								// Set the pctKey and save it again.  It will then be picked up below.
								me.PctKey = seam.PctKey;
								MeStatus saveStat = MobileEntry.SaveMobileEntry(su, me, "N");
								if (saveStat.Status == ActionStatus.SUCCESS)
								{
									// Add the pctKey to the list of smart expense cctKeys
									sePctKeys.Add(seam.PctKey);
								}
								else
								{
									seamStatus.Status = saveStat.Status;
									seamStatus.ErrorMessage = saveStat.ErrorMessage;
								}
							}
							else
							{
								seamStatus.Status = ActionStatus.FAILURE;
								seamStatus.ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", "SmartExpenseNoMobileEntry");
							}

							if (seamStatus.Status != ActionStatus.SUCCESS)
							{
								// Add it to our status list because the pctKey did not make it into the pctKey list
								seStatus.Add(seamStatus);
							}
						}
					}

				}

				// Setup the pct list as needed
				if (pctKeys == null && sePctKeys != null && sePctKeys.Count > 0)
				{
					pctKeys = new List<string>(sePctKeys);
				}
				else if (pctKeys != null && sePctKeys != null && sePctKeys.Count > 0)
				{
					// Add in the smart expense PCTs.
					pctKeys.AddRange(sePctKeys);
				}

				// Setup PCT statuses and add the failed SE statuses to the end if needed
				if (seStatus != null && seStatus.Count > 0)
				{
					status.Transactions = new PctStatus[pctKeys.Count + seStatus.Count];
					seStatus.CopyTo(status.Transactions, pctKeys.Count);
				}
				else
				{
					status.Transactions = new PctStatus[pctKeys.Count];
				}

				// Unprotect the keys and build our key string
				// Also, init the status records for each entry.
				string pctKeyString = "";

				for (int i = 0; i < pctKeys.Count; i++)
				{
					status.Transactions[i] = new PctStatus();
					status.Transactions[i].PctKey = pctKeys[i];
					if (m.matchedPctKeys.ContainsKey(pctKeys[i]))
					{
						// MOB-14421 Support CTE SmartExpense matching logic
						status.Transactions[i].Status = "SUCCESS_SMARTEXP";
						status.Transactions[i].ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", "SmartExpenseMatched");
					}
					else
					{
						status.Transactions[i].Status = ActionStatus.FAILURE;
						status.Transactions[i].ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", "PersonalChargeNotAvail");

						if (pctKeyString.Length > 0)
						{
							pctKeyString += ",";
						}

						pctKeyString += protect.UnprotectValue("PctKey", pctKeys[i]);
					}
				}

				DbDataReader sqlDr = null;
				try
				{
					Object[] parm = new object[] { su.getCurrentUserEmpKey(), su.GetDefaultPolicyKey(), pctKeyString };
					sqlDr = DBUtils.ReturnDataReader("CE_Yodlee_GetTransactionsForImport", su.getCESEntityConnection(), parm);
				}
				finally
				{
					if (sqlDr != null) sqlDr.Close();
				}

				DbDataReader dr = null;

				try
				{
					// Pull back all the data
					Object[] parm = new object[] { su.getCurrentUserEmpKey(), su.GetDefaultPolicyKey(), pctKeyString };
					dr = DBUtils.ReturnDataReader("CE_Yodlee_GetTransactionsForImport", su.getCESEntityConnection(), parm);

					while (dr.Read())
					{
						// Find the correct status object
						string protectedPctKey = protect.ProtectValue("PctKey", ((int)dr["PCT_KEY"]).ToString());
						PctStatus transStatus = status.getStatusForTransaction(protectedPctKey);
						transStatus.Status = ActionStatus.SUCCESS;
						transStatus.ErrorMessage = null;

						bool postedSet;
						DateTime posted;
						DBUtils.SetNullable(out posted, out postedSet, dr["DATE_POSTED"]);

						string expKey = DBUtils.GetNullableString(dr["EXP_KEY"]);

						// If expense types not found, create an undefined expense type
						if (expDict == null || string.IsNullOrEmpty(expKey) || !expDict.ContainsKey(expKey)) 
							expKey = "UNDEF"; 

						XmlNode rpeNode = ReportEntry.getDefaultReportEntryXML(su, rptKey, expKey);
						// Set all fields
						ReportUtils.SetFieldValue(rpeNode, "VendorDescription", DBUtils.GetNullableString(dr["DESCRIPTION"]));
						ReportUtils.SetFieldValue(rpeNode, "ExpKey", expKey);
						ReportUtils.SetFieldValue(rpeNode, "CrnCode", DBUtils.GetNullableString(dr["CRN_CODE"]));
						ReportUtils.RemoveField(rpeNode, "CrnKey");
						ReportUtils.SetFieldValue(rpeNode, "TransactionAmount", ((decimal)dr["TRANSACTION_AMOUNT"]).ToString());
						ReportUtils.SetFieldValue(rpeNode, "TransactionDate", posted.ToString("u"));
						ReportUtils.SetFieldValue(rpeNode, "PersonalCard", "Y");
						ReportUtils.SetFieldValue(rpeNode, "isNew", "true");
						ReportUtils.SetFieldValue(rpeNode, "RptKey", rptKey);
						ReportUtils.SetFieldValue(rpeNode, "PctKey", ((int)dr["PCT_KEY"]).ToString());
						//MOB-4513/CRMC-22487
						if (dr["PAT_KEY"] != DBNull.Value)
						{
							ReportUtils.SetFieldValue(rpeNode, "PatKey", dr["PAT_KEY"].ToString());
						}

						// MOB-13747 Copy down business purpose from report
						if (!string.IsNullOrEmpty(m.businessPurpose))
						{
							ReportUtils.SetFieldValue(rpeNode, "Description", m.businessPurpose);
						}

						string actionXML = "<SaveExpense>" +
							"<EditedForEmpKey>" + su.getCurrentUserEmpKey() + "</EditedForEmpKey>" +
							"<EditedByEmpKey>" + su.getCurrentUserEmpKey() + "</EditedByEmpKey>" +
							hierXml +
							"<Role>TRAVELER</Role>" +
							"<LangCode>" + su.getCESLangCode() + "</LangCode>";
							// MOB-11292 Use IsMobile on request header instead	
							// "<FromMobile>true</FromMobile>";
						actionXML += "<Expense>" + rpeNode.InnerXml + "</Expense>";
						actionXML += "</SaveExpense>";

						XmlDocument xmlRequest = new XmlDocument();
						xmlRequest.LoadXml(actionXML.ToString());

						xmlRequest = MidTierUtil.WrapMidtierRequest(xmlRequest);
						XmlDocument res = CESMidtier.MakeMidtierRequest("SaveExpense", xmlRequest, 60);
						MidtierException ex = su.processMidtierError(xmlRequest, res, false);
						if (ex != null)
						{
							transStatus.Status = ActionStatus.FAILURE;
							transStatus.ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", "AddPersonalChargeError");
						}
					}
				}
				finally
				{
					if (dr != null) dr.Close();
				}

				//ReportDetail result = new ReportDetail();
				//result.PopulateData(su, "TRAVELER", rptKey, WebServiceVersion.V1);
				//status.Report = result;
			}
			else
			{
				status.Status = ActionStatus.FAILURE;
				status.ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", "CannotCreateReport");
			}

			return status;
		}
	}

	[DataContract(Namespace = "", Name="ActionStatus")]
	public class YodleeCardStatus : ActionStatus
	{
		[DataMember(EmitDefaultValue = false)]
		public PersonalCard PersonalCard;
	}

	[DataContract(Namespace = "")]
	public class YodleeCardLoginInfo
	{
		[DataMember(EmitDefaultValue = false)]
		public string ContentServiceId;
		[DataMember(EmitDefaultValue = false)]
		public FormField[] Fields;

		public YodleeCardStatus AddCard(ExpenseSessionUtils su)
		{
			YodleeCardStatus result = new YodleeCardStatus();
			/*			string actionXML = "<AddItemToYodlee><ContentServiceId>" + ContentServiceId+"</ContentServiceId>" +
							"<EmpKey>" + su.getCurrentUserEmpKey() + "</EmpKey>" +
							"<LangCode>" + su.getCESLangCode() + "</LangCode>" +
							"</AddItemToYodlee>";

						XmlDocument xmlRequest = new XmlDocument();
						xmlRequest.LoadXml(actionXML.ToString());
			//                <LOGIN>concurtest.creditCard1</LOGIN>
			//                <PASSWORD>creditCard1</PASSWORD>

						//<AddItemToYodlee><Field><ValueIdentifier>LOGIN</ValueIdentifier><Value>ZqOYUfcQ5528bGdKcwPyA/hduk1y+rnx</Value></Field>
						//<Field><ValueIdentifier>PASSWORD</ValueIdentifier><Value>ZnobkhyN81lSXzzwTodZZA==</Value></Field>
						//<ContentServiceId>15921</ContentServiceId><EmpKey>272</EmpKey><LangCode>en</LangCode></AddItemToYodlee>
						XmlNode addItemNode = xmlRequest.SelectSingleNode("/AddItemToYodlee");
						if (addItemNode != null)
						{
							foreach (FormField fld in this.Fields)
							{
								if (!string.IsNullOrEmpty(fld.Id))
								{
									XmlNode fldNode = addItemNode.OwnerDocument.CreateElement("Field");
									fldNode = addItemNode.AppendChild(fldNode);

									ReportUtils.SetFieldValue(fldNode, "ValueIdentifier", fld.Id);
									ReportUtils.SetFieldValue(fldNode, "Value", fld.getFieldValue());
								}
							}
						}
						xmlRequest = MidTierUtil.WrapMidtierRequest(xmlRequest);
						XmlDocument res = CESMidtier.MakeMidtierRequest("AddItemToYodlee", xmlRequest, 60);
						MidtierException ex = su.processMidtierError(xmlRequest, res, false);

						if (ex != null)
						{
							result.Status = ActionStatus.FAILURE;
							result.ErrorMessage = ex.Message;
						}
						else
						{
							XmlNode xStatus = res.SelectSingleNode("/Response/Body/AddItemToYodlee/Status");
							if (xStatus != null && xStatus.InnerText == "SUCCESS!")
							{
								result.Status = ActionStatus.SUCCESS;
								XmlNode cardNode = res.SelectSingleNode("/Response/Body/AddItemToYodlee/PersonalCardsAdded/PersonalCardAdded");
								result.pca = PersonalCard.getCardFromXml(su, cardNode);
							}
							else
							{
								result.Status = ActionStatus.FAILURE;
								result.ErrorMessage = res.InnerText;
								if (xStatus != null)
								{
									result.ErrorMessage = xStatus.InnerText;
								}
							}
						}

						*/

			XmlDocument xmlRequest = new XmlDocument();
			xmlRequest.LoadXml("<AddItemToYodlee/>");

			//<Field><ValueIdentifier>LOGIN</ValueIdentifier><Value>ZqOYUfcQ5528bGdKcwPyA/hduk1y+rnx</Value></Field>
			//<Field><ValueIdentifier>PASSWORD</ValueIdentifier><Value>ZnobkhyN81lSXzzwTodZZA==</Value></Field>
			XmlNode addItemNode = xmlRequest.SelectSingleNode("/AddItemToYodlee");
			foreach (FormField fld in this.Fields)
			{
				if (!string.IsNullOrEmpty(fld.Id) && addItemNode != null)
				{
					XmlNode fldNode = addItemNode.OwnerDocument.CreateElement("Field");
					fldNode = addItemNode.AppendChild(fldNode);

					ReportUtils.SetFieldValue(fldNode, "ValueIdentifier", fld.Id);
					ReportUtils.SetFieldValue(fldNode, "Value", fld.getFieldValue());
				}
			}

			string url = HttpUtils.GetWebServerBaseUrl() + "/expense/proxy/YodleeData.asp";

			string postdata;

/*			csId	15921
data	AddItemToYodlee
formPasswInfo	<Field><ValueIdentifier>LOGIN</ValueIdentifier><Value>concurtest.creditCard3</Value></Field><Field><ValueIdentifier>PASSWORD</ValueIdentifier><Value>creditCard3</Value></Field>
itemId	
role*/
			postdata = "data=AddItemToYodlee&role=TRAVELER" +
				"&csId=" + HttpUtility.UrlEncode(ContentServiceId) +
				"&formPasswInfo=" + HttpUtility.UrlEncode(addItemNode.InnerXml) +
				"&itemId=";

			string xmlHttp;
			xmlHttp = ReportDetail.MakeHTTPRequest(su.getSessionID(), "POST", url, postdata, "application/x-www-form-urlencoded", null, 200);
			MidTierUtil.logExpenseRouterResult("AddItemToYodleeViaYodleeData", postdata, url + "\n" + xmlHttp);

			result.Status = ActionStatus.FAILURE;
			// fake data
			//xmlHttp = "{Body:{AddItemToYodlee:{Status:\"SUCCESS\",PersonalCardsAdded:{PersonalCardAdded:{PcaKey:3,ItemId:11081204,CardName:\"TestCreditcard Special Case - Super CD Plus\",AccountNumber:\"XXXX XXXX XXXX 2345\",CrnCode:\"USD\",UnUsedTotalAmt:0.00000000} }} } }";
			try
			{
				JavaScriptSerializer js = new JavaScriptSerializer();
				Object obj = js.DeserializeObject(xmlHttp);

				Object body = JSONUtils.getChildJSONObject(obj, "Body");
				Object addItem = JSONUtils.getChildJSONObject(body, "AddItemToYodlee");
				Object status = JSONUtils.getChildJSONObject(addItem, "Status");
				if (((string)status) == "SUCCESS")
				{
					result.Status = ActionStatus.SUCCESS;
					result.ErrorMessage = null;

					Object personalCards = JSONUtils.getChildJSONObject(addItem, "PersonalCardsAdded");
					Object personalCard = JSONUtils.getChildJSONObject(personalCards, "PersonalCardAdded");

					Object pcaKey = JSONUtils.getChildJSONObject(personalCard, "PcaKey");
					Object cardName = JSONUtils.getChildJSONObject(personalCard, "CardName");
					Object accountNumber = JSONUtils.getChildJSONObject(personalCard, "AccountNumber");
					Object crnCode = JSONUtils.getChildJSONObject(personalCard, "CrnCode");
					if (personalCard != null && pcaKey != null)
					{
						string strAcctNumber = (string)accountNumber;
						string acctNumLastFour = (String.IsNullOrEmpty(strAcctNumber) ? null : strAcctNumber.Substring(strAcctNumber.Length - 4));
						string protectedPcaKey = su.getProtect().ProtectNonBlankValue("PcaKey", pcaKey.ToString());
						result.PersonalCard = new PersonalCard(protectedPcaKey, (string)cardName, acctNumLastFour, (string)crnCode);

					}
					/*
					 * <PcaKey>3</PcaKey><ItemId>11081204</ItemId><CardName>TestCreditcard Special Case - Super CD Plus</CardName><AccountNumber>XXXX XXXX XXXX 2345</AccountNumber><CrnCode>USD</CrnCode><UnUsedTotalAmt>0.00000000</UnUsedTotalAmt><RefreshDate>2011-11-22</RefreshDate><RefreshStatus>REFRESH_COMPLETED_SUCCESSFULLY</RefreshStatus><RefreshMessage>Refresh of this Item is complete</RefreshMessage>
					 */
				}
				else if (status != null)
				{
					result.Status = (string)status;
					result.ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", (string)status);
				}
			}
			catch 
			{
				result.PersonalCard = null;
			}
			return result;
		}
	}

	[DataContract(Namespace = "")]
	public class YodleeCardProvider
	{
		[DataMember(EmitDefaultValue = false)]
		public string ContentServiceId;
		[DataMember(EmitDefaultValue = false)]
		public string Name;

		public static YodleeCardProvider[] GetPopularCards(ExpenseSessionUtils su)
		{
			List<YodleeCardProvider> result = new List<YodleeCardProvider>();

			using (var dr = DBUtils.ReturnDataReader("CE_Yodlee_GetMostPopularFinancialInstitutions", su.getCESEntityConnection(), null))
			{
				while (dr.Read())
				{
					YodleeCardProvider card = new YodleeCardProvider();
					card.ContentServiceId = dr["CONTENT_SERVICE_ID"].ToString();
					card.Name = DBUtils.GetNullableString(dr["NAME"]);
					result.Add(card);
				}
			}

			return result.ToArray();
		}

		public static YodleeCardProvider[] SearchCards(ExpenseSessionUtils su, string query)
		{
			List<YodleeCardProvider> result = new List<YodleeCardProvider>();
			string actionXML = "<GetFilteredCreditContentServices>" +
				"<SearchCriteria>" + "</SearchCriteria>" +
				"</GetFilteredCreditContentServices>";

			XmlDocument xmlRequest = new XmlDocument();
			xmlRequest.LoadXml(actionXML.ToString());

			XmlNode searchNode = xmlRequest.SelectSingleNode("/GetFilteredCreditContentServices/SearchCriteria");
			searchNode.InnerText = query;

			xmlRequest = MidTierUtil.WrapMidtierRequest(xmlRequest);
			XmlDocument res = CESMidtier.MakeMidtierRequest("GetFilteredCreditContentServices", xmlRequest, 60);
			su.processMidtierError(xmlRequest, res, true);

			XmlNodeList cardList = res.SelectNodes("/Response/Body/ContentServices/ContentService");
			IEnumerator ienum = cardList.GetEnumerator();
			while (ienum.MoveNext())
			{
				YodleeCardProvider card = new YodleeCardProvider();
				XmlNode cardNode = (XmlNode)ienum.Current;
				card.Name = ReportUtils.getTextFromNode(cardNode, "Name");
				card.ContentServiceId = ReportUtils.getTextFromNode(cardNode, "ContentServiceId");
				result.Add(card);
			}
			return result.ToArray();
		}

		// If we have multi-fixed field, mark them as "<Label> 1".."<Label> N".
		public static FormField[] GetLoginForm(ExpenseSessionUtils su, string contentServiceId)
		{
			List<FormField> result = new List<FormField>();
			string actionXML = "<GetLoginFormForContentService>" +
				"<ContentServiceId>" + contentServiceId + "</ContentServiceId>" +
				"</GetLoginFormForContentService>";

			XmlDocument xmlRequest = new XmlDocument();
			xmlRequest.LoadXml(actionXML.ToString());

			xmlRequest = MidTierUtil.WrapMidtierRequest(xmlRequest);
			XmlDocument res = CESMidtier.MakeMidtierRequest("GetLoginFormForContentService", xmlRequest, 60);
			su.processMidtierError(xmlRequest, res, true);

			/*		        <FormFieldInfo>
								<NeedsBigOr>false</NeedsBigOr>
								<NeedsLittleOr>false</NeedsLittleOr>
								<DisplayName>User ID</DisplayName>
								<InputFields>
								<InputField>
								<FieldType>TEXT</FieldType>
								<ValueIdentifier>LOGIN</ValueIdentifier>
								</InputField>
								</InputFields>
								<IsMultiFixed>false</IsMultiFixed>
							</FormFieldInfo>
			 */

			XmlNodeList fldList = res.SelectNodes("/Response/Body/FormFields/FormFieldInfo");
			IEnumerator ienum = fldList.GetEnumerator();
			while (ienum.MoveNext())
			{
				XmlNode fldNode = (XmlNode)ienum.Current;
				XmlNodeList inputList = fldNode.SelectNodes("InputFields/InputField");
				int iCount = inputList.Count;
				int ix = 0;
				IEnumerator ienum2 = inputList.GetEnumerator();
				while (ienum2.MoveNext())
				{
					ix++;
					FormField fld = new FormField();
					XmlNode inNode = (XmlNode)ienum2.Current;
					fld.Label = ReportUtils.getTextFromNode(fldNode, "DisplayName");
					if (iCount > 1)
						fld.Label += " " + ix;
					fld.Id = ReportUtils.getTextFromNode(inNode, "ValueIdentifier");
					fld.DataType = ReportUtils.getTextFromNode(inNode, "FieldType");
					fld.CtrlType = "edit";
					result.Add(fld);
				}
			}
			return result.ToArray();
		}
	}

	[DataContract(Namespace = "", Name = "PersonalCard")]
	public class PersonalCard
	{
		[DataMember(EmitDefaultValue = false)]
		public string PcaKey;
		[DataMember(EmitDefaultValue = false)]
		public string CardName;
		[DataMember(EmitDefaultValue = false)]
		public string AccountNumberLastFour;
		[DataMember(EmitDefaultValue = false)]
		public string CrnCode;

		[DataMember(EmitDefaultValue = false)]
		public PersonalCardTransaction[] Transactions;

		public PersonalCard(string key, string name, string acctNumLastFour, string crnCode)
		{
			PcaKey = key;
			CardName = name;
			AccountNumberLastFour = acctNumLastFour;
			CrnCode = crnCode;
		}

		public static PersonalCard getCardFromXml(ExpenseSessionUtils su, XmlNode personalCardNode)
		{
			XmlNode pcaKeyNode = personalCardNode.SelectSingleNode("PcaKey");
			XmlNode nameNode = personalCardNode.SelectSingleNode("CardName");
			XmlNode acctNumNode = personalCardNode.SelectSingleNode("AccountNumber");
			XmlNode crnCodeNode = personalCardNode.SelectSingleNode("CrnCode");

			string pcaKeyClear = pcaKeyNode.InnerText;
			string pcaKey = su.getProtect().ProtectValue("PcaKey", pcaKeyClear);

			string acctNum = acctNumNode.InnerText;
			// MOB-14476 Prevent internal server error when user enter place holder value for account number, e.g "8".
			string acctNumLastFour = getAccountNumberLastFour(acctNum);
			//string acctNumLastFour = (String.IsNullOrEmpty(acctNum) ? null : acctNum.Substring(acctNum.Length - 4));

			PersonalCard p = new PersonalCard(pcaKey, nameNode.InnerText, acctNumLastFour, crnCodeNode.InnerText);
			return p;
		}

		public static PersonalCard[] GetCards(ExpenseSessionUtils su)
		{
			return GetCards(su, false);
		}

		static string getAccountNumberLastFour(string acctNum)
		{
			// MOB-14476 Prevent internal server error when user enter place holder value for account number, e.g "8".
			return (String.IsNullOrEmpty(acctNum) ? null : acctNum.Substring(acctNum.Length > 4 ? acctNum.Length - 4 : 0));
		}

		public static PersonalCard[] GetCards(ExpenseSessionUtils su, bool withTransactions)
		{
			string actionXML = "<GetEmployeeYodleeCardsSummary>" +
				"<LangCode>" + su.getCESLangCode() + "</LangCode>" +
				"<EmpKey>" + su.getCurrentUserEmpKey() + "</EmpKey>" +
				"</GetEmployeeYodleeCardsSummary>";

			XmlDocument xmlRequest = new XmlDocument();
			xmlRequest.LoadXml(actionXML.ToString());

			xmlRequest = MidTierUtil.WrapMidtierRequest(xmlRequest);
			XmlDocument res = CESMidtier.MakeMidtierRequest("GetEmployeeYodleeCardsSummary", xmlRequest, 60);
			su.processMidtierError(xmlRequest, res, true);

			XmlNodeList cardList = res.SelectNodes("/Response/Body/YodleeCardsSummary/YodleeCardSummary");

			ArrayList tmp = new System.Collections.ArrayList();

			OTProtect protect = su.getSessionIndependentProtect();

			IEnumerator ienum = cardList.GetEnumerator();
			while (ienum.MoveNext())
			{
				XmlNode personalCardNode = (XmlNode)ienum.Current;
				XmlNode pcaKeyNode = personalCardNode.SelectSingleNode("PcaKey");
				XmlNode nameNode = personalCardNode.SelectSingleNode("CardName");
				XmlNode acctNumNode = personalCardNode.SelectSingleNode("AccountNumber");
				XmlNode crnCodeNode = personalCardNode.SelectSingleNode("CrnCode");

				string pcaKeyClear = pcaKeyNode.InnerText;
				string pcaKey = protect.ProtectValue("PcaKey", pcaKeyClear);

				// MOB-8812/CRMC-31232 Handle cc sync failure - acctNumNode is null 
				string acctNum = acctNumNode==null? null : acctNumNode.InnerText;
				if (!String.IsNullOrEmpty(acctNum))
				{
					// MOB-14476 Prevent internal server error when user enter place holder value for account number, e.g "8".
					string acctNumLastFour = getAccountNumberLastFour(acctNum);
//					string acctNumLastFour = (String.IsNullOrEmpty(acctNum) ? null : acctNum.Substring(acctNum.Length - 4));

					PersonalCard p = new PersonalCard(pcaKey, nameNode.InnerText, acctNumLastFour, crnCodeNode.InnerText);

					if (withTransactions)
					{
						p.Transactions = PersonalCard.GetTransactions(su, pcaKeyClear);
					}

					tmp.Add(p);
				}
			}

			return (PersonalCard[])tmp.ToArray(typeof(PersonalCard));
		}

		public static PersonalCardTransaction[] GetTransactions(ExpenseSessionUtils su, string pcaKey)
		{
			OTProtect protect = su.getSessionIndependentProtect();

			DbDataReader dr = null;
			ArrayList tmp = new System.Collections.ArrayList();

			try
			{
				Object[] parm = new object[] { su.getCurrentUserEmpKey(), su.GetDefaultPolicyKey(), su.getCESLangCode(), pcaKey, 0, 0, null, null };
				dr = DBUtils.ReturnDataReader("CE_Yodlee_GetCardTransactionsWithExpenseType", su.getCESEntityConnection(), parm);

				while (dr.Read())
				{
					bool postedSet;
					DateTime posted;
					DBUtils.SetNullable(out posted, out postedSet, dr["DATE_POSTED"]);

					string rptKeyProt = (dr["RPT_KEY"] == DBNull.Value) ? null : protect.ProtectValue("RptKey", ((int)dr["RPT_KEY"]).ToString());

					PersonalCardTransaction t = new PersonalCardTransaction(
						protect.ProtectValue("PctKey", ((int)dr["PCT_KEY"]).ToString()),
						posted,
						DBUtils.GetNullableString(dr["DESCRIPTION"]),
						(decimal)dr["TRANSACTION_AMOUNT"],
						DBUtils.GetNullableString(dr["STATUS"]),
						DBUtils.GetNullableString(dr["CARD_CATEGORY_NAME"]),
						DBUtils.GetNullableString(dr["EXP_KEY"]),
						DBUtils.GetNullableString(dr["EXP_NAME"]),
						rptKeyProt,
						DBUtils.GetNullableString(dr["REPORT_NAME"])
					);
					tmp.Add(t);
				}

			}
			finally
			{
				if (dr != null) dr.Close();
			}

			PersonalCardTransaction[] pcts = (PersonalCardTransaction[]) tmp.ToArray(typeof(PersonalCardTransaction));
			for (int ix = 0; ix < pcts.Length; ix++)
			{
				PersonalCardTransaction pct = pcts[ix];
				if (pct.MobileEntry != null)
				{
					// TODO: Pull receipt image ID from mobile entry since 'CE_Yodlee_GetCardTransactionsWithExpenseType' doesn't
					//       currently return it!  Need to update this stored proc to also include ReceiptImageId.
					MobileEntry me = MobileEntry.GetMobileEntry(su, pct.MobileEntry.MeKey);
					if (me != null && !String.IsNullOrEmpty(me.ReceiptImageId))
					{
						//		pct.MobileEntry.ReceiptImageId = me.ReceiptImageId;
						//	pct.MobileEntry.ReceiptImageIdUnprotected = me.ReceiptImageIdUnprotected;
						pct.MobileEntry.CopyReceiptImageId(me);
					}
				}
			}

			return pcts;

//			return (PersonalCardTransaction[])tmp.ToArray(typeof(PersonalCardTransaction));
		}

		/**
		 * Iterate the personal cards and match against any mobile entries.
		 **/
		public static List<string> SmartMatchExpenses(PersonalCard[] cards, MobileEntry[] mes, List<String> usedMes)
		{
			List<string> matchedMes = new List<string>();

			if (cards != null && mes != null && cards.Length > 0 && mes.Length > 0)
			{
				for (int pcIdx = 0; pcIdx < cards.Length; pcIdx++)
				{
					PersonalCard card = cards[pcIdx];

					if (card.Transactions != null && card.Transactions.Length > 0) 
					{
						PersonalCardTransaction[] trans = card.Transactions;

						for (int pctIdx = 0; pctIdx < trans.Length; pctIdx++)
						{
							PersonalCardTransaction pct = trans[pctIdx];

							// We don't match on edited charges
							if (pct.MobileEntry == null)
							{
								for (int meIdx = 0; meIdx < mes.Length; meIdx++)
								{
									MobileEntry me = mes[meIdx];

									// Check the used list (from corp cards).  We can't match multiples here.
									// And check that we have not already matched this mobile expense in a prior iteration
									if (!usedMes.Contains(me.MeKey) && !matchedMes.Contains(me.MeKey))
									{
										if (me.TransactionAmount == pct.Amount
												&& me.CrnCode == card.CrnCode)
										{
											long dayDiff = DateUtils.DateDiff(DateDiffInterval.Night, me.TransactionDate, pct.DatePosted);
											if (Math.Abs(dayDiff) <= 1)
											{
												// A match.  Mark it and bail out.
												pct.SmartExpense = me.MeKey;
												matchedMes.Add(me.MeKey);

												break;
											}
										}
									}
								}
							}
						}
					}
				}
			}

			return matchedMes;
		}

		public static void SetTransactionStatus(ExpenseSessionUtils su, string[] pctKeys, string status)
		{
			OTProtect protect = su.getSessionIndependentProtect();

			string pctKeyString = "";

			for (int i = 0; i < pctKeys.Length; i++)
			{
				if (pctKeyString.Length > 0)
				{
					pctKeyString += ",";
				}

				pctKeyString += protect.UnprotectValue("PctKey", pctKeys[i]);
			}

			object[] parm = new object[] { su.getCurrentUserEmpKey(), pctKeyString, status };
			DBUtils.ReturnCode("CE_Yodlee_UpdateTransactionStatus", su.getCESEntityConnection(), parm);

		}
	}

}

