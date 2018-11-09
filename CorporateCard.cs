using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
using Concur.Core;
using CESMidtier = Concur.Spend.CESMidtier;
using Concur.Utils;

namespace Snowbird
{
	[DataContract(Namespace = "", Name = "ActionStatus")]
	public class CctStatus : ActionStatus
	{
		[DataMember(EmitDefaultValue = false)]
		public string CctKey { get; set; }

		public CctStatus() { }

	}

	[DataContract(Namespace = "", Name = "CorporateCardTransaction")]
	public class CorporateCardTransaction
	{
		[DataMember(EmitDefaultValue = false)]
		public string CctKey;
		[DataMember(EmitDefaultValue = false)]
		public DateTime TransactionDate;
		[DataMember(EmitDefaultValue = false)]
		public string Description;
		[DataMember(EmitDefaultValue = false)]
		public string TransactionAmount;

		public decimal _dTransactionAmount;

		[DataMember(EmitDefaultValue = false)]
		public string TransactionCrnCode;
		[DataMember(EmitDefaultValue = false)]
		public string DoingBusinessAs;
		[DataMember(EmitDefaultValue = false)]
		public string CardTypeCode;
		[DataMember(EmitDefaultValue = false)]
		public string CardTypeName;

		[DataMember(EmitDefaultValue = false)]
		public string MerchantName;
		[DataMember(EmitDefaultValue = false)]
		public string MerchantCity;
		[DataMember(EmitDefaultValue = false)]
		public string MerchantState;
		[DataMember(EmitDefaultValue = false)]
		public string MerchantCtryCode;

		[DataMember(EmitDefaultValue = false)]
		public string CctType;
		[DataMember(EmitDefaultValue = false)]
		public string AuthorizationRefNo;

		[DataMember(EmitDefaultValue = false)]
		public string HasRichData;
		[DataMember(EmitDefaultValue = false)]
		public string ExpName;
		[DataMember(EmitDefaultValue = false)]
		public string ExpKey;

		[DataMember(EmitDefaultValue = false)]
		public MobileEntry MobileEntry;

		[DataMember(EmitDefaultValue = false)]
		public String SmartExpense;

		public CorporateCardTransaction() { }
		public CorporateCardTransaction(string cctKey,
			DateTime tDate, string desc, string dba, string tAmount,
			string tCrnCode, string cardTypeCode, string merchantName,
			string merchantCity, string merchantState, string merchantCtryCode,
			string expName, string expKey, string hasRichData)
		{
			this.CctKey = cctKey;
			this.TransactionDate = tDate;
			this.Description = desc;
			this.DoingBusinessAs = dba;
			this.ExpName = expName;
			this.ExpKey = expKey;
			this.TransactionCrnCode = tCrnCode;
			this.TransactionAmount = tAmount;
			this.CardTypeCode = cardTypeCode;
			this.MerchantName = string.IsNullOrEmpty(dba) ? merchantName : dba; // MOB-6364 - give dba priority over merchantName 
			this.MerchantCity = merchantCity;
			this.MerchantState = merchantState;
			this.MerchantCtryCode = merchantCtryCode;
			this.HasRichData = hasRichData;
			// Porting logic from inc_chargeData.asp
			var cardTypeName = "";
			if (!string.IsNullOrEmpty(cardTypeCode) && cardTypeCode != "OT")
			{
				cardTypeName = Concur.Localization.CQLocale.FormatText("Lookup", "CreditCard" + cardTypeCode);
				if (cardTypeName == "CreditCard" + cardTypeCode)
				{
					cardTypeName = "";
				}
			}
			this.CardTypeName = cardTypeName;

		}

		// MOB-9783 support cct to entry custom mapping
		public static Dictionary<string, string> GetCCToEntryMapping(ExpenseSessionUtils su)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			object[] meParm = new object[] { "CTE_CREDIT_CARD_TO_ENTRY_MAPPINGS", "CUSTOMIZATION" };
			string strMapping = "";
			using (var dr = DBUtils.ReturnDataReader("CE_SETTINGS_GetEntitySetting", su.getCESEntityConnection(), meParm))
			{
				if (dr.Read())
				{
					strMapping = DBUtils.GetNullableString(dr["VALUE"]);
				}
			}

