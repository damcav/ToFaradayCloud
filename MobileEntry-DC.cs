using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Data.Common;
using Concur.API.mobile.Utils;
using Concur.Core;
using Concur.Localization;
using Snowbird;

[assembly: ContractNamespace("http://schemas.datacontract.org/2014/07/Snowbird", ClrNamespace = "Concur.API.mobile.V1")]

namespace Concur.API.mobile.V1
{
	// TODO - move
	public class ReceiptStore
	{
		public const string RECEIPT_IMAGE_ID_PROTECT_KEY = "ReceiptImageId";
	}
	//// OCR receipt objects
	//[DataContract(Namespace = "", Name = "ActionStatus")]
	//public class RCStatus : ActionStatus // RC - ReceiptCapture
	//{
	//	[DataMember(EmitDefaultValue = false)]
	//	public string SmartExpenseId { get; set; }
	//}

	//[DataContract(Namespace = "", Name = "ReceiptCapture")]
	//public class ReceiptCapture // SmartExpenseItem
	//{
	//	[DataMember(EmitDefaultValue = false)]
	//	public string SmartExpenseId { get; set; }
	//	[DataMember(EmitDefaultValue = false)]
	//	public string RcKey { get; set; }
	//	[DataMember(EmitDefaultValue = false)]
	//	public string ExpKey { get; set; }
	//	[DataMember(EmitDefaultValue = false)]
	//	public string ExpName { get; set; }
	//	[DataMember(EmitDefaultValue = false)]
	//	public string VendorName { get; set; }
	//	[DataMember(EmitDefaultValue = false)]
	//	public string Comment { get; set; }
	//	[DataMember(EmitDefaultValue = false)]
	//	public decimal TransactionAmount { get; set; }
	//	[DataMember(EmitDefaultValue = false)]
	//	public string CrnCode { get; set; }
	//	[DataMember(EmitDefaultValue = false)]
	//	public string CrnKey { get; set; }
	//	[DataMember(EmitDefaultValue = false)]
	//	public DateTime TransactionDate { get; set; }
	//	[DataMember(EmitDefaultValue = false)]
	//	public string ReceiptImageId { get; set; }

	//	public string clearRcKey;

	//	public ReceiptCapture(DbDataReader Reader, OTProtect prot)
	//	{
	//		clearRcKey = ((int)Reader["RC_KEY"]).ToString();
	//		RcKey = AddToReportMap.getProtSmartExpenseIdFromRcKey(clearRcKey, prot);
	//		SmartExpenseId = RcKey;

	//		ExpKey = DBUtils.GetNullableString(Reader["EXP_KEY"]);
	//		ExpName = DBUtils.GetNullableString(Reader["EXP_NAME"]);
	//		Comment = DBUtils.GetNullableString(Reader["COMMENT"]);
	//		VendorName = DBUtils.GetNullableString(Reader["VENDOR_DESCRIPTION"]);
	//		CrnCode = DBUtils.GetNullableString(Reader["CRN_CODE"]);
	//		if (Reader["TRANSACTION_AMOUNT"] != DBNull.Value)
	//		{
	//			TransactionAmount = (decimal)Reader["TRANSACTION_AMOUNT"];
	//		}
	//		if (Reader["TRANSACTION_DATE"] != DBNull.Value)
	//		{
	//			TransactionDate = (DateTime)Reader["TRANSACTION_DATE"];
	//		}
	//		ReceiptImageId = prot.ProtectValue(ReceiptStore.RECEIPT_IMAGE_ID_PROTECT_KEY, Reader["RECEIPT_IMAGE_ID"].ToString());
	//	}


	//	/**
	//	* <summary>
	//	* Will retrieve a list of unassigned ReceiptCapture items with a status of 'M_DONE' or 'A_DONE'.
	//	* </summary>
	//	* <param name="su">an expense session utils object.</param>
	//	* <returns>
	//	*      an array of ReceiptCapture items.
	//	* </returns>
	//	*/
	//	public static ReceiptCapture[] GetReceiptCaptureList(ExpenseSessionUtils su)
	//	{
	//		return GetReceiptCaptureList(su, 'N');
	//	}

