using System;
using System.Collections.Generic;
using System.Web;
using System.Runtime.Serialization;
using System.Xml;
using Snowbird.Service.Receipt;
using System.Data.Common;
using Concur.Core;
using CESMidtier = Concur.Spend.CESMidtier;
using Concur.Utils;
using System.Net;
using Snowbird.Controllers;
using Concur.CESClient;
using System.Web.Script.Serialization;

namespace Snowbird
{


	[DataContract(Namespace = "", Name = "DeleteReceiptAction")]
	public class DeleteReceiptAction
	{
		[DataMember(EmitDefaultValue = false)]
		public string ReceiptImageId { get; set; }

		public DeleteReceiptAction() { }
	}

	[DataContract(Namespace = "", Name = "DeleteMultipleReceipts")]
	public class DeleteMultipleReceiptsAction
	{
		[DataMember(EmitDefaultValue = false)]
		public string ReceiptImageIds { get; set; }

		public DeleteMultipleReceiptsAction() { }
	}

	[DataContract(Namespace = "", Name = "AppendReceiptAction")]
	public class AppendReceiptAction
	{
		[DataMember(EmitDefaultValue = false)]
		public string ToReceiptImageId { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public String FromReceiptImageId { get; set; }

		public AppendReceiptAction() { }
	}

	[DataContract(Namespace = "", Name = "AddReportReceiptAction")]
	public class AddReportReceiptAction
	{
		[DataMember(EmitDefaultValue = false)]
		public string RptKey { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string ReceiptImageId { get; set; }

		public AddReportReceiptAction() { }
	}