			if (string.IsNullOrEmpty(strMapping))
				return result;

			string[] mappingPairs = strMapping.Split(',');
			foreach (string mappingPair in mappingPairs)
			{
				string[] vals = mappingPair.Split(':');
				if (vals != null && vals.Length == 2)
				{
					// CarRentalDays:RentalDays means cct field RentalDays maps to entry field CarRentalDays
					string ccFldName = vals[1];
					StringBuilder ccDbName = new StringBuilder();
					for (int ix = 0; ix < ccFldName.Length; ix++)
					{
						string sub = ccFldName.Substring(ix, 1);
						if (ix == 0 || sub == sub.ToLower())
						{
							ccDbName.Append(sub.ToUpper());
						}
						else
							ccDbName.Append("_" + sub);
					}
					result.Add(ccDbName.ToString(), vals[0]);
				}
			}

			return result;
		}

		public static void SetCcMappingValues(XmlNode rpeNode, Dictionary<string, string> ccMapping, DbDataReader dr)
		{
			foreach (string key in ccMapping.Keys)
			{
				try
				{
					// MOB-11027 Need to accomodate datetime field. (boolean comes as string)
					string val = null;
					Object dbval = dr[key];
					if (dbval != DBNull.Value)
					{
						switch (Type.GetTypeCode(dbval.GetType()))
						{
							case TypeCode.DateTime:  // CTE format: 2012-11-12 00:00:00.0
								val = ((DateTime)dbval).ToString("yyyy-MM-dd HH:mm:ss.0");
								break;
							default:
								val = dbval.ToString();
								break;
						}
					}

					//					string val = DBUtils.GetNullableString(dr[key]);
					ReportUtils.SetFieldValue(rpeNode, ccMapping[key], val);
				}
				catch (Exception) { }

			}
		}