	//	/**
	//	* <summary>
	//	* Will retrieve a list of unassigned ReceiptCapture items.
	//	* </summary>
	//	* <param name="su">an expense session utils object.</param>
	//	* <param name="includeNotDone">contains either 'Y' or 'N' indicating whether "not done" items should be included.</param>
	//	* <returns>
	//	*      an array of ReceiptCapture items.
	//	* </returns>
	//	*/
	//	public static ReceiptCapture[] GetReceiptCaptureList(ExpenseSessionUtils su, char includeNotDone)
	//	{
	//		object[] parm = new object[] { su.getCurrentUserEmpKey(), su.getCESLangCode(), includeNotDone }; // Include only M_DONE status
	//		List<ReceiptCapture> tmp = new List<ReceiptCapture>();
	//		using (var dr = DBUtils.ReturnDataReader("CE_RECEIPTCAPTURE_GetUnassignedOcrReceipts", su.getCESEntityConnection(), parm))
	//		{
	//			while (dr.Read())
	//			{
	//				tmp.Add(new ReceiptCapture(dr, su.getProtect()));
	//			}
	//		}
	//		return (ReceiptCapture[])tmp.ToArray();
	//	}

	//	public static int GetReceiptCaptureCount(ExpenseSessionUtils su, char includeNotDone)
	//	{
	//		object[] parm = new object[] { su.getCurrentUserEmpKey(), su.getCESLangCode(), includeNotDone }; // Include only M_DONE status
	//		List<ReceiptCapture> tmp = new List<ReceiptCapture>();
	//		var dr = DBUtils.ReturnDynamic("CE_RECEIPTCAPTURE_GetUnassignedOcrReceipts", su.getCESEntityConnection(), parm);
	//		return dr.Count;
	//	}


	//}

	[DataContract(Namespace = "", Name = "ActionStatus")]
	public class MeStatus : ActionStatus
	{
		[DataMember(EmitDefaultValue = false)]
		public string MeKey { get; set; }

		public MeStatus() { }

		public static implicit operator MeStatus(Snowbird.MeStatus v)
		{
			return new MeStatus() { MeKey = v.MeKey, ErrorMessage = v.ErrorMessage, Status = v.Status };
		}
	}

	[DataContract(Namespace = "", Name = "MobileEntry")]
	public class MobileEntry
	{
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

		public MobileEntry() { }

		public MobileEntry(DbDataReader Reader, EmpInfo empInfo, bool getImage)
			: this(Reader, empInfo)
		{
			if (getImage)
			{
				HasReceiptImage = "N";
				if (Reader["RECEIPT_IMAGE"] != DBNull.Value)
				{
					ReceiptImage = (byte[])Reader["RECEIPT_IMAGE"];
					HasReceiptImage = "Y";
				}
				else if (Reader["MOBILE_RECEIPT_IMAGE_ID"] != DBNull.Value)
				{
					HasReceiptImage = "Y";
				}
			}
			else
				HasReceiptImage = (Int32)Reader["HAS_RECEIPT_IMAGE"] == 1 ? "Y" : "N";

		}

		public MobileEntry(DbDataReader Reader, EmpInfo empInfo)
		{
			MeKey = empInfo.Protect.ProtectValue("MeKey", ((int)Reader["ME_KEY"]).ToString());

			ExpKey = DBUtils.GetNullableString(Reader["EXP_KEY"]);
			ExpName = DBUtils.GetNullableString(Reader["EXP_NAME"]);
			Comment = DBUtils.GetNullableString(Reader["COMMENT"]);
			LocationName = DBUtils.GetNullableString(Reader["LOCATION_NAME"]);
			VendorName = DBUtils.GetNullableString(Reader["VENDOR_NAME"]);
			CrnCode = DBUtils.GetNullableString(Reader["CRN_CODE"]);
			TransactionAmount = (decimal)Reader["TRANSACTION_AMOUNT"];
			TransactionDate = (DateTime)Reader["TRANSACTION_DATE"];
			if (Reader["MOBILE_RECEIPT_IMAGE_ID"] != DBNull.Value)
			{
				ReceiptImageIdUnprotected = Reader["MOBILE_RECEIPT_IMAGE_ID"].ToString();
				ReceiptImageId = empInfo.Protect.ProtectValue(ReceiptStore.RECEIPT_IMAGE_ID_PROTECT_KEY, ReceiptImageIdUnprotected);
			}
			if (Reader["PCT_KEY"] != DBNull.Value)
				PctKey = empInfo.Protect.ProtectValue("PctKey", ((int)Reader["PCT_KEY"]).ToString());
			if (Reader["CCT_KEY"] != DBNull.Value)
				CctKey = empInfo.Protect.ProtectValue("CctKey", ((int)Reader["CCT_KEY"]).ToString());
		}

	}
}