	[DataContract(Namespace = "", Name = "ActionStatus")]
	public class GetReceiptImageUrlStatus : ActionStatus
	{
		[DataMember(EmitDefaultValue = false)]
		public string ReceiptImageURL { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string FileType { get; set; }

		// Contains the Timestamp associated with image.
		[DataMember(EmitDefaultValue = false)]
		public TimeStamp TimeStamp { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string ReceiptImageIdUnprotected { get; set; }
		public GetReceiptImageUrlStatus() { }
	}

	public class JsonTimeStamp
	{
		public string status { get; set; }

		public string resolution { get; set; }

		public string colorDepth { get; set; }

		public string captureDate { get; set; }
	}

	[DataContract(Namespace = "")]
	public class TimeStamp
	{
		// Status of the Receipt Analyzed.
		[DataMember(EmitDefaultValue = false)]
		public String TimeStampStatus { get; set; }

		// Resolution of the processed Image.
		[DataMember(EmitDefaultValue = false)]
		public String Resolution { get; set; }

		// Color Depth of the Receipt.
		[DataMember(EmitDefaultValue = false)]
		public String ColorDepth { get; set; }

		// Image captured date.
		[DataMember(EmitDefaultValue = false)]
		public String CaptureDate { get; set; }

		public TimeStamp() { }

		public TimeStamp(JsonTimeStamp timeStampValue)
		{

			this.Resolution = timeStampValue.resolution;
			this.TimeStampStatus = timeStampValue.status;
			this.ColorDepth = timeStampValue.colorDepth;
			this.CaptureDate = timeStampValue.captureDate;
		}
	}



	[DataContract(Namespace = "", Name = "ReceiptInfo")]
	public class ReceiptImageInfo
	{
		// Contains the receipt image ID (GUID) PROTECTED
		[DataMember(EmitDefaultValue = false)]
		public String ReceiptImageId { get; set; }

		// Contains the receipt image ID (GUID) UNPROTECTED
		[DataMember(EmitDefaultValue = false)]
		public String ReceiptImageIdUnprotected { get; set; }

		// Contains the file type, i.e., PNG, JPG, etc.
		[DataMember(EmitDefaultValue = false)]
		public String FileType { get; set; }

		// Contains the GMT timestamp of when the image was received at the server.
		[DataMember(EmitDefaultValue = false)]
		public String ImageDate { get; set; }

		// Contains the receipt image filename that was uploaded.
		[DataMember(EmitDefaultValue = false)]
		public String FileName { get; set; }

		// Contains the receipt system origin tag.
		[DataMember(EmitDefaultValue = false)]
		public String SystemOrigin { get; set; }

		// Contains the receipt image origin tag.
		[DataMember(EmitDefaultValue = false)]
		public String ImageOrigin { get; set; }

		// Contains the receipt URL to retrieve the full sized image.
		[DataMember(EmitDefaultValue = false)]
		public String ImageUrl { get; set; }

		// Contains the receipt URL to retrieve a thumbnail sized image.
		[DataMember(EmitDefaultValue = false)]
		public String ThumbUrl { get; set; }

		// Contains the Timestamp associated with image.
		[DataMember(EmitDefaultValue = false)]
		public TimeStamp TimeStamp { get; set; }

		public ReceiptImageInfo() { }

		public ReceiptImageInfo(OTProtect protect, String receiptImageIdUnprotected, String fileType, String imageDate, String fileName, String systemOrigin, String imageOrigin, String imageUrl, String thumbUrl, TimeStamp timeStamp)
		{
			this.ReceiptImageIdUnprotected = receiptImageIdUnprotected;
			this.ReceiptImageId = protect.ProtectValue(ReceiptStore.RECEIPT_IMAGE_ID_PROTECT_KEY, receiptImageIdUnprotected);
			this.FileType = fileType;
			this.ImageDate = imageDate;
			this.FileName = fileName;
			this.SystemOrigin = systemOrigin;
			this.ImageOrigin = imageOrigin;
			this.ImageUrl = imageUrl;
			this.ThumbUrl = thumbUrl;
			this.TimeStamp = timeStamp;
		}

	}

	[DataContract(Namespace = "", Name = "ActionStatus")]
	public class GetReportPdfUrlStatus : ActionStatus
	{
		[DataMember(EmitDefaultValue = false)]
		public string PdfURL { get; set; }

		public GetReportPdfUrlStatus() { }
	}

	[DataContract(Namespace = "", Name = "ActionStatus")]
	public class GetReceiptImageUrlsStatus : ActionStatus
	{
		[DataMember(EmitDefaultValue = false)]
		public ReceiptImageInfo[] ReceiptInfos { get; set; }

		public GetReceiptImageUrlsStatus() { }
	}

	[DataContract(Namespace = "", Name = "ActionStatus")]
	public class UploadImageStatus : ActionStatus
	{
		[DataMember(EmitDefaultValue = false)]
		public string ReceiptImageId { get; set; }

		public UploadImageStatus() { }

	}



	public class ReceiptStore
	{
		public static string RECEIPT_IMAGE_ID_PROTECT_KEY = "ReceiptImageId";

		private static string ME_KEY_PREFIX = "RS_IMG_ID";

		// Contains the value for 'ImageOrigin' that tags a receipt as having come from a mobile device.
		public static string MOBILE_EXPENSE_IMAGE_ORIGIN = "mobile";

		// An enumeration used to determine a receipt image type.
		public enum DocumentType { PNG, JPG, PDF, UNKNOWN };

		public static string IMAGING_SOURCE_CTE = "CTE";
		public static string IMAGING_SOURCE_TM = "TM";

		/**
		 * <summary>
		 * Will determine whether image data stored in a byte array is a PNG, JPG, PDF or UNKNOWN
		 * if not one of the first 3.
		 * </summary>
		 * <param name="data">the image data.</param>
		 * <returns>
		 *      an instance of DocumentType containing the result.
		 * </returns>
		 */
		public static DocumentType getDocumentType(byte[] data)
		{
			DocumentType docType = DocumentType.UNKNOWN;
			if (data != null && data.Length >= 4)
			{
				if ((0x000000FF & data[0]) == 0x89 && (0x000000FF & data[1]) == 0x50
						 && (0x000000FF & data[2]) == 0x4e && (0x000000FF & data[3]) == 0x47)
				{
					docType = DocumentType.PNG;
				}
				else if ((0x000000FF & data[0]) == 0xff && (0x000000FF & data[1]) == 0xd8
						&& (0x000000FF & data[2]) == 0xff && (0x000000FF & data[3]) == 0xe0)
				{
					// JFIF
					docType = DocumentType.JPG;
				}
				else if ((0x000000FF & data[0]) == 0xff && (0x000000FF & data[1]) == 0xd8
						&& (0x000000FF & data[2]) == 0xff && (0x000000FF & data[3]) == 0xe1)
				{
					// EXIF
					docType = DocumentType.JPG;
				}
				else if ((0x000000FF & data[0]) == 0x25 && (0x000000FF & data[1]) == 0x50
						&& (0x000000FF & data[2]) == 0x44 && (0x000000FF & data[3]) == 0x46)
				{
					// PDF
					docType = DocumentType.PDF;
				}
			}
			return docType;
		}

		/**
		 * Will prefix a receipt store image ID with a value.
		 */
		public static string MakeMeKeyPrefixReceiptStoreId(string receiptImageId)
		{
			return ME_KEY_PREFIX + receiptImageId;
		}

		/**
		 * Will determine whether an meKey value has been prefixed with
		 * a Receipt Store ID prefix indicating the meKey is really a receipt
		 * store image id.
		 */
		public static Boolean IsMeKeyReceiptStoreImageId(string meKey)
		{
			return meKey.StartsWith(ME_KEY_PREFIX);
		}

		/**
		 * Will return the remainder of the 'receiptImageId' after stripping
		 * off the MeKey prefix.
		 */
		public static string StripMeKeyPrefix(string receiptImageId)
		{
			return receiptImageId.SubstringAfter(ME_KEY_PREFIX);
		}

		/**
		 * <summary>
		 * Will add a receipt image not stored in the receipt store to a report.
		 * </summary>
		 * <param name="su">an expense session utils object.</param>
		 * <param name="protRptKey">a protected report key.</param>
		 * <param name="data">image data.</param>
		 * <returns>
		 *      an instance of ActionStatus indicating success or failure.
		 * </returns>
		 */
		public static ActionStatus AddReportReceiptImage(ExpenseSessionUtils su, string protRptKey, byte[] data)
		{
			ActionStatus st = new ActionStatus();
			st.Status = ActionStatus.SUCCESS;
			// Set the entity ID.
			string entityID = su.getCESEntityId();

			OTProtect protect = su.getProtect();

			// Unprotect the report key.
			string clearRptKey = null;
			try
			{
				// TravelManager reportKeys are not encrypted
				if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
				{
					clearRptKey = protRptKey;
				}
				else
				{
					clearRptKey = protect.UnprotectValue("RptKey", protRptKey);
				}
			}
			catch (ArgumentException /* argExc */)
			{
				// TODO: set localized error message.
				// TODO: log failure.
				throw new HttpException(400, "Bad Request - invalid report key.");
			}

			string reportId;
			int icKey;
			// Travel Manager is passing in the unencrypted report id
			if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
			{
				reportId = clearRptKey;
				icKey = Snowbird.Controllers.GovTravelManagerController.getTMICKey();
			}
			else
			{
				// Obtain the report id and policy key based on the report key.
				object[] keys = Report.getReportKeyInfo(clearRptKey, su);
				reportId = (string)keys[Report.IDX_RPT_KEY_INFO_RPT_ID];
				int polKey = (int)keys[Report.IDX_RPT_KEY_INFO_POL_KEY];

				// Set the IC Key.
				icKey = GetPolicyICKey(su, polKey);
			}

			// Set the reference id.
			DateTime Epoch = new DateTime(1970, 1, 1);
			long currentTimeMillis = (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;
			string refId = Convert.ToString(currentTimeMillis);
			// Base64 encode the content.
			string content = Convert.ToBase64String(data);

			if (icKey != -1)
			{
				try
				{
					PutImageRequest request = new PutImageRequest(entityID, icKey, GetImagingUrl(su), GetImagingKey(su), GetIncludeSource(su), reportId, refId, content, GetImagingSource(su), su.getCurrentLoginName());
					PutImageReply reply = (PutImageReply)request.process();
					if (!reply.isSuccess())
					{
						st.Status = ActionStatus.FAILURE;
						st.ErrorMessage = reply.getStatusMessage();
						// TODO: set localized error message.
						String errorMsg = "PutImageRequest failed: ";
						if (!string.IsNullOrEmpty(st.ErrorMessage))
						{
							errorMsg += st.ErrorMessage;
						}
						Exception e = new Exception(errorMsg);
						Logger.LogError(e, true, "Snowbird - AddReportReceiptImage failed.");
					}
					else
					{
						// Flip the flag that a receipt image is available on the report.
						object[] parm = new object[] { reportId };
						DBUtils.ReturnCode("CE_IMAGING_SetReceiptImageAvailable", su.getCESEntityConnection(), parm);
						try
						{
							ApproveReject.MarkUploadReportReceipt(su, clearRptKey, su.getCurrentUserEmpKey());
						}
						finally { }
					}
				}
				catch (Exception e)
				{
					st.Status = ActionStatus.FAILURE;
					// TODO: set localized error message.
					Logger.LogError(e, true, "Snowbird - AddReportReceiptImage failed.");
				}
			}
			else
			{
				throw new HttpException(500, "Imaging configuration not available for policy.");
			}

			return st;
		}


		/**
		 * <summary>
		 * Will add a receipt image to a report within the receipt store.
		 * </summary>
		 * <param name="su">an expense session utils object.</param>
		 * <param name="protRptKey">a protected report key.</param>
		 * <param name="protReceiptImageId">a protected receipt image id.</param>
		 * <returns>
		 *      an instance of ActionStatus indicating success or failure.
		 * </returns>
		 */
		public static ActionStatus AddReportReceiptImageId(ExpenseSessionUtils su, string protRptKey, string protReceiptImageId)
		{
			ActionStatus st = new ActionStatus();
			st.Status = ActionStatus.SUCCESS;
			// Set the entity ID.
			string entityID = su.getCESEntityId();

			OTProtect protect = su.getProtect();
			// Unprotect the receipt image id.
			string clearReceiptImageId = null;
			try
			{

				// TravelManager imageIds are not encrypted
				if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
				{
					clearReceiptImageId = protReceiptImageId;
				}
				else
				{
					clearReceiptImageId = protect.UnprotectValue(RECEIPT_IMAGE_ID_PROTECT_KEY, protReceiptImageId);
				}
			}
			catch (ArgumentException /* argExc */ )
			{
				// TODO: set localized error message.
				// TODO: log failure.
				throw new HttpException(400, "Bad Request - invalid receipt image id.");
			}
			// Unprotect the report key.
			string clearRptKey = null;
			try
			{
				// TravelManager reportKeys are not encrypted
				if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
				{
					clearRptKey = protRptKey;
				}
				else
				{
					clearRptKey = protect.UnprotectValue("RptKey", protRptKey);
				}
			}
			catch (ArgumentException /* argExc */)
			{
				// TODO: set localized error message.
				// TODO: log failure.
				throw new HttpException(400, "Bad Request - invalid report key.");
			}

			string reportId;
			int icKey;
			// Travel Manager is passing in the unencrypted report id
			if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
			{
				reportId = clearRptKey;
				icKey = Snowbird.Controllers.GovTravelManagerController.getTMICKey();
			}
			else
			{
				// Obtain the report id and policy key based on the report key.
				object[] keys = Report.getReportKeyInfo(clearRptKey, su);
				reportId = (string)keys[Report.IDX_RPT_KEY_INFO_RPT_ID];
				int polKey = (int)keys[Report.IDX_RPT_KEY_INFO_POL_KEY];

				// Set the IC Key.
				icKey = GetPolicyICKey(su, polKey);
			}


			if (icKey != -1)
			{
				try
				{
					AssociateImageRequest request = new AssociateImageRequest(entityID, icKey, GetImagingUrl(su), GetImagingKey(su), GetIncludeSource(su), clearReceiptImageId, reportId, GetImagingSource(su));
					AssociateImageReply reply = (AssociateImageReply)request.process();
					if (!reply.isSuccess())
					{
						st.Status = ActionStatus.FAILURE;
						st.ErrorMessage = reply.getStatusMessage();
						// TODO: set localized error message.
						String errorMsg = "AssociateImageRequest failed: ";
						if (!string.IsNullOrEmpty(st.ErrorMessage))
						{
							errorMsg += st.ErrorMessage;
						}
						Exception e = new Exception(errorMsg);
						Logger.LogError(e, true, "Snowbird - AddReportReceiptImageId failed.");
					}
					else
					{
						// Flip the flag that a receipt image is available on the report.
						object[] parm = new object[] { reportId };
						DBUtils.ReturnCode("CE_IMAGING_SetReceiptImageAvailable", su.getCESEntityConnection(), parm);
						try
						{
							ApproveReject.MarkUploadReportReceipt(su, clearRptKey, su.getCurrentUserEmpKey());
						}
						finally { }
					}
				}
				catch (Exception e)
				{
					st.Status = ActionStatus.FAILURE;
					// TODO: set localized error message.
					Logger.LogError(e, true, "Snowbird - AddReportReceiptImageId failed.");
				}
			}
			else
			{
				throw new HttpException(500, "Imaging configuration not available for policy.");
			}

			return st;
		}

		/**
		* <summary>
		* Will add a receipt image to a report line item.
		* </summary>
		* <param name="su">an expense session utils object.</param>
		* <param name="protRptKey">a protected report key.</param>
		* <param name="protReceiptImageId">a protected receipt image id.</param>
		* <returns>
		*      an instance of ActionStatus indicating success or failure.
		* </returns>
		*/
		public static ActionStatus AddLineItemReceiptImageId(ExpenseSessionUtils su, string protRptKey, string protRptEntKey, string protReceiptImageId)
		{
			ActionStatus st = new ActionStatus();
			st.Status = ActionStatus.SUCCESS;
			// Set the entity ID.
			string entityID = su.getCESEntityId();

			OTProtect protect = su.getProtect();
			// Unprotect the receipt image id.
			string clearReceiptImageId = null;
			try
			{

				// TravelManager imageIds are not encrypted
				if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
				{
					clearReceiptImageId = protReceiptImageId;
				}
				else
				{
					clearReceiptImageId = protect.UnprotectValue(RECEIPT_IMAGE_ID_PROTECT_KEY, protReceiptImageId);
				}
			}
			catch (ArgumentException /* argExc */ )
			{
				// TODO: set localized error message.
				// TODO: log failure.
				throw new HttpException(400, "Bad Request - invalid receipt image id.");
			}
			// Unprotect the report key.
			string clearRptKey = null;
			try
			{
				// TravelManager reportKeys are not encrypted
				if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
				{
					clearRptKey = protRptKey;
				}
				else
				{
					clearRptKey = protect.UnprotectValue("RptKey", protRptKey);
				}
			}
			catch (ArgumentException /* argExc */)
			{
				// TODO: set localized error message.
				// TODO: log failure.
				throw new HttpException(400, "Bad Request - invalid report key.");
			}

			// Unprotect the report entry key.
			string clearRptEntKey = null;
			try
			{
				// TravelManager reportKeys are not encrypted
				if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
				{
					clearRptEntKey = protRptEntKey;
				}
				else
				{
					clearRptEntKey = protect.UnprotectValue("RpeKey", protRptEntKey);
				}
			}
			catch (ArgumentException /* argExc */)
			{
				// TODO: set localized error message.
				// TODO: log failure.
				throw new HttpException(400, "Bad Request - invalid report entry key.");
			}


			string reportId;
			int icKey;
			int polKey;
			object[] keys = Report.getReportKeyInfo(clearRptKey, su);
			polKey = (int)keys[Report.IDX_RPT_KEY_INFO_POL_KEY];
			// Travel Manager is passing in the unencrypted report id
			if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
			{
				reportId = clearRptKey;
				icKey = Snowbird.Controllers.GovTravelManagerController.getTMICKey();
			}
			else
			{
				// Obtain the report id based on the report key.
				reportId = (string)keys[Report.IDX_RPT_KEY_INFO_RPT_ID];
				// Set the IC Key.
				icKey = GetPolicyICKey(su, polKey);
			}


			if (icKey != -1)
			{
				try
				{
					if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
					{
						// NOTE: Travel Manager users do not use the Mid-Tier calls to associate receipt image ID's with line items.
						AssociateImageRequest request = new AssociateImageRequest(entityID, icKey, GetImagingUrl(su), GetImagingKey(su), GetIncludeSource(su), clearReceiptImageId, reportId, GetImagingSource(su));
						AssociateImageReply reply = (AssociateImageReply)request.process();
						if (!reply.isSuccess())
						{
							st.Status = ActionStatus.FAILURE;
							st.ErrorMessage = reply.getStatusMessage();
							// TODO: set localized error message.
							String errorMsg = "AssociateImageRequest failed: ";
							if (!string.IsNullOrEmpty(st.ErrorMessage))
							{
								errorMsg += st.ErrorMessage;
							}
							Exception e = new Exception(errorMsg);
							Logger.LogError(e, true, "Snowbird - AddLineItemReceiptImageId failed.");
						}
						else
						{
							// Set the receipt image ID on the report entry.
							Object[] parm = new object[] { clearRptEntKey, clearReceiptImageId };
							DBUtils.ReturnCode("CE_EXPENSE_SetLineItemImage", su.getCESEntityConnection(), parm);

							// Flip the flag that a receipt image is available on the report.
							parm = new object[] { reportId };
							DBUtils.ReturnCode("CE_IMAGING_SetReceiptImageAvailable", su.getCESEntityConnection(), parm);
							try
							{
								ApproveReject.MarkUploadReportReceipt(su, clearRptKey, su.getCurrentUserEmpKey());
							}
							finally { }
						}
					}
					else
					{
						AssociateLineItemImageMTRequest request = new AssociateLineItemImageMTRequest(entityID, icKey, GetImagingUrl(su), GetImagingKey(su), GetIncludeSource(su),
							reportId, clearRptEntKey, clearReceiptImageId, polKey, su.getCurrentUserEmpKey(), GetImagingSource(su), GetRunAuditRules(su));
						AssociateLineItemImageMTReply reply = (AssociateLineItemImageMTReply)request.process();
						if (!reply.isSuccess())
						{
							st.Status = ActionStatus.FAILURE;
							st.ErrorMessage = reply.getStatusMessage();
							// TODO: set localized error message.
							String errorMsg = "AssociateLineItemImageMTRequest failed: ";
							if (!string.IsNullOrEmpty(st.ErrorMessage))
							{
								errorMsg += st.ErrorMessage;
							}
							Exception e = new Exception(errorMsg);
							Logger.LogError(e, true, "Snowbird - AddLineItemReceiptImageId failed.");
						}
						else
						{
							try
							{
								ApproveReject.MarkUploadReportReceipt(su, clearRptKey, su.getCurrentUserEmpKey());
							}
							finally { }
						}
					}
				}
				catch (Exception e)
				{
					st.Status = ActionStatus.FAILURE;
					// TODO: set localized error message.
					Logger.LogError(e, true, "Snowbird - AddLineItemReceiptImageId failed.");
				}
			}
			else
			{
				throw new HttpException(500, "Imaging configuration not available for policy.");
			}

			return st;
		}

		/// <summary>
		/// Deletes Multiple Receipt images.
		/// </summary>
		/// <param name="su">SessionUtil</param>
		/// <param name="base64EncryptedImageIds">Encoded ReceiptImageIds</param>
		/// <returns>ActionStatus indicating the result of operation.</returns>
		public static ActionStatus DeleteMultipleReceiptImageIds(ExpenseSessionUtils su, string[] base64EncryptedImageIds)
		{
			ActionStatus st = new ActionStatus();
			st.Status = ActionStatus.SUCCESS;
			// Set the entity ID.
			string entityID = su.getCESEntityId();

			OTProtect protect = su.getProtect();
			// Unprotect the receipt image id.
			List<string> clearReceiptImageIds = new List<string>();

			//Need to decode and get the clearImageIds.
			//ImageIds are no longer than 20 or 32 bit. From iOS client we get encoded version of Id.
			//It needs to be decoded.
			try
			{
				foreach (String protectedId in base64EncryptedImageIds)
				{
					clearReceiptImageIds.Add(protect.UnprotectValue(RECEIPT_IMAGE_ID_PROTECT_KEY, protectedId));
				}
			}
			catch (ArgumentException /* argExc */)
			{
				// TODO: set localized error message.
				// TODO: log failure.
				throw new HttpException(400, "Bad Request - invalid receipt image id.");
			}

			// Set the IC Key.
			int icKey = GetICKeyUserDefault(su);
			if (icKey == -1)
			{
				throw new HttpException(500, "Imaging Configuration Not Available.");
			}
			try
			{
				DeleteMultipleLineItemImagesRequest request = new DeleteMultipleLineItemImagesRequest(entityID, icKey, GetImagingUrl(su), GetImagingKey(su), GetIncludeSource(su), clearReceiptImageIds, GetImagingSource(su));
				DeleteMultipleLineItemImagesReply reply = (DeleteMultipleLineItemImagesReply)request.process();
				if (!reply.isSuccess())
				{
					st.Status = ActionStatus.FAILURE;
					st.ErrorMessage = reply.getStatusMessage();
					// TODO: set localized error message.
					String errorMsg = "DeleteLineImageRequest failed: ";
					if (!string.IsNullOrEmpty(st.ErrorMessage))
					{
						errorMsg += st.ErrorMessage;
					}
					Exception e = new Exception(errorMsg);
					Logger.LogError(e, true, "Snowbird - DeleteMultipleReceiptImageIds failed.");
				}
			}
			catch (Exception e)
			{
				st.Status = ActionStatus.FAILURE;
				// TODO: set localized error message.
				Logger.LogError(e, true, "Snowbird - DeleteMultipleReceiptImageIds failed.");
			}
			return st;
		}


		/**
		 * <summary>
		 * Will delete a receipt image within the receipt store.
		 * </summary>
		 * <param name="su">an expense session utils object.</param>
		 * <param name="protFromReceiptImageId">a protected receipt image id.</param>
		 * <returns>
		 *      an instance of ActionStatus indicating success or failure.
		 * </returns>
		 */
		public static ActionStatus DeleteReceiptImageId(ExpenseSessionUtils su, string protReceiptImageId)
		{
			ActionStatus st = new ActionStatus();
			st.Status = ActionStatus.SUCCESS;
			// Set the entity ID.
			string entityID = su.getCESEntityId();

			OTProtect protect = su.getProtect();
			// Unprotect the receipt image id.
			string clearReceiptImageId = null;
			try
			{
				// TravelManager imageIds are not encrypted
				if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
				{
					clearReceiptImageId = protReceiptImageId;
				}
				else
				{
					clearReceiptImageId = protect.UnprotectValue(RECEIPT_IMAGE_ID_PROTECT_KEY, protReceiptImageId);
				}
			}
			catch (ArgumentException /* argExc */)
			{
				// TODO: set localized error message.
				// TODO: log failure.
				throw new HttpException(400, "Bad Request - invalid receipt image id.");
			}

			// Set the IC Key.
			int icKey = GetICKeyUserDefault(su);
			if (icKey == -1)
			{
				throw new HttpException(500, "Imaging Configuration Not Available.");
			}
			try
			{
				DeleteLineItemImageRequest request = new DeleteLineItemImageRequest(entityID, icKey, GetImagingUrl(su), GetImagingKey(su), GetIncludeSource(su), clearReceiptImageId, GetImagingSource(su));
				DeleteLineItemImageReply reply = (DeleteLineItemImageReply)request.process();
				if (!reply.isSuccess())
				{
					st.Status = ActionStatus.FAILURE;
					st.ErrorMessage = reply.getStatusMessage();
					// TODO: set localized error message.
					String errorMsg = "DeleteLineImageRequest failed: ";
					if (!string.IsNullOrEmpty(st.ErrorMessage))
					{
						errorMsg += st.ErrorMessage;
					}
					Exception e = new Exception(errorMsg);
					Logger.LogError(e, true, "Snowbird - DeleteReceiptImageId failed.");
				}
			}
			catch (Exception e)
			{
				st.Status = ActionStatus.FAILURE;
				// TODO: set localized error message.
				Logger.LogError(e, true, "Snowbird - DeleteReceiptImageId failed.");
			}
			return st;
		}

		/**
		* <summary>
		* Will append a receipt image within the receipt store to a line item image contained
		* within a report entry.
		* </summary>
		* <param name="su">an expense session utils object.</param>
		* <param name="protFromReceiptImageId">Contains the protected receipt image id to be appended.</param>
		* <param name="protToReceiptImageId">Contains the protected receipt image id to be appended to.</param>
		* <returns>
		*      an instance of ActionStatus indicating success or failure.
		* </returns>
		*/
		public static ActionStatus AppendReceiptImageId(ExpenseSessionUtils su, string protFromReceiptImageId, string protToReceiptImageId)
		{
			ActionStatus st = new ActionStatus();
			st.Status = ActionStatus.SUCCESS;
			// Set the entity ID.
			string entityID = su.getCESEntityId();

			OTProtect protect = su.getProtect();
			// Unprotect the from receipt image id.
			string clearFromReceiptImageId = null;
			try
			{
				// TravelManager imageIds are not encrypted
				if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
				{
					clearFromReceiptImageId = protFromReceiptImageId;
				}
				else
				{
					clearFromReceiptImageId = protect.UnprotectValue(RECEIPT_IMAGE_ID_PROTECT_KEY, protFromReceiptImageId);
				}
			}
			catch (ArgumentException /* argExc */)
			{
				// TODO: set localized error message.
				// TODO: log failure.
				throw new HttpException(400, "Bad Request - invalid 'FromReceiptImageId' receipt image id.");
			}

			// Unprotect the to receipt image id.
			string clearToReceiptImageId = null;
			try
			{
				// TravelManager imageIds are not encrypted
				if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
				{
					clearToReceiptImageId = protToReceiptImageId;
				}
				else
				{
					clearToReceiptImageId = protect.UnprotectValue(RECEIPT_IMAGE_ID_PROTECT_KEY, protToReceiptImageId);
				}
			}
			catch (ArgumentException /* argExc */)
			{
				// TODO: set localized error message.
				// TODO: log failure.
				throw new HttpException(400, "Bad Request - invalid 'ToReceiptImageId' receipt image id.");
			}


			// Set the IC Key.
			int icKey = GetICKeyUserDefault(su);
			if (icKey == -1)
			{
				throw new HttpException(500, "Imaging Configuration Not Available.");
			}
			try
			{
				AppendLineItemImageRequest request = new AppendLineItemImageRequest(entityID, icKey, GetImagingUrl(su), GetImagingKey(su), GetIncludeSource(su), clearFromReceiptImageId, clearToReceiptImageId, GetImagingSource(su));
				AppendLineItemImageReply reply = (AppendLineItemImageReply)request.process();
				if (!reply.isSuccess())
				{
					st.Status = ActionStatus.FAILURE;
					st.ErrorMessage = reply.getStatusMessage();
					// TODO: set localized error message.
					String errorMsg = "AppendLineItemImageRequest failed: ";
					if (!string.IsNullOrEmpty(st.ErrorMessage))
					{
						errorMsg += st.ErrorMessage;
					}
					Exception e = new Exception(errorMsg);
					Logger.LogError(e, true, "Snowbird - AppendReceiptImageId failed.");
				}
			}
			catch (Exception e)
			{
				st.Status = ActionStatus.FAILURE;
				// TODO: set localized error message.
				Logger.LogError(e, true, "Snowbird - AppendReceiptImageId failed.");
			}
			return st;
		}


		/**
		 * <summary>
		 * Clears the receipt image ID associated with a report line item.
		 * </summary>
		 * 
		 * <param name="su">an ExpenseSessionUtils object.</param>
		 * <param name="reportId">contains the report id.</param>
		 * <param name="rpeKey">the report entry key.</param>
		 * <param name="receiptImageId">the unprotected report entry receipt image id</param>
		 * <param name="polKey">the report policy key</param>
		 */
		public static ActionStatus ClearLineItemReceiptImageId(ExpenseSessionUtils su, string reportId, string rpeKey, string receiptImageId, int polKey)
		{
			ActionStatus st = new ActionStatus();
			st.Status = ActionStatus.SUCCESS;
			// Set the entity ID.
			string entityID = su.getCESEntityId();
			// Set the IC Key.
			int icKey = GetPolicyICKey(su, polKey);
			if (icKey != -1)
			{
				ReportLineItemImageInfo[] infos = new ReportLineItemImageInfo[1];
				infos[0] = new ReportLineItemImageInfo(receiptImageId, rpeKey);
				DisassociateLineItemImageRequest request = new DisassociateLineItemImageRequest(entityID, icKey, GetImagingUrl(su), GetImagingKey(su), GetIncludeSource(su), reportId, infos, polKey, GetImagingSource(su));
				DisassociateLineItemImageReply reply = (DisassociateLineItemImageReply)request.process();
				if (reply.isSuccess())
				{
					st.Status = ActionStatus.SUCCESS;
				}
				else
				{
					st.Status = ActionStatus.FAILURE;
					st.ErrorMessage = reply.getStatusMessage();
					String errorMsg = "DisassociateLineItemImageRequest failed: ";
					if (!string.IsNullOrEmpty(st.ErrorMessage))
					{
						errorMsg += st.ErrorMessage;
					}
					Exception e = new Exception(errorMsg);
					Logger.LogError(e, true, "Snowbird - ClearLineItemReceiptImageId failed.");
				}
			}
			else
			{
				st.Status = ActionStatus.FAILURE;
				st.ErrorMessage = "Imaging configuration not available for policy.";
			}
			return st;
		}

		/**
		 * Will retrieve the actual bytes of the image stored in the Receipt Store or 'null' if
		 * unable.
		 */
		public static byte[] GetReceiptImageBytes(ExpenseSessionUtils su, string receiptImageId)
		{
			byte[] res = null;

			try
			{
				GetReceiptImageUrlStatus replyStatus = GetReceiptImageUrl(su, receiptImageId);
				if (replyStatus.Status == ActionStatus.SUCCESS)
				{
					HttpStatusCode statusCode;
					res = HttpUtils.MakeHTTPRequest("GET", replyStatus.ReceiptImageURL, null, null, null, null, 60, out statusCode);
				}
			}
			catch (Exception /* exc */)
			{
				// TODO: handle exception.
			}
			return res;
		}

		/**
		* <summary>
		* Gets the list of receipt meta-data for an end-user based on a filtering parameter.
		* 
		* This method will return receipts that:
		* 1. Are not referenced by mobile entries or receipt captures that have a status of "A_DONE" or "M_DONE".
		* 2. Do not have values of 'ImageOrigin' equal to 'CARD' or 'ERECEIPT'.
		* 3. Can have 'ImageOrigin' values of 'OCR', this includes any outstanding receipt uploaded via ExpenseIt or emailed to
		*    'receipts@expenseit.com'.
		* </summary>
		* 
		* <param name="su">an ExpenseSessionUtils object.</param>
		* <param name="filterMobileExpense">whether receipts referenced by mobile entries should be filtered out.</param>
		*/
		public static GetReceiptImageUrlsStatus GetReceiptImageUrlsStatusForOCR(ExpenseSessionUtils su, Boolean filterMobileExpense)
		{
			if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
			{
				GetReceiptImageUrlsStatus retVal = GetReceiptImageUrls(su, filterMobileExpense, true);
				if (retVal.Status == ActionStatus.FAILURE)
				{
					String errorMsg = "GetReceiptImageUrls: call to imaging server directly failed!";
					Exception e = new Exception(errorMsg);
					Logger.LogError(e, true, "Snowbird - GetReceiptImageUrlsStatusForOCR.");
				}
				else
				{
					// Filter out receipts with an image origin of 'CARD' or 'ERECEIPT'.
					filterCardEReceiptImageInfos(retVal);
				}
				return retVal;
			}
			else
			{
				// Attempt to call the mid-tier.
				GetReceiptImageUrlsStatus retVal = GetReceiptImageUrlsMT(su, filterMobileExpense, WhatToGet.None);
				if (retVal.Status == ActionStatus.FAILURE)
				{
					String errorMsg = "GetReceiptImageUrls: call to mid-tier failed, calling imaging server directly...";
					retVal = GetReceiptImageUrls(su, filterMobileExpense, true);
					if (retVal.Status == ActionStatus.FAILURE)
					{
						errorMsg += "call failed!";
					}
					else
					{
						// Filter out receipts with an image origin of 'CARD' or 'ERECEIPT'.
						filterCardEReceiptImageInfos(retVal);

						errorMsg += "call succeeded!";
					}
					Exception e = new Exception(errorMsg);
					Logger.LogError(e, true, "Snowbird - GetReceiptImageUrlsStatusForOCR.");
				}
				else
				{
					if (retVal.ReceiptInfos.Length > 0)
					{
						// First, filter out receipts with image origin values of "CARD" or "ERECEIPT".
						filterCardEReceiptImageInfos(retVal);

						// Second, filter out any receipts referenced by a receipt capture with a status of "A_DONE" or "M_DONE".
						filterDoneReceiptCaptures(su, retVal);
					}
				}
				return retVal;
			}
		}

		/**
		* <summary>
		* Will filter out any receipts referenced by a receipt capture item with either a status of "A_DONE" or "M_DONE".
		* </summary>
		* 
		* <param name="su">retVal - the status object </param>
		*/
		private static void filterDoneReceiptCaptures(ExpenseSessionUtils su, GetReceiptImageUrlsStatus rcpts)
		{
			// Build a map of receipt image ID's to instances of 'ReceiptCapture'.
			Dictionary<String, ReceiptCapture> capDict = new Dictionary<string, ReceiptCapture>();
			ReceiptCapture[] receiptCaptures = ReceiptCapture.GetReceiptCaptureList(su, 'Y');
			if (receiptCaptures != null && receiptCaptures.Length > 0)
			{
				OTProtect protect = su.getProtect();
				foreach (ReceiptCapture recCap in receiptCaptures)
				{
					if (!String.IsNullOrEmpty(recCap.ReceiptImageId))
					{
						String clearRcptId = protect.UnprotectValue(RECEIPT_IMAGE_ID_PROTECT_KEY, recCap.ReceiptImageId);
						if (!capDict.ContainsKey(clearRcptId))
						{
							capDict.Add(clearRcptId, recCap);
						}
					}
				}
			}

			List<ReceiptImageInfo> filteredItems = new List<ReceiptImageInfo>();
			foreach (var rcptInfo in rcpts.ReceiptInfos)
			{
				if (!String.IsNullOrEmpty(rcptInfo.ReceiptImageId))
				{
					if (capDict.ContainsKey(rcptInfo.ReceiptImageId))
					{
						ReceiptCapture recCap = capDict[rcptInfo.ReceiptImageId];
						if (!String.IsNullOrEmpty(recCap.StatKey))
						{
							if (!(string.Equals("M_DONE", recCap.StatKey, StringComparison.CurrentCultureIgnoreCase) ||
								 string.Equals("A_DONE", recCap.StatKey, StringComparison.CurrentCultureIgnoreCase)))
							{
								filteredItems.Add(rcptInfo);
							}
						}
						else
						{
							filteredItems.Add(rcptInfo);
						}
					}
					else
					{
						filteredItems.Add(rcptInfo);
					}
				}
			}
			// Assign the resulting array.
			rcpts.ReceiptInfos = filteredItems.ToArray();
		}

		/**
		* <summary>
		* Will filter out any receipts with an ImageOrigin value of "CARD" or "ERECEIPT".
		* </summary>
		* 
		* <param name="su">retVal - the status object </param>
		*/
		private static void filterCardEReceiptImageInfos(GetReceiptImageUrlsStatus rcpts)
		{
			if (rcpts != null && rcpts.ReceiptInfos.Length > 0)
			{
				List<ReceiptImageInfo> filteredItems = new List<ReceiptImageInfo>();
				foreach (var rcptInfo in rcpts.ReceiptInfos)
				{
					if (!String.IsNullOrEmpty(rcptInfo.ImageOrigin))
					{
						if (!(string.Equals(rcptInfo.ImageOrigin, "CARD", StringComparison.CurrentCultureIgnoreCase) ||
							string.Equals(rcptInfo.ImageOrigin, "ERECEIPT", StringComparison.CurrentCultureIgnoreCase)))
						{
							filteredItems.Add(rcptInfo);
						}
					}
					else
					{
						// If ImaginOrigin is null or empty, then add it to the resulting list.
						filteredItems.Add(rcptInfo);
					}
				}

				// Assign the resulting array.
				rcpts.ReceiptInfos = filteredItems.ToArray();
			}
		}

		/**
		* <summary>
		* Gets the list of receipt meta-data for an end-user based on a filtering parameter.
		* 
		* This method will return receipts that:
		* 1. Are not referenced by mobile entries or receipt captures.
		* 2. Do not have values of 'ImageOrigin' equal to 'CARD', 'ERECEIPT' or OCR.
		* </summary>
		* 
		* <param name="su">an ExpenseSessionUtils object.</param>
		* <param name="filterMobileExpense">whether receipts referenced by mobile entries should be filtered out.</param>
		*/
		public static GetReceiptImageUrlsStatus GetReceiptImageUrlsStatus(ExpenseSessionUtils su, Boolean filterMobileExpense)
		{
			if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
			{
				return GetReceiptImageUrls(su, filterMobileExpense, false);
			}
			else
			{
				GetReceiptImageUrlsStatus retVal = GetReceiptImageUrls(su, filterMobileExpense, false);

				if (retVal.Status == ActionStatus.FAILURE)
				{
					Logger.LogError(new Exception(retVal.ErrorMessage), true, "Snowbird - GetReceiptImageUrls.");
				}

				return retVal;
			}
		}

		/**
		* <summary>
		* Gets the report pdf url, or empty string if no images available
		* </summary>
		* 
		* <param name="su">an ExpenseSessionUtils object.</param>
		* <param name="reportId">Report id </param>
		* <param name="polKey">Policy Key"</param>
		*/
		public static GetReportPdfUrlStatus GetReportPdfUrlStatus(ExpenseSessionUtils su, string reportId, string polKey)
		{



			GetReportPdfUrlStatus retVal = GetReportPdfUrl(su, reportId, polKey);

			if (retVal.Status == ActionStatus.FAILURE)
			{
				Logger.LogError(new Exception(retVal.ErrorMessage), true, "Snowbird - GetReportPdfUr.");
			}

			return retVal;


		}
		/**
		* <summary>
		* Gets the report pdf url or empty string if no imag(s) have been added
		* </summary>
		* 
		* <param name="su">an ExpenseSessionUtils object.</param>
		* <param name="reportId">Report id </param>
		* <param name="polKey">Policy Key"</param>
		*/
		private static GetReportPdfUrlStatus GetReportPdfUrl(ExpenseSessionUtils su, string reportId, string polKey)
		{

			int unProtectPolKey = Convert.ToInt32(su.getProtect().UnprotectNonBlankValue("PolKey", polKey));

			GetReportPdfUrlStatus getReportPdfUrlStatus = new GetReportPdfUrlStatus();
			getReportPdfUrlStatus.Status = ActionStatus.SUCCESS;
			getReportPdfUrlStatus.PdfURL = "";

			string actionXML = "<GetReceiptImages>" +
				"<RptID>" + reportId + "</RptID>" +
				"<PolKey>" + polKey + "</PolKey>" +
				"</GetReceiptImages>";

			XmlDocument xmlRequest = new XmlDocument();
			xmlRequest.LoadXml(actionXML.ToString());

			xmlRequest = CESMidtier.WrapMidtierRequest(xmlRequest);
			XmlDocument res = CESMidtier.MakeMidtierRequest("GetReceiptImages", xmlRequest, 60);
			su.processMidtierError(xmlRequest, res, true);

			XmlNode fileUrlNode = res.SelectSingleNode("/Response/Body/FileURL");
			if (fileUrlNode != null && !string.IsNullOrEmpty(fileUrlNode.InnerText))
			{
				getReportPdfUrlStatus.PdfURL = fileUrlNode.InnerText;
			}

			return getReportPdfUrlStatus;

		}
		/**
		* <summary>
		* Gets the list of receipt meta-data for an end-user based on two filtering parameters.
		* </summary>
		* 
		* <param name="su">an ExpenseSessionUtils object.</param>
		* <param name="filterMobileExpense">whether receipts referenced by mobile entries should be filtered out.</param>
		* <param name="includeNotDoneReceiptCaptures">whether receipts referenced by receipt capture items with a status other than 'M_DONE' or 'A_DONE' will be filtered out.
		*                                             If 'true', then receipts referenced by a receipt capture with a "non-DONE" status will be returned; otherwise any 
		*                                            receipt referenced by a receipt capture will be filtered out.</param>
		*/
		private static GetReceiptImageUrlsStatus GetReceiptImageUrls(ExpenseSessionUtils su, Boolean filterMobileExpense, Boolean includeNotDoneReceiptCaptures)
		{
			GetReceiptImageUrlsStatus st = new GetReceiptImageUrlsStatus();
			st.Status = ActionStatus.SUCCESS;
			// Set the entity ID.
			string entityID = su.getCESEntityId();
			// Set the IC Key.
			int icKey = GetICKeyUserDefault(su);
			if (icKey == -1)
			{
				throw new HttpException(500, "Imaging Configuration Not Available.");
			}
			// Set the category ID to the current user id.
			string categoryId = su.getCurrentUserId();
			try
			{
				GetLineItemImageListRequest request = new GetLineItemImageListRequest(entityID, icKey, GetImagingUrl(su), GetImagingKey(su), GetIncludeSource(su), categoryId, GetImagingSource(su));
				GetLineItemImageListReply reply = (GetLineItemImageListReply)request.process();
				if (reply.isSuccess())
				{
					if (reply.getImageInfos() != null)
					{
						OTProtect protect = su.getProtect();

						// Build a map of receipt image ID's to instances of 'ReceiptCapture'.
						Dictionary<String, ReceiptCapture> capDict = new Dictionary<string, ReceiptCapture>();
						ReceiptCapture[] receiptCaptures = ReceiptCapture.GetReceiptCaptureList(su, 'Y');
						if (receiptCaptures != null && receiptCaptures.Length > 0)
						{
							foreach (ReceiptCapture recCap in receiptCaptures)
							{
								if (!String.IsNullOrEmpty(recCap.ReceiptImageId))
								{
									String clearRcptId = protect.UnprotectValue(RECEIPT_IMAGE_ID_PROTECT_KEY, recCap.ReceiptImageId);
									if (!capDict.ContainsKey(clearRcptId))
									{
										capDict.Add(clearRcptId, recCap);
									}
								}
							}
						}

						List<ReceiptImageInfo> imageInfos = new List<ReceiptImageInfo>();
						for (int imgInfoInd = 0; imgInfoInd < reply.getImageInfos().Length; ++imgInfoInd)
						{
							LineItemImageInfo lineItemImageInfo = reply.getImageInfos()[imgInfoInd];
							if (lineItemImageInfo.ReceiptImageId != null)
							{
								// 11/3/2011 (AVK): As of the November 2011 release, any receipt referenced by an unassigned mobile entry will not be
								// returned in the list.
								// MOB-14771 - 9/12/2013 (AVK): As of the September 2013 release, any receipt referenced by an entry in the receipt
								//                              capture table that is either pending, or completed in the OCR process will be filtered
								//                              out of this list.
								// MOB-21160 - 11/17/14 - There is now a boolean parameter 'includeNotDoneReceiptCaptures' that controls whether receipts
								//                        referenced by receipt captures that don't have a value of both 'A_DONE' or 'M_DONE' are included.
								//                        Callers that pass a value of 'false' for this will result in all receipts referenced by receipt capture
								//                        items being filtered out.  However, passing a value of 'true' will permit "non-DONE" receipt captures
								//                        to be included.


								// Explicit Filtering of receipts referenced by mobile entries is not required any more because those receipts have different category on it.
								// Receipt capture reference check.
								if (capDict.ContainsKey(lineItemImageInfo.ReceiptImageId))
								{
									// Check whether 'includeNotDoneReceiptCaptures' is 'true' and 'recCap' has a status of neither "A_DONE" | "M_DONE".
									ReceiptCapture recCap = capDict[lineItemImageInfo.ReceiptImageId];
									if (recCap != null && includeNotDoneReceiptCaptures && !(string.Equals("M_DONE", recCap.StatKey, StringComparison.CurrentCultureIgnoreCase) ||
													 string.Equals("A_DONE", recCap.StatKey, StringComparison.CurrentCultureIgnoreCase)))
									{
										JsonTimeStamp jsonTimeStamp = new JsonTimeStamp();
										if (!string.IsNullOrWhiteSpace(lineItemImageInfo.TimeStamp))
										{
											jsonTimeStamp = new JavaScriptSerializer().Deserialize<JsonTimeStamp>(lineItemImageInfo.TimeStamp);
										}
										// Include receipt referenced by a "non-DONE" receipt capture item.
										imageInfos.Add(new ReceiptImageInfo(protect, lineItemImageInfo.ReceiptImageId,
												lineItemImageInfo.FileType, lineItemImageInfo.ImageDate, lineItemImageInfo.FileName, lineItemImageInfo.SystemOrigin,
												lineItemImageInfo.ImageOrigin, lineItemImageInfo.ImageUrl, lineItemImageInfo.ThumbUrl, new TimeStamp(jsonTimeStamp)));
									}
								}
								else
								{
									JsonTimeStamp jsonTimeStamp = new JsonTimeStamp();
									if (!string.IsNullOrWhiteSpace(lineItemImageInfo.TimeStamp))
									{
										jsonTimeStamp = new JavaScriptSerializer().Deserialize<JsonTimeStamp>(lineItemImageInfo.TimeStamp);
									}

									// Receipt is not referenced by either mobile entry or receipt capture.
									imageInfos.Add(new ReceiptImageInfo(protect, lineItemImageInfo.ReceiptImageId,
											lineItemImageInfo.FileType, lineItemImageInfo.ImageDate, lineItemImageInfo.FileName, lineItemImageInfo.SystemOrigin,
											lineItemImageInfo.ImageOrigin, lineItemImageInfo.ImageUrl, lineItemImageInfo.ThumbUrl, new TimeStamp(jsonTimeStamp)));
								}
							}
						}
						st.ReceiptInfos = imageInfos.ToArray();
					}
				}
				else
				{
					st.Status = ActionStatus.FAILURE;
					st.ErrorMessage = reply.getStatusMessage();
					// TODO: set localized error message.
					String errorMsg = "GetLineItemImageListRequest failed: ";
					if (!string.IsNullOrEmpty(st.ErrorMessage))
					{
						errorMsg += st.ErrorMessage;
					}
					Exception e = new Exception(errorMsg);
					Logger.LogError(e, true, "Snowbird - GetReceiptImageUrls failed.");
				}
			}
			catch (Exception e)
			{
				st.Status = ActionStatus.FAILURE;
				// TODO: set localized error message.
				Logger.LogError(e, true, "Snowbird - GetReceiptImageUrls failed.");
			}
			return st;
		}

		private static GetReceiptImageUrlsStatus GetReceiptImageUrlsMT(ExpenseSessionUtils su, Boolean filterMobileExpense, WhatToGet whatToGet)
		{
			GetReceiptImageUrlsStatus st = new GetReceiptImageUrlsStatus();
			st.Status = ActionStatus.SUCCESS;
			// Set the entity ID.
			string entityID = su.getCESEntityId();
			// Set the IC Key.
			int icKey = GetICKeyUserDefault(su);
			if (icKey == -1)
			{
				throw new HttpException(500, "Imaging Configuration Not Available.");
			}

			// Set the category ID to the current user id.
			string categoryId = su.getCurrentUserId();
			try
			{
				GetLineItemImageListMTRequest request = new GetLineItemImageListMTRequest(entityID, icKey, GetImagingUrl(su), GetImagingKey(su), GetIncludeSource(su), categoryId, GetImagingSource(su),
					su.getCurrentUserEmpKey(), su.getCurrentUserId(), whatToGet);
				GetLineItemImageListMTReply reply = (GetLineItemImageListMTReply)request.process();
				if (reply.isSuccess())
				{
					if (reply.getImageInfos() != null)
					{
						OTProtect protect = su.getProtect();
						List<ReceiptImageInfo> imageInfos = new List<ReceiptImageInfo>();
						for (int imgInfoInd = 0; imgInfoInd < reply.getImageInfos().Length; ++imgInfoInd)
						{
							LineItemImageMTInfo lineItemImageInfo = reply.getImageInfos()[imgInfoInd];
							if (lineItemImageInfo.ReceiptImageId != null)
							{
								// AVK - as of 10/31/2013 - this code is relying upon the mid-tier's request to properly filter out receipts
								//       that should not be displayed in the receipt store.
								imageInfos.Add(new ReceiptImageInfo(protect, lineItemImageInfo.ReceiptImageId,
									lineItemImageInfo.FileType, lineItemImageInfo.ImageDate, lineItemImageInfo.FileName, lineItemImageInfo.SystemOrigin,
									lineItemImageInfo.ImageOrigin, lineItemImageInfo.ImageUrl, lineItemImageInfo.ThumbUrl, lineItemImageInfo.TimeStamp));
							}
						}
						st.ReceiptInfos = imageInfos.ToArray();
					}
				}
				else
				{
					st.Status = ActionStatus.FAILURE;
					st.ErrorMessage = reply.getStatusMessage();
					// TODO: set localized error message.
					String errorMsg = "GetLineItemImageListMTRequest failed: ";
					if (!string.IsNullOrEmpty(st.ErrorMessage))
					{
						errorMsg += st.ErrorMessage;
					}
					Exception e = new Exception(errorMsg);
					Logger.LogError(e, true, "Snowbird - GetReceiptImageUrls failed.");
				}
			}
			catch (Exception e)
			{
				st.Status = ActionStatus.FAILURE;
				// TODO: set localized error message.
				Logger.LogError(e, true, "Snowbird - GetReceiptImageUrls failed.");
			}
			return st;
		}

		public static GetReceiptImageUrlStatus GetReceiptImageUrl(ExpenseSessionUtils su, string receiptImageId)
		{
			GetReceiptImageUrlStatus st = new GetReceiptImageUrlStatus();
			st.Status = ActionStatus.SUCCESS;
			// Set the entity ID.
			string entityID = su.getCESEntityId();
			// Set the IC Key.
			int icKey = GetICKeyUserDefault(su);
			if (icKey == -1)
			{
				throw new HttpException(500, "Imaging Configuration Not Available.");
			}
			try
			{
				string unprotectedReceiptImageId = null;
				try
				{
					// TravelManager imageIds are not encrypted
					if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
					{
						unprotectedReceiptImageId = receiptImageId;
					}
					else
					{
						OTProtect protect = su.getProtect();
						unprotectedReceiptImageId =
							protect.UnprotectValue(RECEIPT_IMAGE_ID_PROTECT_KEY, receiptImageId);
					}

					st.ReceiptImageIdUnprotected = unprotectedReceiptImageId;
				}
				catch (ArgumentException /* argExc */)
				{
					throw new HttpException(400, "Bad Request - invalid receiptImageId path component.");
				}
				GetReceiptImageUrlRequest request = new GetReceiptImageUrlRequest(entityID, icKey, GetImagingUrl(su), GetImagingKey(su), GetIncludeSource(su), unprotectedReceiptImageId, GetImagingSource(su), su.getCurrentLoginName());
				GetReceiptImageUrlReply reply = (GetReceiptImageUrlReply)request.process();
				if (reply.isSuccess())
				{
					st.ReceiptImageURL = reply.getReceiptImageUrl();
					st.FileType = reply.getFileType();
					if (!String.IsNullOrEmpty(reply.getTimeStampData()))
					{
						var jsonTimeStamp = new JavaScriptSerializer().Deserialize<JsonTimeStamp>(reply.getTimeStampData());
						st.TimeStamp = new TimeStamp(jsonTimeStamp);
					}
				}
				else
				{
					st.Status = ActionStatus.FAILURE;
					st.ErrorMessage = reply.getStatusMessage();
					// TODO: set localized error message.
					String errorMsg = "GetReceiptImageUrlRequest failed: ";
					if (!string.IsNullOrEmpty(st.ErrorMessage))
					{
						errorMsg += st.ErrorMessage;
					}
					Exception e = new Exception(errorMsg);
					Logger.LogError(e, true, "Snowbird - GetReceiptImageUrl failed.");
				}
			}
			catch (Exception e)
			{
				st.Status = ActionStatus.FAILURE;
				// TODO: set localized error message.
				Logger.LogError(e, true, "Snowbird - GetReceiptImageUrl failed.");
			}
			return st;
		}



		public static UploadImageStatus UploadReceipt(ExpenseSessionUtils su, byte[] data, string contentType, string imageOrigin, string timeStamp)
		{
			UploadImageStatus st = new UploadImageStatus();
			st.Status = ActionStatus.SUCCESS;
			// Set up the necessary values to pass into the call to upload the receipt to the receipt store.

			// Set the entity ID.
			string entityID = su.getCESEntityId();
			// Set the IC Key.
			int icKey = GetICKeyUserDefault(su);
			if (icKey == -1)
			{
				throw new HttpException(500, "Imaging Configuration Not Available.");
			}
			//Set the category ID to the current user id.
			string categoryId = su.getCurrentUserId();


			WebServiceVersion ver = (WebServiceVersion)HttpContext.Current.Session["SaveReceiptVersion"];


			if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session) && ver ==
				WebServiceVersion.V2)
			{
				//Sending Category id as ssn+ "_" + tanum
				categoryId = (string)HttpContext.Current.Session["TraverlerId"] + "_" + (string)HttpContext.Current.Session["TaNum"];
			}

			// Set the file type.

			string fileType = contentType.ToLower();
			if (fileType.EndsWith("jpeg") || fileType.EndsWith("jpg"))
			{
				fileType = "JPG";
			}
			else if (fileType.EndsWith("png"))
			{
				fileType = "PNG";
			}
			else if (fileType.EndsWith("pdf"))
			{
				fileType = "PDF";
			}
			// Set the reference id.
			DateTime Epoch = new DateTime(1970, 1, 1);
			long currentTimeMillis = (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;

			string dateTime = DateTime.Now.ToString();
			string refId = Convert.ToDateTime(dateTime).ToString("MM-dd-yyyy h:mm:ss tt");

			// Base64 encode the content.
			string content = Convert.ToBase64String(data);

			PutLineItemImageReply reply = null;
			try
			{
				PutLineItemImageRequest request = new PutLineItemImageRequest(entityID, icKey, GetImagingUrl(su), GetImagingKey(su), GetIncludeSource(su), categoryId, fileType, imageOrigin, refId, content, GetImagingSource(su), timeStamp, su.getCurrentLoginName());
				reply = (PutLineItemImageReply)request.process();
				if (reply.isSuccess() || reply.getImageId().Length > 0)
				{
					// dont encrypt for TravelManager
					if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
					{
						st.ReceiptImageId = reply.getImageId();
					}
					else
					{
						OTProtect protect = su.getProtect();
						st.ReceiptImageId = protect.ProtectValue(RECEIPT_IMAGE_ID_PROTECT_KEY, reply.getImageId());
					}
				}
				else
				{
					st.Status = ActionStatus.FAILURE;
					st.ErrorMessage = reply.getStatusMessage();
					// TODO: set localized error message.
					String errorMsg = "PutLineItemImageRequest failed: ";
					if (!string.IsNullOrEmpty(st.ErrorMessage))
					{
						errorMsg += st.ErrorMessage;
					}
					Exception e = new Exception(errorMsg);
					Logger.LogError(e, true, "Snowbird - UploadReceipt failed.");
				}
			}
			catch (Exception e)
			{
				st.Status = ActionStatus.FAILURE;
				if (reply != null)
				{
					st.ErrorMessage = reply.getStatusMessage();
				}
				// TODO: set localized error message.
				Logger.LogError(e, true, "Snowbird - UploadReceipt failed.");
			}
			return st;
		}

		private static string GetImagingUrl(ExpenseSessionUtils su)
		{
			if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
			{
				return GovTravelManagerController.getTMImageURL(HttpContext.Current.Session);
			}
			else
			{
				return CESImaging.GetImagingUrl(su.getCESEntityId(), "EXPENSE") + "/concurws";
			}
		}

		private static string GetImagingKey(ExpenseSessionUtils su)
		{
			if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
			{
				return Snowbird.Controllers.GovTravelManagerController.getEncryptionKey();
			}

			return CESImaging.GetImagingKey();
		}

		private static string GetImagingSource(ExpenseSessionUtils su)
		{
			string imagingSource = IMAGING_SOURCE_CTE;
			if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
			{
				imagingSource = IMAGING_SOURCE_TM;
			}
			return imagingSource;
		}

		private static bool GetRunAuditRules(ExpenseSessionUtils su)
		{
			return true;
		}

		private static bool GetIncludeSource(ExpenseSessionUtils su)
		{
			return true;
		}


		/**
		 * Gets the first imaging configuration object located in the imaging configuration for
		 * the entity.
		 */
		private static int GetICKey(ExpenseSessionUtils su)
		{
			if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
			{
				return Snowbird.Controllers.GovTravelManagerController.getTMICKey();
			}

			object[] parm = new object[] { DBNull.Value, su.getCESLangCode() };
			System.Collections.ArrayList tmp = new System.Collections.ArrayList();
			DbDataReader dr = null;
			int icKey = -1;
			try
			{
				dr = DBUtils.ReturnDataReader("CE_IMAGING_GetImagingConfig", su.getCESEntityConnection(), parm);
				if (dr.Read())
				{
					if (dr["IC_KEY"] != DBNull.Value)
					{
						icKey = (int)dr["IC_KEY"];
					}
				}
			}
			finally
			{
				if (dr != null) dr.Close();
			}
			return icKey;
		}

		/**
		 * Gets the imaging configuration key based on the users default policy.
		 */
		private static int GetICKeyUserDefault(ExpenseSessionUtils su)
		{
			if (GovTravelManagerController.isTMEntity(HttpContext.Current.Session))
			{
				return Snowbird.Controllers.GovTravelManagerController.getTMICKey();
			}

			return GetPolicyICKey(su, Convert.ToInt32(su.GetDefaultPolicyKey()));
		}

		/**
		 * Gets the imaging configuration key based on the passed in policy key.
		 */
		private static int GetPolicyICKey(ExpenseSessionUtils su, int polKey)
		{
			int icKey = -1;
			Policies pols = new Policies(su);
			object polAttrObj = pols.GetPolicyAttribute(polKey, "IC_KEY");
			if (polAttrObj != null)
			{
				icKey = Convert.ToInt32(polAttrObj);
			}
			return icKey;
		}

	}
}
