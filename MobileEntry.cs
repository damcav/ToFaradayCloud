using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Xml;
using Concur.Core;
using CESMidtier = Concur.Spend.CESMidtier;
using Concur.Localization;
using System.Web.Script.Serialization;
using System.Text;

namespace Snowbird
{
	// OCR receipt objects
	[DataContract(Namespace = "", Name = "ActionStatus")]
	public class RCStatus : ActionStatus // RC - ReceiptCapture
	{
		[DataMember(EmitDefaultValue = false)]
		public string SmartExpenseId { get; set; }
	}

	[DataContract(Namespace = "", Name = "ReceiptCapture")]
	public class ReceiptCapture // SmartExpenseItem
	{
		[DataMember(EmitDefaultValue = false)]
		public string SmartExpenseId { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string RcKey { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string ExpKey { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string ExpName { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string VendorName { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string Comment { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public decimal TransactionAmount { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string CrnCode { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string CrnKey { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public DateTime TransactionDate { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string ReceiptImageId { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string ReceiptImageIdUnprotected { get; set; }
		public string StatKey { get; set; }

		public string clearRcKey;

		public ReceiptCapture(DbDataReader Reader, OTProtect prot)
		{
			clearRcKey = ((int)Reader["RC_KEY"]).ToString();
			RcKey = AddToReportMap.getProtSmartExpenseIdFromRcKey(clearRcKey, prot);
			SmartExpenseId = RcKey;

			ExpKey = DBUtils.GetNullableString(Reader["EXP_KEY"]);
			ExpName = DBUtils.GetNullableString(Reader["EXP_NAME"]);
			Comment = DBUtils.GetNullableString(Reader["COMMENT"]);
			VendorName = DBUtils.GetNullableString(Reader["VENDOR_DESCRIPTION"]);
			CrnCode = DBUtils.GetNullableString(Reader["CRN_CODE"]);
			if (Reader["TRANSACTION_AMOUNT"] != DBNull.Value)
			{
				TransactionAmount = (decimal)Reader["TRANSACTION_AMOUNT"];
			}
			if (Reader["TRANSACTION_DATE"] != DBNull.Value)
			{
				TransactionDate = (DateTime)Reader["TRANSACTION_DATE"];
			}
			if (Reader["STAT_KEY"] != DBNull.Value)
			{
				StatKey = DBUtils.GetNullableString(Reader["STAT_KEY"]);
			}
			ReceiptImageIdUnprotected = Reader["RECEIPT_IMAGE_ID"].ToString();
			ReceiptImageId = prot.ProtectValue(ReceiptStore.RECEIPT_IMAGE_ID_PROTECT_KEY, ReceiptImageIdUnprotected);
		}


		/**
		* <summary>
		* Will retrieve a list of unassigned ReceiptCapture items with a status of 'M_DONE' or 'A_DONE'.
		* </summary>
		* <param name="su">an expense session utils object.</param>
		* <returns>
		*      an array of ReceiptCapture items.
		* </returns>
		*/
		public static ReceiptCapture[] GetReceiptCaptureList(ExpenseSessionUtils su)
		{
			return GetReceiptCaptureList(su, 'N');
		}

		/**
		* <summary>
		* Will retrieve a list of unassigned ReceiptCapture items.
		* </summary>
		* <param name="su">an expense session utils object.</param>
		* <param name="includeNotDone">contains either 'Y' or 'N' indicating whether "not done" items should be included.</param>
		* <returns>
		*      an array of ReceiptCapture items.
		* </returns>
		*/
		public static ReceiptCapture[] GetReceiptCaptureList(ExpenseSessionUtils su, char includeNotDone)
		{
			object[] parm = new object[] { su.getCurrentUserEmpKey(), su.getCESLangCode(), includeNotDone }; // Include only M_DONE status
			List<ReceiptCapture> tmp = new List<ReceiptCapture>();
			using (var dr = DBUtils.ReturnDataReader("CE_RECEIPTCAPTURE_GetUnassignedOcrReceipts", su.getCESEntityConnection(), parm))
			{
				while (dr.Read())
				{
					tmp.Add(new ReceiptCapture(dr, su.getProtect()));
				}
			}
			return (ReceiptCapture[])tmp.ToArray();
		}

		public static int GetReceiptCaptureCount(ExpenseSessionUtils su, char includeNotDone)
		{
			object[] parm = new object[] { su.getCurrentUserEmpKey(), su.getCESLangCode(), includeNotDone }; // Include only M_DONE status
			List<ReceiptCapture> tmp = new List<ReceiptCapture>();
			var dr = DBUtils.ReturnDynamic("CE_RECEIPTCAPTURE_GetUnassignedOcrReceipts", su.getCESEntityConnection(), parm);
			return dr.Count;
		}


	}

	[DataContract(Namespace = "", Name = "ActionStatus")]
	public class MeStatus : ActionStatus
	{
		[DataMember(EmitDefaultValue = false)]
		public string MeKey { get; set; }

		public MeStatus() { }

	}
	[DataContract(Namespace = "", Name = "MobileEntryFormField")]
	public class MobileEntryFormField
	{
		[DataMember(EmitDefaultValue = false)]
		public string Access { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string CtrlType { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string DataType { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string FieldValue { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string FtCode { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string HierKey { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string HierLevel { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string ID { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string Label { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string LiKey { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string MobileEntryId { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string Ordinal { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string ParFieldId { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string ParFtcode { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string ParHierLevel { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string ParLiKey { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string TaxAuthKey { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string TaxFormKey { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string Section { get; set; }

	}


	[DataContract(Namespace = "", Name = "MobileEntry")]
	public class MobileEntry
	{
		public void SetReceiptImageId(OTProtect prot, string idUnprotected)
		{
			ReceiptImageIdUnprotected = idUnprotected;
			ReceiptImageId = prot.ProtectValue(ReceiptStore.RECEIPT_IMAGE_ID_PROTECT_KEY, idUnprotected);
		}

		public void CopyReceiptImageId(MobileEntry other)
		{
			ReceiptImageId = other.ReceiptImageId;
			ReceiptImageIdUnprotected = other.ReceiptImageIdUnprotected;
		}

		[DataMember(EmitDefaultValue = false)]
		public string MeKey { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string ExpKey { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string ExpName { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string LocationName { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string VendorName { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string Comment { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public decimal TransactionAmount { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string CrnCode { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public DateTime TransactionDate { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public byte[] ReceiptImage { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string ReceiptImageId { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string ReceiptImageIdUnprotected { get; set; }
		
		[DataMember(EmitDefaultValue = false)]
		public string HasReceiptImage { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string PctKey { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string CctKey { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public MobileEntryMileageDetails MileageDetails { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public List<MobileEntryFormField> MobileEntryFormFields { get; set; }

		public MobileEntry() { }

		public MobileEntry(DbDataReader Reader, ExpenseSessionUtils su, bool getImage)
				: this(Reader, su)
		{
			if (getImage)
			{
				HasReceiptImage = "N";
				if (Reader["RECEIPT_IMAGE"] != DBNull.Value)
				{
					ReceiptImage = (byte[])Reader["RECEIPT_IMAGE"];
					HasReceiptImage = "Y";
				}
				else if (!su.getCESVersion().StartsWith("72.0") && Reader["MOBILE_RECEIPT_IMAGE_ID"] != DBNull.Value)
				{
					HasReceiptImage = "Y";
				}
			}
			else
				HasReceiptImage = (Int32)Reader["HAS_RECEIPT_IMAGE"] == 1 ? "Y" : "N";

		}

		public MobileEntry(DbDataReader Reader, ExpenseSessionUtils su)
		{
			MeKey = su.getProtect().ProtectValue("MeKey", ((int)Reader["ME_KEY"]).ToString());

			ExpKey = DBUtils.GetNullableString(Reader["EXP_KEY"]);
			ExpName = DBUtils.GetNullableString(Reader["EXP_NAME"]);
			Comment = DBUtils.GetNullableString(Reader["COMMENT"]);
			LocationName = DBUtils.GetNullableString(Reader["LOCATION_NAME"]);
			VendorName = DBUtils.GetNullableString(Reader["VENDOR_NAME"]);
			CrnCode = DBUtils.GetNullableString(Reader["CRN_CODE"]);
			TransactionAmount = (decimal)Reader["TRANSACTION_AMOUNT"];
			TransactionDate = (DateTime)Reader["TRANSACTION_DATE"];
			if (!su.getCESVersion().StartsWith("72.0") && Reader["MOBILE_RECEIPT_IMAGE_ID"] != DBNull.Value)
			{
				SetReceiptImageId(su.getProtect(), Reader["MOBILE_RECEIPT_IMAGE_ID"].ToString());
		//		ReceiptImageIdUnprotected = Reader["MOBILE_RECEIPT_IMAGE_ID"].ToString();
		//		ReceiptImageId = su.getProtect().ProtectValue(ReceiptStore.RECEIPT_IMAGE_ID_PROTECT_KEY, ReceiptImageIdUnprotected);
			}
			if (Reader["PCT_KEY"] != DBNull.Value)
				PctKey = su.getProtect().ProtectValue("PctKey", ((int)Reader["PCT_KEY"]).ToString());
			if (Reader["CCT_KEY"] != DBNull.Value)
				CctKey = su.getProtect().ProtectValue("CctKey", ((int)Reader["CCT_KEY"]).ToString());

			// DMB MOB-17920
			// we have been passing the string value null into stored procedure for entry details
			// have added string null check......hopefully remove in the future
			// YW 10/24: Temperarily comment out, since this blocks ereceipt hot fix testing
			if (!su.getCESVersion().StartsWith("108."))
			{
				String entryDetails = DBUtils.GetNullableString(Reader["Entry_Details"]);
				if (!string.IsNullOrEmpty(entryDetails) && entryDetails != "null")
				{
					MileageDetails = Snowbird.MobileEntryUtils.GetMileageDetails(entryDetails);
				}
			}
		}

		public static implicit operator MobileEntry(Concur.API.mobile.V1.MobileEntry v)
		{
			return new Snowbird.MobileEntry()
			{
				MeKey = v.MeKey,
				ExpKey = v.ExpKey,
				ExpName = v.ExpName,
				LocationName = v.LocationName,
				VendorName = v.VendorName,
				Comment = v.Comment,
				TransactionAmount = v.TransactionAmount,
				CrnCode = v.CrnCode,
				TransactionDate = v.TransactionDate,
				ReceiptImage = v.ReceiptImage,
				ReceiptImageId = v.ReceiptImageId,
				ReceiptImageIdUnprotected = v.ReceiptImageIdUnprotected,
				HasReceiptImage = v.HasReceiptImage,
				PctKey = v.PctKey,
				CctKey = v.CctKey,
				MileageDetails = null,
				MobileEntryFormFields = null
			};
		}

		public static implicit operator Concur.API.mobile.V1.MobileEntry(MobileEntry v)
		{
			return new Concur.API.mobile.V1.MobileEntry()
			{
				MeKey = v.MeKey,
				ExpKey = v.ExpKey,
				ExpName = v.ExpName,
				LocationName = v.LocationName,
				VendorName = v.VendorName,
				Comment = v.Comment,
				TransactionAmount = v.TransactionAmount,
				CrnCode = v.CrnCode,
				TransactionDate = v.TransactionDate,
				ReceiptImage = v.ReceiptImage,
				ReceiptImageId = v.ReceiptImageId,
				ReceiptImageIdUnprotected = v.ReceiptImageIdUnprotected,
				HasReceiptImage = v.HasReceiptImage,
				PctKey = v.PctKey,
				CctKey = v.CctKey
			};
		}

		// DMB MOB-17920
		private static List<MobileEntryFormField> GetMobileEntryFormFields(String entryDetails)
		{
			try
			{
				dynamic mobileEntryFormfields = Newtonsoft.Json.JsonConvert.DeserializeObject(entryDetails);
				return mobileEntryFormfields.ToObject<List<MobileEntryFormField>>();
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static int GetMobileEntryCount(ExpenseSessionUtils su, bool unassigned)
		{
			return QuickExpenseV4Utils.CountQuickExpenses(su.getCurrentLoginName());
		}

		public static MobileEntry GetMobileEntry(ExpenseSessionUtils su, string protectedMeKey)
		{
			return GetMobileEntry(su, protectedMeKey, null);
		}

		public static MobileEntry GetMobileEntry(ExpenseSessionUtils su, string protectedMeKey, string protectedRptKey)
		{
			string meKey = su.getProtect().UnprotectNonBlankValue("MeKey", protectedMeKey);
			long empKey = su.getCurrentUserEmpKey();
			if (!string.IsNullOrEmpty(protectedRptKey))
			{
				// Let's get empKey from rptKey
				string rptKey = su.getProtect().UnprotectNonBlankValue("RptKey", protectedRptKey);
				object[] result = Report.getReportKeyInfo(rptKey, su);
				if (result != null)
				{
					empKey = (int)result[Report.IDX_RPT_KEY_INFO_EMP_KEY];
				}
			}
			return GetMobileEntry(su, empKey, meKey, true);
		}



		public static MeStatus[] DeleteMobileEntries(ExpenseSessionUtils su, string[] meKeys)
		{
			if (meKeys == null || meKeys.Length == 0)
				return null;

            return QuickExpenseV4Utils.DeleteMobileEntriesUsingService(
                    meKeys,
                    su.getProtect(),
                    su.getCurrentLoginName());

        }

		public static MeStatus SaveReceiptImageId(ExpenseSessionUtils su, string meKey, string receiptImageId)
		{

			MeStatus meStatus = new MeStatus();
			meStatus.Status = ActionStatus.SUCCESS;
			meStatus.MeKey = meKey;

			object[] parm = new object[] {
								su.getProtect().UnprotectNonBlankValue("MeKey", meKey),
								su.getProtect().UnprotectNonBlankValue(ReceiptStore.RECEIPT_IMAGE_ID_PROTECT_KEY, receiptImageId)
						};

			try
			{
				MobileEntry me = GetMobileEntry(su, meKey);
				me.ReceiptImageId = receiptImageId;
				meStatus = SaveMobileEntry(su, me, "N");
			}
			catch (Exception ex)
			{
				su.LogException("SaveMobileEntryReceipt", ex);
				meStatus.Status = ActionStatus.FAILURE;
				meStatus.ErrorMessage = CQLocale.FormatText("Mobile", "SaveMobileEntryReceiptError");
			}
			return meStatus;
		}

		public static string GetEntryDetails(MobileEntry me)
		{
			// DMB due to no check we pass the string value null into stored procedure
			// causing issue deserializing entry details back to List as value is string null not null

			if (me.MileageDetails != null)
			{
				return MobileEntryUtils.serializeMileageDetails(me.MileageDetails);
			}
			else if (me.MobileEntryFormFields != null)
			{
				JavaScriptSerializer js = new JavaScriptSerializer();
				StringBuilder s = new StringBuilder();
				js.Serialize(me.MobileEntryFormFields, s);
				return s.ToString();
			}

			return null;
		}




		public static long? ToNullableLong(string number)
		{
			long n;
			bool success = long.TryParse(number, out n);
			return success ? (long?)n : null;
		}






		public static MeStatus SaveMobileEntry(ExpenseSessionUtils su, MobileEntry me, string clearImageFlag)
		{
			OTProtect protect = su.getProtect();

			string MeKeyUnprotected = protect.UnprotectNonBlankValue("MeKey", me.MeKey);
			if (QuickExpenseV4Utils.UseQuickExpenseService())
			{
				string loginId = su.getCurrentLoginName();
				return QuickExpenseV4Utils.SaveMobileEntry(me, protect, MeKeyUnprotected, loginId);
			}
			else
			{
				long empKey = su.getCurrentUserEmpKey();
				DbConnection conn = su.getCESEntityConnection();
				return DoSaveMobileEntry(me, protect, MeKeyUnprotected, empKey, clearImageFlag, conn);
			}
		}

		//V4 API


		public static MeStatus DoSaveMobileEntry(
				MobileEntry me,
				OTProtect protect,
				string MeKeyUnprotected,
				long empKey,
				string clearImageFlag,
				DbConnection conn)
		{
			MeStatus st = new MeStatus();
			st.Status = ActionStatus.FAILURE;
			st.ErrorMessage = CQLocale.FormatText("Mobile", "SaveMobileEntryError");
			return st;
		}

		public static void AddMeToRpeNode(XmlNode rpeNode, MobileEntry curME, OTProtect prot, string rptKey, Object lnKey)
		{
			// DMB MOB-17920
			AddGemsToRpeNode(rpeNode, curME);

			ReportUtils.SetFieldValue(rpeNode, "ExpKey", curME.ExpKey);
			ReportUtils.RemoveField(rpeNode, "CrnKey");
			if (curME.Comment != null)
				ReportUtils.SetFieldValue(rpeNode, "Comment", curME.Comment);
			if (curME.LocationName != null)
				ReportUtils.SetFieldValue(rpeNode, "LocName", curME.LocationName);
			ReportUtils.SetFieldValue(rpeNode, "TransactionAmount", curME.TransactionAmount.ToString());

			// MOB-6018 Strip off the time component
			string tranDate = curME.TransactionDate.ToString("u");
			tranDate = ReportUtils.stripTimeFromDate(tranDate);

			ReportUtils.SetFieldValue(rpeNode, "TransactionDate", tranDate);
			ReportUtils.SetFieldValue(rpeNode, "CrnCode", curME.CrnCode);
			ReportUtils.SetFieldValue(rpeNode, "MeKey", prot.UnprotectNonBlankValue("MeKey", curME.MeKey));
			ReportUtils.SetFieldValue(rpeNode, "isNew", "true");
			if (!string.IsNullOrEmpty(curME.VendorName))  // MOB-5234 make sure vendor field remains null, if add via ME.
				ReportUtils.SetFieldValue(rpeNode, "VendorDescription", curME.VendorName);
			ReportUtils.SetFieldValue(rpeNode, "RptKey", rptKey);
			if (!string.IsNullOrEmpty(curME.ReceiptImageId))
			{
				String unprotReceiptImageId = prot.UnprotectValue(ReceiptStore.RECEIPT_IMAGE_ID_PROTECT_KEY, curME.ReceiptImageId);
				// JIRA MOB-8156 - ensure receipts attached to quick expenses are passed in 'SaveExpense' call as 'ReceiptImageId'.
				ReportUtils.SetFieldValue(rpeNode, "ReceiptImageId", unprotReceiptImageId);
			}

			if (lnKey != null)
			{
				ReportUtils.SetFieldValue(rpeNode, "LnKey", lnKey.ToString());
			}



		}

		// DMB MOB-17920
		// EntryDetails contain a json object with form field information.
		// we will loop round the field definitions
		private static void AddGemsToRpeNode(XmlNode rpeNode, MobileEntry curME)
		{
			try
			{
				if (curME.MobileEntryFormFields != null)
				{
					foreach (MobileEntryFormField mobileEntryFormField in curME.MobileEntryFormFields)
					{
						if (!string.IsNullOrEmpty(mobileEntryFormField.LiKey))
						{
							ReportUtils.SetFieldValue(rpeNode, mobileEntryFormField.ID, mobileEntryFormField.LiKey);
						}
						else if (!string.IsNullOrEmpty(mobileEntryFormField.FieldValue))
						{
							ReportUtils.SetFieldValue(rpeNode, mobileEntryFormField.ID, mobileEntryFormField.FieldValue);

						}
					}
				}
			}
			catch (Exception)
			{
				// should not occur added as precaution
			}

		}
		public static ActionStatus AddToReport(ExpenseSessionUtils su, AddToReportMap m, AddToReportStatus st)
		{
			string hierXml = su.getHierarchyPathXML("EXP", "EXP");
			OTProtect prot = su.getProtect();

			String rptKey = prot.UnprotectNonBlankValue("RptKey", m.RptKey);
			// save expense
			string[] MeKeys = m.MeKeys;

			if (!string.IsNullOrEmpty(rptKey) && MeKeys != null && MeKeys.Length > 0)
			{
				object[] rptKeyInfo = Report.getReportKeyInfo(rptKey, su);
				int cPolKey = (int)rptKeyInfo[Report.IDX_RPT_KEY_INFO_POL_KEY];
				Dictionary<string, ExpenseType> expDict = ExpenseType.GetExpTypeDictByExpKey(su, cPolKey);

				st.Status = ActionStatus.SUCCESS;
				st.RptKey = prot.ProtectNonBlankValue("RptKey", rptKey);

				st.Entries = new MeStatus[MeKeys.Length];
				for (int ix = 0; MeKeys != null && ix < MeKeys.Length; ix++)
				{
					st.Entries[ix] = new MeStatus();
					st.Entries[ix].MeKey = MeKeys[ix];

					if (m.matchedMeKeys.ContainsKey(MeKeys[ix]))
					{
						st.Entries[ix].Status = "SUCCESS_SMARTEXP";
						st.Entries[ix].ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", "SmartExpenseMatched");
						continue;
					}

					st.Entries[ix].Status = ActionStatus.SUCCESS;

					MobileEntry curME = GetMobileEntry(su, su.getCurrentUserEmpKey(), prot.UnprotectNonBlankValue("MeKey", MeKeys[ix]), false);

					if (curME == null)
					{
						st.Entries[ix].Status = "MobileEntryNotAvail";
						st.Entries[ix].ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", "MobileEntryNotAvail");
					}
					else
					{
						// If a location name was provided, then look the the corresponding lnKey.
						Object lnKey = null;
						if (curME.LocationName != null && curME.LocationName.Length > 0)
						{

							object[] param = new object[] { su.getCurrentUserEmpKey(), su.getCESLangCode(), curME.LocationName, DBNull.Value };
							DbDataReader dataReader = null;
							try
							{
								dataReader = DBUtils.ReturnDataReader("CE_LISTS_SearchLocations", su.getCESEntityConnection(), param);
								while (dataReader.Read())
								{
									lnKey = (int)(dataReader["LN_KEY"]);
								}
							}
							catch (Exception)
							{
								// If we don't find an lnKey, we can still use the location name, so just keep going.
							}
							finally
							{
								if (dataReader != null) dataReader.Close();
							}
						}

						// If expense types not found, create an undefined expense type
						if (expDict == null || !expDict.ContainsKey(curME.ExpKey))
							curME.ExpKey = "UNDEF";

						XmlNode rpeNode = ReportEntry.getDefaultReportEntryXML(su, rptKey, curME.ExpKey);
						// Set all fields
						AddMeToRpeNode(rpeNode, curME, prot, rptKey, lnKey);

						// MOB-13747 Copy down business purpose from report
						if (!string.IsNullOrEmpty(m.businessPurpose))
						{
							ReportUtils.SetFieldValue(rpeNode, "Description", m.businessPurpose);
						}

						if (m.AttendeesMap == null)
						{
							// MOB-10339 If Attendees field is not RW on entry form, do not add default attendee
							bool toAddAttendee = ItemizationDetail.containsRWAttendeesField(su, "TRAVELER", rptKey, curME.ExpKey,
													(int)su.getCurrentUserEmpKey(), cPolKey, false);

							// Add default attendee
							if (toAddAttendee)
								AttendeeSearchCriteria.AddDefaultAttendeeToRpeNode(su, curME.ExpKey, rpeNode, expDict);
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
							st.Entries[ix].Status = ActionStatus.FAILURE;
							st.Entries[ix].ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", "AddMobileEntryError");
						}
						else
						{
							string status = su.getMidtierActionStatus(res);
							if (status != "SUCCESS!")
							{
								st.Entries[ix].Status = ActionStatus.FAILURE;//status;
								st.Entries[ix].ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", "AddMobileEntryError");
								//Concur.Localization.CQLocale.FormatText("Mobile", "MobileError"+status);
							}
							else
							{
								XmlNode node = res.SelectSingleNode("/Response/Body/Expense/RpeKey");
								AttendeeEntryMap.SetRpeKey(curME.MeKey, node == null ? null : node.InnerText, m.AttendeesMap);
							}
						}
					}
				}

				//ReportDetail result = new ReportDetail();
				//result.PopulateData(su, "TRAVELER", rptKey);
				//st.Report = result;
			}
			else
			{
				st.Status = ActionStatus.FAILURE;
				st.ErrorMessage = Concur.Localization.CQLocale.FormatText("Mobile", "CannotCreateReport");
			}
			return st;
		}

		public static object[] GetMobileEntryParms(
				long empKey,
				string langCode,
				string MeKey = null,
				bool include_assigned = false,
				int? pct_key = null,
				int? cct_key = null)
		{
			return new object[] {
								empKey,
								langCode,
								MeKey == null ? (object)DBNull.Value : MeKey,
								include_assigned ? 1 : 0,
								pct_key == null ? (object)DBNull.Value : pct_key,
								cct_key == null ? (object)DBNull.Value : cct_key
								};
		}

		public static MobileEntry GetMobileEntry(
				ExpenseSessionUtils su,
				long empKey,
				string unprotectedMeKey = null,
				bool include_assigned = false,
				int? pct_key = null,
				int? cct_key = null)
		{
			MobileEntry[] entries = SmartExpenseEmtUtils.GetMobileEntriesFromEmt(
					su.getProtect(),
					su.getCurrentCompanyId(),
					su.getCESEntityId(),
					su.getCESLangCode(),
					su.getCurrentUserEmpKey(),
					su.getCurrentUserId());

			if (entries != null)
			{
				string protectedMeKey = su.getProtect().ProtectNonBlankValue("MeKey", unprotectedMeKey);
				foreach (MobileEntry me in entries)
					if (me.MeKey == protectedMeKey)
						return me;
			}
			return null;
		}

		public static MobileEntry[] GetMobileEntryList(ExpenseSessionUtils su)
		{
			return SmartExpenseEmtUtils.GetMobileEntriesFromEmt(
					su.getProtect(),
					su.getCurrentCompanyId(),
					su.getCESEntityId(),
					su.getCESLangCode(),
					su.getCurrentUserEmpKey(),
					su.getCurrentUserId());
		}
	}
}
 