		public static ActionStatus AddToReport(ExpenseSessionUtils su, AddToReportMap m, AddToReportStatus status)
		{
			string hierXml = su.getHierarchyPathXML("EXP", "EXP");
			OTProtect protect = su.getSessionIndependentProtect();

			String rptKey = protect.UnprotectNonBlankValue("RptKey", m.RptKey);

			List<string> cctKeys = null;
			if (m.CctKeys != null)
			{
				cctKeys = new List<string>(m.CctKeys);
			}

			SmartExpensesAddMap[] smartExpenses = m.SmartExpenses;

			// Do the charges
			if (!string.IsNullOrEmpty(rptKey) && ((cctKeys != null && cctKeys.Count > 0) || m.ContainsCctSmartExpense()))
			{
				object[] rptKeyInfo = Report.getReportKeyInfo(rptKey, su);
				int cPolKey = (int)rptKeyInfo[Report.IDX_RPT_KEY_INFO_POL_KEY];
				Dictionary<string, ExpenseType> expDict = ExpenseType.GetExpTypeDictByExpKey(su, cPolKey);

				// Init overall status
				status.Status = ActionStatus.SUCCESS;
				status.RptKey = protect.ProtectNonBlankValue("RptKey", rptKey);

				// Process the smart expenses
				List<String> seCctKeys = null;
				List<CctStatus> seStatus = null;
				if (smartExpenses != null && smartExpenses.Length > 0)
				{
					seCctKeys = new List<string>(smartExpenses.Length);
					seStatus = new List<CctStatus>(smartExpenses.Length);

					// Iterate each smart expense
					for (int i = 0; i < smartExpenses.Length; i++)
					{
						SmartExpensesAddMap seam = smartExpenses[i];

						if (m.matchedCctKeys.ContainsKey(seam.CctKey) || m.matchedMeKeys.ContainsKey(seam.MeKey))
						{
							// MOB-14421 Support CTE SmartExpense matching logic
							// Ignore those already smart matched.
						}
						// Make sure someone isn't sending confused data.  Ignore confused data.
						else if (seam.CctKey != null && seam.PctKey == null && seam.MeKey != null)
						{
							CctStatus seamStatus = new CctStatus();
							seamStatus.CctKey = seam.CctKey;
							seamStatus.Status = ActionStatus.SUCCESS;

							// If a CCT smarty then convert the ME to a linked ME
							MobileEntry me = MobileEntry.GetMobileEntry(su, seam.MeKey);
							if (me != null)
							{
								// Set the cctKey and save it again.  It will then be picked up below.
								me.CctKey = seam.CctKey;
								MeStatus saveStat = MobileEntry.SaveMobileEntry(su, me, "N");
								if (saveStat.Status == ActionStatus.SUCCESS)
								{
									// Add the cctKey to the list of smart expense cctKeys
									seCctKeys.Add(seam.CctKey);
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
								// Add it to our status list because the cctKey did not make it into the cctKey list
								seStatus.Add(seamStatus);
							}
						}
					}

				}

				// Setup the cct list as needed
				if (cctKeys == null && seCctKeys != null && seCctKeys.Count > 0)
				{
					cctKeys = new List<string>(seCctKeys);
				}
				else if (cctKeys != null && seCctKeys != null && seCctKeys.Count > 0)
				{
					// Add in the smart expense CCTs.
					cctKeys.AddRange(seCctKeys);
				}

				// Setup CCT statuses and add the failed SE statuses to the end if needed
				if (seStatus != null && seStatus.Count > 0)
				{
					status.CcTransactions = new CctStatus[cctKeys.Count + seStatus.Count];
					seStatus.CopyTo(status.CcTransactions, cctKeys.Count);
				}
				else
				{
					status.CcTransactions = new CctStatus[cctKeys.Count];
				}

				Dictionary<string, string> ccMapping = GetCCToEntryMapping(su);

				// Unprotect the keys and build our key string
				// Also, init the status records for each entry.
				for (int i = 0; i < cctKeys.Count; i++)
				{
					status.CcTransactions[i] = new CctStatus();
					status.CcTransactions[i].CctKey = cctKeys[i];
					if (m.matchedCctKeys.ContainsKey(cctKeys[i]))
					{
						// MOB-14421 Support CTE SmartExpense matching logic
						status.CcTransactions[i].Status = "SUCCESS_SMARTEXP";
						status.CcTransactions[i].ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", "SmartExpenseMatched");
						continue;
					}

					status.CcTransactions[i].Status = ActionStatus.FAILURE;
					status.CcTransactions[i].ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", "CorporateChargeNotAvail");


					string cctKeyString = protect.UnprotectValue("CctKey", cctKeys[i]);
					MobileEntry me = null;
					me = QuickExpenseV4Utils.GetEntryByLangCodeAndCctKey(su.getCurrentLoginName(), su.getCESLangCode(), cctKeyString, su.getProtect());
					Object[] parm = null;
					DbDataReader dr = null;

					if (!string.IsNullOrEmpty(cctKeyString))
					{
						parm = new object[] { cctKeyString, su.getCESLangCode() };
						dr = DBUtils.ReturnDataReader("CE_CREDIT_GetCardTransactionsByCctKey", su.getCESEntityConnection(), parm);
					}
					else
					{
						parm = new object[] { su.getCurrentUserEmpKey(), su.getCESLangCode(), "UN" };
						dr = DBUtils.ReturnDataReader("CE_CREDIT_GetCardTransactions", su.getCESEntityConnection(), parm);
					}

					using (dr)
					{
						while (dr.Read())
						{
							string cctStatus = DBUtils.GetNullableString(dr["TRANSACTION_STATUS"]);
							if ("UN" != cctStatus)
							{
								// MOB-11880 Skip credit cards already added to a report.
								continue;
							}

							// Find the correct status object
							CctStatus transStatus = status.CcTransactions[i];
							transStatus.Status = ActionStatus.SUCCESS;
							transStatus.ErrorMessage = null;

							bool postedSet, transactionDateSet;
							DateTime posted, transactionDate;
							DBUtils.SetNullable(out posted, out postedSet, dr["POSTED_DATE"]);
							DBUtils.SetNullable(out transactionDate, out transactionDateSet, dr["TRANSACTION_DATE"]);

							String expKey = DBUtils.GetNullableString(dr["EXP_KEY"]);
							if (me != null && !string.IsNullOrEmpty(me.ExpKey))
								expKey = me.ExpKey;
							// Default to Undefined, if neither the sproc nor the me returns anything
							// If expense types not found, create an undefined expense type
							if (expDict == null || string.IsNullOrEmpty(expKey) || !expDict.ContainsKey(expKey))
								expKey = "UNDEF";

							XmlNode rpeNode = ReportEntry.getDefaultReportEntryXML(su, rptKey, expKey);

							ReportUtils.SetFieldValue(rpeNode, "ExpKey", expKey);
							string dba = DBUtils.GetNullableString(dr["DOING_BUSINESS_AS"]);
							// MOB-6364 - give dba priority over merchantName
							ReportUtils.SetFieldValue(rpeNode, "VendorDescription", !string.IsNullOrEmpty(dba) ? dba : DBUtils.GetNullableString(dr["MERCHANT_NAME"]));
							if (me != null)
							{
								ReportUtils.SetFieldValue(rpeNode, "Comment", me.Comment);
								ReportUtils.SetFieldValue(rpeNode, "LocName", me.LocationName);
								ReportUtils.SetFieldValue(rpeNode, "MeKey", su.getProtect().UnprotectNonBlankValue("MeKey", me.MeKey));
								if (!string.IsNullOrEmpty(me.VendorName))
									ReportUtils.SetFieldValue(rpeNode, "VendorDescription", me.VendorName);

								// JIRA MOB-8191 - ensure receipts attached to card charges are passed in 'SaveExpense' call.
								// Grab the MobileEntry referenced by the PCT and determine if it has a receipt image ID set, if so
								// then set the field value of 'ReceiptImageId' on the 'rpeNode' object.
								if (!String.IsNullOrEmpty(me.ReceiptImageId))
								{
									String clearReceiptImageId = su.getProtect().UnprotectValue(ReceiptStore.RECEIPT_IMAGE_ID_PROTECT_KEY, me.ReceiptImageId);
									ReportUtils.SetFieldValue(rpeNode, "ReceiptImageId", clearReceiptImageId);
								}

							}
							// Set all fields
							ReportUtils.SetFieldValue(rpeNode, "CrnKey", dr["TRANSACTION_CRN_KEY"].ToString());
							ReportUtils.SetFieldValue(rpeNode, "TransactionAmount", ((decimal)dr["TRANSACTION_AMOUNT"]).ToString());
							ReportUtils.SetFieldValue(rpeNode, "PostedCrnKey", dr["POSTED_CRN_KEY"].ToString());
							ReportUtils.SetFieldValue(rpeNode, "PostedAmount", ((decimal)dr["POSTED_AMOUNT"]).ToString());
							ReportUtils.SetFieldValue(rpeNode, "PostedDate", posted.ToString("u"));
							ReportUtils.SetFieldValue(rpeNode, "TransactionDate", transactionDate.ToString("u"));
							//<MerchantName>DELTA AIR LINES</MerchantName>
							//<MerchantStreetAddress>ATLANTA AIRPORT</MerchantStreetAddress>
							//<MerchantCity>ATLANTA</MerchantCity>
							//<MerchantState>GA</MerchantState>
							//<MerchantCtryCode>US</MerchantCtryCode>
							//<VendorDescription>DELTA AIR LINES</VendorDescription>
							//<VenLiKey>17</VenLiKey><VenLiName>Delta Air Lines</VenLiName><TicketNumber>0061157802696</TicketNumber><TicketNum>0061157802696</TicketNum>
							ReportUtils.SetFieldValue(rpeNode, "MerchantName", DBUtils.GetNullableString(dr["MERCHANT_NAME"]));
							ReportUtils.SetFieldValue(rpeNode, "MerchantStreetAddress", DBUtils.GetNullableString(dr["MERCHANT_STREET_ADDRESS"]));
							ReportUtils.SetFieldValue(rpeNode, "MerchantCity", DBUtils.GetNullableString(dr["MERCHANT_CITY"]));
							ReportUtils.SetFieldValue(rpeNode, "MerchantState", DBUtils.GetNullableString(dr["MERCHANT_STATE"]));
							ReportUtils.SetFieldValue(rpeNode, "MerchantCtryCode", DBUtils.GetNullableString(dr["MERCHANT_CTRY_CODE"]));
							ReportUtils.SetFieldValue(rpeNode, "VenLiName", DBUtils.GetNullableString(dr["VEN_LI_NAME"]));
							ReportUtils.SetFieldValue(rpeNode, "TicketNumber", DBUtils.GetNullableString(dr["TICKET_NUMBER"]));
							ReportUtils.SetFieldValue(rpeNode, "TicketNum", DBUtils.GetNullableString(dr["TICKET_NUM"]));
							if (dr["VEN_LI_KEY"] != DBNull.Value)
								ReportUtils.SetFieldValue(rpeNode, "VenLiKey", dr["VEN_LI_KEY"].ToString());
							// MOB-15097 Set LnKey only if no matching me location
							if (dr["LN_KEY"] != DBNull.Value && (me == null || string.IsNullOrEmpty(me.LocationName)))
								ReportUtils.SetFieldValue(rpeNode, "LnKey", dr["LN_KEY"].ToString());
							ReportUtils.SetFieldValue(rpeNode, "isNew", "true");
							ReportUtils.SetFieldValue(rpeNode, "RptKey", rptKey);
							ReportUtils.SetFieldValue(rpeNode, "CctKey", ((int)dr["CCT_KEY"]).ToString());

							SetCcMappingValues(rpeNode, ccMapping, dr);
							if (m.AttendeesMap == null)
							{
								// MOB-10339 If Attendees field is not RW on entry form, do not add default attendee
								bool toAddAttendee = ItemizationDetail.containsRWAttendeesField(su, "TRAVELER", rptKey, expKey,
											(int)su.getCurrentUserEmpKey(), cPolKey, false);

								// Add default attendee
								if (toAddAttendee)
									AttendeeSearchCriteria.AddDefaultAttendeeToRpeNode(su, expKey, rpeNode, expDict);
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
								transStatus.ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", "AddCorporateChargeError");
							} // TODO - get status from res - cct already assigned error
							else
							{
								XmlNode node = res.SelectSingleNode("/Response/Body/Expense/RpeKey");

								if (node == null)
								{
									transStatus.Status = ActionStatus.FAILURE;
									transStatus.ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", "AddCorporateChargeError");
								}

								if (me != null)
								{
									AttendeeEntryMap.SetRpeKey(me.MeKey, node == null ? null : node.InnerText, m.AttendeesMap);
								}
							}
						}
					}
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

	public class CorporateCard
	{
		public static CorporateCardTransaction[] GetCardTransactionsForPortal(ExpenseSessionUtils su, string pCcaKey)
		{
			List<CorporateCardTransaction> result = new List<CorporateCardTransaction>();
			DbDataReader dr = null;
			string ccaKey = su.getProtect().UnprotectNonBlankValue("CcaKey", pCcaKey);
			Object[] parm = new object[] { su.getCurrentUserEmpKey(),
				ccaKey,
				su.getCESLangCode() };

			using (dr = DBUtils.ReturnDataReader("CE_CREDIT_GetCardTransactionsForPortal", su.getCESEntityConnection(), parm))
			{
				while (dr.Read())
				{
					string pCctKey = su.getProtect().ProtectNonBlankValue("CctKey", dr["CCT_KEY"].ToString());

					CorporateCardTransaction cct = new CorporateCardTransaction(
						pCctKey,
						(DateTime)dr["TRANSACTION_DATE"],
						DBUtils.GetNullableString(dr["DESCRIPTION"]),
						DBUtils.GetNullableString(dr["DOING_BUSINESS_AS"]),
						null, // Fill amount later out of the result set
						(string)dr["TRANSACTION_CRN_CODE"],
						DBUtils.GetNullableString(dr["CARD_TYPE_CODE"]),
						DBUtils.GetNullableString(dr["MERCHANT_NAME"]),
						DBUtils.GetNullableString(dr["MERCHANT_CITY"]),
						DBUtils.GetNullableString(dr["MERCHANT_STATE"]),
						DBUtils.GetNullableString(dr["MERCHANT_CTRY_CODE"]),
						DBUtils.GetNullableString(dr["EXP_NAME"]),
						DBUtils.GetNullableString(dr["EXP_KEY"]),
						DBUtils.GetNullableString(dr["HAS_RICH_DATA"]));
					cct._dTransactionAmount = (decimal)dr["TRANSACTION_AMOUNT"];

					// MOB-13615 Old vs New, need to remove the condition after August 13 SU release
					if (!(su.getCESVersion().StartsWith("92.0")))
					{
						cct.CctType = DBUtils.GetNullableString(dr["CCT_TYPE"]);
						cct.AuthorizationRefNo = DBUtils.GetNullableString(dr["AUTHORIZATION_REF_NO"]);
					}

					result.Add(cct);
				}
			}

			for (int ix = 0; ix < result.Count; ix++)
			{
				CorporateCardTransaction cct = result[ix];
				cct.TransactionAmount = ReportUtils.formatAmount(cct._dTransactionAmount, cct.TransactionCrnCode, su);
				// Assign the ReceiptImageId from the MobileEntry if the CCT has one.
				if (cct.MobileEntry != null)
				{
					// TODO: Pull receipt image ID from mobile entry since 'CE_CREDIT_GetCardTransactionsForPortal' doesn't
					//       currently return it!  Need to update this stored proc to also include ReceiptImageId.
					MobileEntry me = MobileEntry.GetMobileEntry(su, cct.MobileEntry.MeKey);
					if (me != null && !String.IsNullOrEmpty(me.ReceiptImageId))
					{
						cct.MobileEntry.CopyReceiptImageId(me);
				//		cct.MobileEntry.ReceiptImageId = me.ReceiptImageId;
				//		cct.MobileEntry.ReceiptImageIdUnprotected = me.ReceiptImageIdUnprotected;
					}
				}
			}
			return result.ToArray();
		}

		/**
		 * Iterate the corporate cards and match against any mobile entries.
		 **/
		public static List<string> SmartMatchExpenses(CorporateCardTransaction[] ccts, MobileEntry[] mes)
		{
			List<string> matchedMes = new List<string>();

			if (ccts != null && mes != null && ccts.Length > 0 && mes.Length > 0)
			{
				for (int i = 0; i < ccts.Length; i++)
				{
					CorporateCardTransaction cct = ccts[i];

					// We don't match on edited charges
					if (cct.MobileEntry == null)
					{
						for (int j = 0; j < mes.Length; j++)
						{
							MobileEntry me = mes[j];

							// Check that we have not already matched this mobile expense in a prior iteration
							if (!matchedMes.Contains(me.MeKey))
							{
								if (me.TransactionAmount == cct._dTransactionAmount
										&& me.CrnCode == cct.TransactionCrnCode)
								{
									long dayDiff = DateUtils.DateDiff(DateDiffInterval.Night, me.TransactionDate, cct.TransactionDate);
									if (Math.Abs(dayDiff) <= 1)
									{
										// A match.  Mark it and bail out.
										cct.SmartExpense = me.MeKey;
										matchedMes.Add(me.MeKey);

										break;
									}
								}
							}
						}
					}
				}
			}

			return matchedMes;
		}

		public static CctStatus[] SetTransactionStatus(ExpenseSessionUtils su, string[] cctKeys, string status)
		{
			CctStatus[] result = new CctStatus[cctKeys == null ? 0 : cctKeys.Length];

			OTProtect protect = su.getSessionIndependentProtect();

			for (int i = 0; i < cctKeys.Length; i++)
			{
				result[i] = new CctStatus();
				result[i].CctKey = cctKeys[i];
				result[i].Status = ActionStatus.SUCCESS;
				string cctKeyString = protect.UnprotectValue("CctKey", cctKeys[i]);
				object[] parm = new object[] { su.getCurrentUserEmpKey(), cctKeyString, status };
				try
				{
					DBUtils.ReturnCode("CE_CREDIT_SetTransactionStatus", su.getCESEntityConnection(), parm);
				}
				catch (Exception e)
				{
					result[i].ErrorMessage = "Transaction hide failed. [" + e.Message + "]";
					result[i].Status = ActionStatus.FAILURE;
				}
			}
			return result;
		}

	}

}